using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.JSON;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Server.Auth.Data.Models;
using Server.Auth.Network;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server.Auth
{
    public class AuthManager
    {
        private readonly string Host;
        private readonly int Port;
        public readonly int ServerId;
        public ServerConfig Config;
        public Socket MainSocket;
        public bool ServerIsClosed;

        private readonly ConcurrentDictionary<string, int> ConnectionCounter = new ConcurrentDictionary<string, int>();
        private readonly int MaxConnectionsPerIP = 5;
        private readonly Timer ConnectionResetTimer;

        public AuthManager(int ServerId, string Host, int Port)
        {
            this.Host = Host;
            this.Port = Port;
            this.ServerId = ServerId;

            ConnectionResetTimer = new Timer(_ => ResetConnectionCounter(), null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        }
        public bool Start()
        {
            try
            {
                Config = ServerConfigJSON.GetConfig(ConfigLoader.ConfigId);
                MainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint Local = new IPEndPoint(IPAddress.Parse(Host), Port);
                MainSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                MainSocket.SetIPProtectionLevel(IPProtectionLevel.EdgeRestricted);
                MainSocket.Bind(Local);
                MainSocket.Listen(ConfigLoader.BackLog);
                Thread ThreadAuth = new Thread(Read)
                {
                    Priority = ThreadPriority.Highest
                };
                ThreadAuth.Start();
                CLogger.Print($"Auth Serv Address {Local.Address}:{Local.Port}", LoggerType.Info);
                return true;
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
                return false;
            }
        }
        private void Read()
        {
            try
            {
                MainSocket.BeginAccept(new AsyncCallback(AcceptCallback), MainSocket);
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
        }

        private void ResetConnectionCounter()
        {
            ConnectionCounter.Clear();
        }

        private bool IsIpAllowed(IPAddress ip)
        {
            string ipStr = ip.ToString();
            ConnectionCounter.AddOrUpdate(ipStr, 1, (key, old) => old + 1);
            return ConnectionCounter[ipStr] <= MaxConnectionsPerIP;
        }

        //private void AcceptCallback(IAsyncResult Result)
        //{
        //    if (ServerIsClosed)
        //    {
        //        return;
        //    }
        //    Socket ClientSocket = (Socket)Result.AsyncState;
        //    try
        //    {
        //        Socket Handler = ClientSocket.EndAccept(Result);
        //        if (Handler != null)
        //        {
        //            AuthClient Client = new AuthClient(ServerId, Handler);
        //            AddSocket(Client);
        //            if (Client == null)
        //            {
        //                CLogger.Print("Destroyed after failed to add to list.", LoggerType.Warning);
        //            }
        //            Thread.Sleep(5);
        //        }
        //    }
        //    catch
        //    {
        //        CLogger.Print("Failed a Client Connection", LoggerType.Warning);
        //    }
        //    MainSocket.BeginAccept(new AsyncCallback(AcceptCallback), ClientSocket);
        //}

        private void AcceptCallback(IAsyncResult Result)
        {
            if (ServerIsClosed)
            {
                return;
            }

            Socket listenerSocket = (Socket)Result.AsyncState;
            try
            {
                Socket handler = listenerSocket.EndAccept(Result);
                if (handler != null)
                {
                    // Cek IP client
                    IPEndPoint remoteEndPoint = (IPEndPoint)handler.RemoteEndPoint;
                    IPAddress ipAddress = remoteEndPoint.Address;

                    if (!IsIpAllowed(ipAddress))
                    {
                        CLogger.Print($"Blocked IP: {ipAddress}", LoggerType.Warning);
                        handler.Close();
                    }
                    else
                    {
                        // Set timeout
                        handler.ReceiveTimeout = 5000;
                        handler.SendTimeout = 5000;

                        // Gunakan thread pool
                        ThreadPool.QueueUserWorkItem(_ =>
                        {
                            try
                            {
                                AuthClient client = new AuthClient(ServerId, handler);
                                AddSocket(client);
                                if (client == null)
                                {
                                    CLogger.Print("Destroyed after failed to add to list.", LoggerType.Warning);
                                }
                            }
                            catch (Exception ex)
                            {
                                CLogger.Print($"Client init error: {ex.Message}", LoggerType.Error, ex);
                                handler.Close();
                            }
                        });

                        Thread.Sleep(1); // Sedikit delay untuk meringankan loop
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"Accept error: {ex.Message}", LoggerType.Error, ex);
            }

            try
            {
                listenerSocket.BeginAccept(new AsyncCallback(AcceptCallback), listenerSocket);
            }
            catch (Exception ex)
            {
                CLogger.Print($"BeginAccept failed: {ex.Message}", LoggerType.Error, ex);
            }
        }

        public void AddSocket(AuthClient Client)
        {
            if (Client == null)
            {
                return;
            }
            int MaxVal = 0x7FFF;
            DateTime Connect = DateTimeUtil.Now();
            for (int SessionId = 1; SessionId < MaxVal; SessionId++)
            {
                if (!AuthXender.SocketList.ContainsKey(SessionId) && AuthXender.SocketList.TryAdd(SessionId, Client))
                {
                    Client.SessionDate = Connect;
                    Client.SessionId = SessionId;
                    Client.SessionSeed = (ushort)new Random(Connect.Millisecond).Next(SessionId, MaxVal);
                    Client.Start();
                    return;
                }
            }
            Client.Close(500, true);
        }
        public bool RemoveSocket(AuthClient Client)
        {
            try
            {
                if (Client == null || Client.SessionId == 0)
                {
                    return false;
                }
                if (AuthXender.SocketList.ContainsKey(Client.SessionId) && AuthXender.SocketList.TryGetValue(Client.SessionId, out Client))
                {
                    return AuthXender.SocketList.TryRemove(Client.SessionId, out Client);
                }
                Client = null;
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
            return false;
        }
        public int SendPacketToAllClients(AuthServerPacket Packet)
        {
            int Count = 0;
            if (AuthXender.SocketList.Count == 0)
            {
                return Count;
            }
            byte[] Data = Packet.GetCompleteBytes("AuthManager.SendPacketToAllClients");
            foreach (AuthClient Client in AuthXender.SocketList.Values)
            {
                Account Player = Client.Player;
                if (Player != null && Player.IsOnline)
                {
                    Player.SendCompletePacket(Data, Packet.GetType().Name);
                    Count++;
                }
            }
            return Count;
        }
        public Account SearchActiveClient(long accountId)
        {
            if (AuthXender.SocketList.Count == 0)
            {
                return null;
            }
            foreach (AuthClient client in AuthXender.SocketList.Values)
            {
                Account player = client.Player;
                if (player != null && player.PlayerId == accountId)
                {
                    return player;
                }
            }
            return null;
        }

    }

}
