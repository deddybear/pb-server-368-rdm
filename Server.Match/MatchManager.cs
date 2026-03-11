using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Utility;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server.Match
{
    public class MatchManager
    {
        private readonly string Host;
        private readonly int Port;
        public Socket MainSocket;
        public bool ServerIsClosed;
        private StateObject State;
        public MatchManager(string Host, int Port)
        {
            this.Host = Host;
            this.Port = Port;
        }
        public bool Start()
        {
            try
            {
                MainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                uint IOC_IN = 0x80000000, IOC_VENDOR = 0x18000000, SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                IPEndPoint Local = new IPEndPoint(IPAddress.Parse(Host), Port);
                State = new StateObject() { WorkSocket = MainSocket, LocalPoint = Local };
                MainSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                MainSocket.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
                MainSocket.SetIPProtectionLevel(IPProtectionLevel.EdgeRestricted);
                MainSocket.Bind(Local);
                Thread ThreadMatch = new Thread(Read)
                {
                    Priority = ThreadPriority.Highest
                };
                ThreadMatch.Start();
                CLogger.Print($"Match Serv Address {Local.Address}:{Local.Port}", LoggerType.Info);
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
                MainSocket.BeginReceiveFrom(State.Buffer, 0, StateObject.BufferSize, SocketFlags.None, ref State.LocalPoint, new AsyncCallback(AcceptCallback), State);
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
        }

        private void AcceptCallback(IAsyncResult Result)
        {
            if (ServerIsClosed)
            {
                return;
            }
            if (!Result.IsCompleted)
            {
                CLogger.Print("IAsyncResult is not completed.", LoggerType.Warning);
            }
            Result.AsyncWaitHandle.WaitOne(5000);
            EndPoint Address = new IPEndPoint(IPAddress.Any, 0);
            Socket WorkSocket = (Result.AsyncState as StateObject).WorkSocket;
            EndPoint Local = (Result.AsyncState as StateObject).LocalPoint;
            try
            {
                int Received = WorkSocket.EndReceiveFrom(Result, ref Address);
                if (Received > StateObject.BufferSize)
                {
                    CLogger.Print($"Received Sized Is Over The Limit! (Size: {Received})", LoggerType.Warning);
                    return;
                }
                byte[] NewBuffer = new byte[Received];
                Buffer.BlockCopy(State.Buffer, 0, NewBuffer, 0, Received);
                if (NewBuffer.Length >= 22)
                {
                    MatchClient Client = new MatchClient(MainSocket, Address as IPEndPoint);
                    BeginReceive(Client, NewBuffer);
                    if (Client == null)
                    {
                        CLogger.Print("Destroyed after failed to receive the udp.", LoggerType.Warning);
                    }
                }
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.ConnectionReset)
                {
                    // Silent ignore or log if needed
                    CLogger.Print("UDP connection reset by peer (ICMP unreachable).", LoggerType.Warning);
                }
                else
                {
                    CLogger.Print($"Socket error: {ex.Message}", LoggerType.Error, ex);
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"Failed a Client Connection: {ex.Message}", LoggerType.Error, ex);
            }
            MainSocket.BeginReceiveFrom(State.Buffer, 0, StateObject.BufferSize, SocketFlags.None, ref State.LocalPoint, new AsyncCallback(AcceptCallback), new StateObject() { WorkSocket = WorkSocket, LocalPoint = Local });
        }

        //private void AcceptCallback(IAsyncResult Result)
        //{
        //    if (ServerIsClosed || Result == null)
        //        return;

        //    EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        //    StateObject state = Result.AsyncState as StateObject;

        //    if (state == null || state.WorkSocket == null)
        //        return;

        //    Socket workSocket = state.WorkSocket;
        //    EndPoint localPoint = state.LocalPoint;

        //    byte[] receiveBuffer = new byte[StateObject.BufferSize];

        //    try
        //    {
        //        int receivedBytes = workSocket.EndReceiveFrom(Result, ref remoteEndPoint);

        //        if (receivedBytes > StateObject.BufferSize)
        //        {
        //            CLogger.Print($"Received size is over the limit! (Size: {receivedBytes})", LoggerType.Warning);
        //            return;
        //        }

        //        Buffer.BlockCopy(state.Buffer, 0, receiveBuffer, 0, receivedBytes);

        //        if (receivedBytes >= 22)
        //        {
        //            MatchClient client = new MatchClient(MainSocket, remoteEndPoint as IPEndPoint);
        //            BeginReceive(client, receiveBuffer);

        //            if (client == null)
        //                CLogger.Print("Destroyed after failed to receive the UDP.", LoggerType.Warning);
        //        }
        //    }
        //    catch (SocketException ex)
        //    {
        //        if (ex.SocketErrorCode == SocketError.ConnectionReset)
        //        {
        //            // Silent ignore or log if needed
        //            CLogger.Print("UDP connection reset by peer (ICMP unreachable).", LoggerType.Warning);
        //        }
        //        else
        //        {
        //            CLogger.Print($"Socket error: {ex.Message}", LoggerType.Error, ex);
        //        }
        //    }
        //    catch (ObjectDisposedException)
        //    {
        //        // Socket was closed while waiting
        //        CLogger.Print("Socket was already closed.", LoggerType.Warning);
        //    }
        //    catch (Exception ex)
        //    {
        //        CLogger.Print($"Exception in AcceptCallback: {ex.Message}", LoggerType.Error, ex);
        //    }

        //    // Continue listening (use new StateObject every time)
        //    try
        //    {
        //        StateObject newState = new StateObject()
        //        {
        //            WorkSocket = workSocket,
        //            LocalPoint = localPoint
        //        };

        //        workSocket.BeginReceiveFrom(newState.Buffer, 0, StateObject.BufferSize, SocketFlags.None, ref newState.LocalPoint, new AsyncCallback(AcceptCallback), newState);
        //    }
        //    catch (Exception ex)
        //    {
        //        CLogger.Print($"Failed to restart receive: {ex.Message}", LoggerType.Error, ex);
        //    }
        //}


        public void BeginReceive(MatchClient Client, byte[] Buffer)
        {
            try
            {
                if (Client == null)
                {
                    return;
                }
                Client.BeginReceive(Buffer, DateTimeUtil.Now());
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
        }
        public void SendPacket(byte[] Data, IPEndPoint Address)
        {
            MainSocket.SendTo(Data, 0, Data.Length, SocketFlags.None, Address);
        }
    }
}