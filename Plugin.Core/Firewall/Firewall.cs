using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using NetFwTypeLib;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Plugin.Core.Firewall
{
    public class FirewallManager
    {
        public static List<string> Allowed = new List<string>();
        public static List<string> Blocked = new List<string>();
        public static string PortTCP = "39191";
        public static string PortUDP = "40009";
        public static void Allow(string ip)
        {
            try
            {
                Type tNetFwPolicy2 = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                INetFwPolicy2 fwPolicy2 = (INetFwPolicy2)Activator.CreateInstance(tNetFwPolicy2);

                string subnet = GetSubnet(ip, 16);

                if (Allowed.Contains(ip))
                {
                    return;
                }
                INetFwRule2 tcpAllowRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
                tcpAllowRule.Name = "[PB Firewall] " + ip + " Allow TCP Packet";
                tcpAllowRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
                tcpAllowRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
                tcpAllowRule.Enabled = true;
                tcpAllowRule.RemoteAddresses = subnet;
                tcpAllowRule.Protocol = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;
                tcpAllowRule.LocalPorts = PortTCP; //
                fwPolicy2.Rules.Add(tcpAllowRule);

                INetFwRule2 udpAllowRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
                udpAllowRule.Name = "[PB Firewall] " + ip + " Allow UDP Packet";
                udpAllowRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
                udpAllowRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
                udpAllowRule.Enabled = true;
                udpAllowRule.RemoteAddresses = subnet;
                udpAllowRule.Protocol = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_UDP;
                udpAllowRule.LocalPorts = PortUDP;
                fwPolicy2.Rules.Add(udpAllowRule);

                Allowed.Add(ip);
            }
            catch (Exception e)
            {
                CLogger.Print("Firewall Error: " + e.Message, Enums.LoggerType.Error, e);
            }
        }

        public static void Block(string ip, string description)
        {
            try
            {
                Type tNetFwPolicy2 = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                INetFwPolicy2 fwPolicy2 = (INetFwPolicy2)Activator.CreateInstance(tNetFwPolicy2);

                if (Blocked.Contains(ip))
                {
                    return;
                }

                INetFwRule2 blockRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
                blockRule.Name = "[Block PB Firewall] " + ip + " Block Packet";
                blockRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
                blockRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
                blockRule.Enabled = true;
                blockRule.RemoteAddresses = ip;
                blockRule.Description = description;
                fwPolicy2.Rules.Add(blockRule);

                Blocked.Add(ip);
            }
            catch (Exception e)
            {
                CLogger.Print("Firewall Error: " + e.Message, Enums.LoggerType.Error, e);
            }
        }
        public static void Delete(string name)
        {
            try
            {
                Type tNetFwPolicy2 = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                INetFwPolicy2 fwPolicy2 = (INetFwPolicy2)Activator.CreateInstance(tNetFwPolicy2);

                foreach (INetFwRule2 rule in fwPolicy2.Rules)
                {
                    if (rule.Name.StartsWith(name))
                    {
                        fwPolicy2.Rules.Remove(rule.Name);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                CLogger.Print("Firewall Error: " + e.Message, Enums.LoggerType.Error, e);
            }
        }
        public static void Reset()
        {
            try
            {
                string arg = "";

                if (1 == 1)
                {
                    arg += "/c netsh advfirewall firewall delete rule name= all protocol=tcp localport=" + PortTCP;
                    arg += " && netsh advfirewall firewall delete rule name= all protocol=udp localport=" + PortUDP;
                }
                else
                {
                    arg += "/c netsh advfirewall firewall delete rule name= all protocol=tcp localport=" + PortTCP;
                    arg += " && netsh advfirewall firewall delete rule name= all protocol=udp localport=" + PortUDP;
                }

                Process pr = new Process();
                ProcessStartInfo prs = new ProcessStartInfo();
                prs.FileName = @"cmd.exe";
                prs.Verb = "runas"; // Run to admin
                prs.Arguments = arg;
                prs.WindowStyle = ProcessWindowStyle.Hidden;
                pr.StartInfo = prs;
                pr.Start();
            }
            catch (Exception ex)
            {
                CLogger.Print("Firewall Error: " + ex.ToString(), Enums.LoggerType.Error, ex);
            }
        }
        private static string GetSubnet(string ip, int subnetMask)
        {
            IPAddress ipAddr = IPAddress.Parse(ip);

            string subnet = ip.Substring(0, ip.LastIndexOf("."));
            //Opsional 8/16/24/32 (0.0.0.0)
            return subnet + ".0/" + subnetMask;
        }
    }
}
