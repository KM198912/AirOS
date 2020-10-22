using System;
using System.Collections.Generic;
using System.Text;

namespace AirOS.Network
{
    class IPConfig
    {
        public static void c_IPConfig(string cmd)
        {
            string[] args = cmd.Split(' ');

            if (args.Length == 1)
            {
                Console.WriteLine("IP Config");
                return;
            }

            if (args[1] == "/release")
            {
                Network.DHCP.Core.SendReleasePacket();
            }
            else if (args[1] == "/interfaces")
            {
                Utilities.Settings settings = new Utilities.Settings(@"0:\netinterface.conf");
                int i = 0;
                foreach (String s in NetworkInterfaces.CustomName)
                {
                    Console.WriteLine(s + " " + settings.Get(s) + " " + NetworkInterfaces.PCIName[i]);
                    i++;
                }
            }
            else if (args[1] == "/interface")
            {
                Console.WriteLine(NetworkInterfaces.Interface(args[2]));
            }
            else if (args[1] == "/set")
            {
                if (args.Length <= 3)
                {
                    Console.WriteLine("Usage : " + args[0] + " /set {interface} {IPv4} {Subnet} -g {Gateway} -d {PrimaryDNS}");
                    //ipconfig /set PCNETII 192.168.1.32 255.255.255.0 -g 192.168.1.254 -d 8.8.8.8
                }
                else
                {
                    if (NetworkInterfaces.Interface(args[2]) != "null")
                    {
                        Utilities.Settings settings = new Utilities.Settings(@"0:\" + NetworkInterfaces.Interface(args[2]) + ".conf");
                        NetworkStack.RemoveAllConfigIP();
                        ApplyIP(args, settings);
                        settings.Push();
                        NetworkInit.Enable();
                    }
                    else
                    {
                        Console.WriteLine("This interface doesn't exists.");
                    }
                }
            }
            else if (args[1] == "/renew")
            {
                Network.DHCP.Core.SendDiscoverPacket();
            }
            else
            {
                Console.WriteLine("IP Config");
            }
        }
        private static void ApplyIP(string[] args, Utilities.Settings settings)
        {
            int args_count = args.Length;
            switch (args_count)
            {
                default:
                    Console.WriteLine("Usage : " + args[0] + " /set {interface} {IPv4} {Subnet} -g {Gateway} -d {PrimaryDNS}");
                    break;
                case 5:
                    if (Utilities.Misc.IsIpv4Address(args[3]) && Utilities.Misc.IsIpv4Address(args[4]))
                    {
                        settings.Edit("ipaddress", args[3]);
                        settings.Edit("subnet", args[4]);
                        settings.Edit("gateway", "0.0.0.0");
                        settings.Edit("dns01", "0.0.0.0");
                    }
                    else
                    {
                        //notcorrectaddress
                    }
                    break;
                case 7:
                    if (Utilities.Misc.IsIpv4Address(args[3]) && Utilities.Misc.IsIpv4Address(args[4]) && Utilities.Misc.IsIpv4Address(args[6]))
                    {
                        settings.Edit("ipaddress", args[3]);
                        settings.Edit("subnet", args[4]);
                        if (args[5] == "-g")
                        {
                            settings.Edit("gateway", args[6]);
                            settings.Edit("dns01", "0.0.0.0");
                        }
                        else if (args[5] == "-d")
                        {
                            settings.Edit("dns01", args[6]);
                            settings.Edit("gateway", "0.0.0.0");
                        }
                        else
                        {
                            Console.WriteLine("Usage : " + args[0] + " /set {interface} {IPv4} {Subnet} -g {Gateway} -d {PrimaryDNS}");
                            settings.Edit("gateway", "0.0.0.0");
                            settings.Edit("dns01", "0.0.0.0");
                        }
                    }
                    else
                    {
                        //notcorrectaddress
                    }
                    break;
                case 9:
                    if (Utilities.Misc.IsIpv4Address(args[3]) && Utilities.Misc.IsIpv4Address(args[4]) && Utilities.Misc.IsIpv4Address(args[6]) && Utilities.Misc.IsIpv4Address(args[8]))
                    {
                        settings.Edit("ipaddress", args[3]);
                        settings.Edit("subnet", args[4]);
                        if (args[5] == "-g")
                        {
                            settings.Edit("gateway", args[6]);
                        }
                        else if (args[5] == "-d")
                        {
                            settings.Edit("dns01", args[6]);
                        }
                        else
                        {
                            Console.WriteLine("Usage : " + args[0] + " /set {interface} {IPv4} {Subnet} -g {Gateway} -d {PrimaryDNS}");
                            settings.Edit("gateway", "0.0.0.0");
                            settings.Edit("dns01", "0.0.0.0");
                        }

                        if (args[7] == "-g")
                        {
                            settings.Edit("gateway", args[8]);
                        }
                        else if (args[7] == "-d")
                        {
                            settings.Edit("dns01", args[8]);
                        }
                        else
                        {
                            Console.WriteLine("Usage : " + args[0] + " /set {interface} {IPv4} {Subnet} -g {Gateway} -d {PrimaryDNS}");
                            settings.Edit("gateway", "0.0.0.0");
                            settings.Edit("dns01", "0.0.0.0");
                        }
                    }
                    break;

            }
        }
    }
}
