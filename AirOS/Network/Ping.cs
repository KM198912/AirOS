using System;
using AirOS.Network.IPV4;
using AirOS.Utilities;

namespace AirOS.Network
{
    class Ping
    {
        private static string HelpInfo = "";

        /// <summary>
        /// Getter and Setters for Help Info.
        /// </summary>
        public static string HI
        {
            get { return HelpInfo; }
            set { HelpInfo = value; /*PUSHED OUT VALUE (in)*/}
        }

        /// <summary>
        /// Empty constructor. (Good for debug)
        /// </summary>
        public Ping() { }

        /// <summary>
        /// c = command, c_Ping
        /// </summary>
        /// <param name="arg">IP Address</param>
        /// /// <param name="startIndex">The start index for remove.</param>
        /// <param name="count">The count index for remove.</param>
        public static void c_Ping(string arg, short startIndex = 0, short count = 5)
        {
            if (Misc.IsIpv4Address(arg))
            {
                string[] items = arg.Split('.');

                int PacketSent = 0;
                int PacketReceived = 0;
                int PacketLost = 0;

                int PercentLoss = 0;

                try
                {
                    Address destination = new Address((byte)int.Parse(items[0]), (byte)int.Parse(items[1]), (byte)int.Parse(items[2]), (byte)int.Parse(items[3]));
                    Address source = Config.FindNetwork(destination);

                    int _deltaT = 0;
                    int second;

                    for (int i = 0; i < 4; i++)
                    {
                        second = 0;
                        Console.WriteLine("Sending ping to " + destination.ToString() + "...");

                        System.Utilities.WaitSeconds(1);

                        try
                        {
                            ICMPEchoRequest request = new ICMPEchoRequest(source, destination, 0x0001, 0x50); //this is working
                            OutgoingBuffer.AddPacket(request); //Aura doesn't work when this is called.
                            NetworkStack.Update();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error on ping " + i + ": " + ex);
                        }

                        PacketSent++;

                        while (true)
                        {

                            if (ICMPPacket.recvd_reply != null)
                            {
                                //if (ICMPPacket.recvd_reply.SourceIP == destination)
                                //{

                                if (second < 1)
                                {
                                    Console.WriteLine("Reply received from " + ICMPPacket.recvd_reply.SourceIP.ToString() + " time < 1s");
                                }
                                else if (second >= 1)
                                {
                                    Console.WriteLine("Reply received from " + ICMPPacket.recvd_reply.SourceIP.ToString() + " time " + second + "s");
                                }

                                PacketReceived++;

                                ICMPPacket.recvd_reply = null;
                                break;
                                //}
                            }

                            if (second >= 5)
                            {
                                Console.WriteLine("Destination host unreachable.");
                                PacketLost++;
                                break;
                            }

                            if (_deltaT != Cosmos.HAL.RTC.Second)
                            {
                                second++;
                                _deltaT = Cosmos.HAL.RTC.Second;
                            }
                        }
                    }
                }
                catch
                {
                    Console.WriteLine("Internal error.");
                }
                finally
                {
                    PercentLoss = 25 * PacketLost;

                    Console.WriteLine();
                    Console.WriteLine("Ping statistics for " + arg + ":");
                    Console.WriteLine("    Packets: Sent = " + PacketSent + ", Received = " + PacketReceived + ", Lost = " + PacketLost + " (" + PercentLoss + "% loss)");
                }
            }
            else
            {
                Network.IPV4.UDP.DNS.DNSClient DNSRequest = new Network.IPV4.UDP.DNS.DNSClient(53);
                DNSRequest.Ask(arg);
                int _deltaT = 0;
                int second = 0;
                while (!DNSRequest.ReceivedResponse)
                {
                    if (_deltaT != Cosmos.HAL.RTC.Second)
                    {
                        second++;
                        _deltaT = Cosmos.HAL.RTC.Second;
                    }

                    if (second >= 4)
                    {
                        //Apps.System.Debugger.debugger.Send("No response in 4 secondes...");
                        break;
                    }
                }
                DNSRequest.Close();
                c_Ping("     " + DNSRequest.address.ToString());
            }
        }

    }
}