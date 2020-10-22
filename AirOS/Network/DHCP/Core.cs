using AirOS.Network.IPV4;
using System;
using AirOS.Network.Drivers;
using AirOS.Utilities;

/*
* PROJECT:          Aura Operating System Development
* CONTENT:          DHCP - DHCP Core
* PROGRAMMER(S):    Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

namespace AirOS.Network.DHCP
{
    class Core
    {
        public static bool DHCPAsked = false;

        /// <summary>
        /// Get the IP address of the DHCP server
        /// </summary>
        /// <returns></returns>
        public static Address DHCPServerAddress(NetworkDevice networkDevice)
        {
            Settings settings = new Settings(@"0:\" + networkDevice.Name + ".conf");
            return Address.Parse(settings.GetValue("dhcp_server"));
        }

        /// <summary>
        /// Send a packet to the DHCP server to make the address available again
        /// </summary>
        public static void SendReleasePacket()
        {
            foreach (NetworkDevice networkDevice in NetworkDevice.Devices)
            {
                Address source = Config.FindNetwork(DHCPServerAddress(networkDevice));
                DHCPRelease dhcp_release = new DHCPRelease(source, DHCPServerAddress(networkDevice));
                OutgoingBuffer.AddPacket(dhcp_release);
                NetworkStack.Update();

                NetworkStack.RemoveAllConfigIP();

                Settings settings = new Settings(@"0:\" + networkDevice.Name + ".conf");
                settings.EditValue("ipaddress", "0.0.0.0");
                settings.EditValue("subnet", "0.0.0.0");
                settings.EditValue("gateway", "0.0.0.0");
                settings.EditValue("dns01", "0.0.0.0");
                settings.PushValues();

                NetworkInit.Enable();
            }            
        }

        /// <summary>
        /// Send a packet to find the DHCP server and tell that we want a new IP address
        /// </summary>
        public static void SendDiscoverPacket()
        {
            foreach (NetworkDevice networkDevice in NetworkDevice.Devices)
            {
                DHCPDiscover dhcp_discover = new DHCPDiscover(networkDevice.MACAddress);
                OutgoingBuffer.AddPacket(dhcp_discover);
                NetworkStack.Update();

                DHCPAsked = true;
            }            
        }

        /// <summary>
        /// Send a request to apply the new IP configuration
        /// </summary>
        public static void SendRequestPacket(Address RequestedAddress, Address DHCPServerAddress)
        {
            foreach (NetworkDevice networkDevice in NetworkDevice.Devices)
            {
                DHCPRequest dhcp_request = new DHCPRequest(networkDevice.MACAddress, RequestedAddress, DHCPServerAddress);
                OutgoingBuffer.AddPacket(dhcp_request);
                NetworkStack.Update();
            }
        }

        /*
         * Method called to applied the differents options received in the DHCP packet ACK
         **/
        /// <summary>
        /// Apply the new IP configuration received.
        /// </summary>
        /// <param name="Options">DHCPOption class using the packetData from the received dhcp packet.</param>
        /// <param name="message">Enable/Disable the displaying of messages about DHCP applying and conf. Disabled by default.
        /// </param>
        public static void Apply(DHCPOption Options, bool message = false)
        {
            NetworkStack.RemoveAllConfigIP();

            //cf. Roadmap. (have to change this, because some network interfaces are not configured in dhcp mode) [have to be done in 0.5.x]
            foreach (NetworkDevice networkDevice in NetworkDevice.Devices)
            {
                if (message)
                {
                    Console.WriteLine();
                    Console.WriteLine("[DHCP ACK][" + networkDevice.Name + "] Packet received, applying IP configuration...");
                    Console.WriteLine("   IP Address  : " + Options.Address().ToString());
                    Console.WriteLine("   Subnet mask : " + Options.Subnet().ToString());
                    Console.WriteLine("   Gateway     : " + Options.Gateway().ToString());
                    Console.WriteLine("   DNS server  : " + Options.DNS01().ToString());
                }

                Settings settings = new Settings(@"0:\" + networkDevice.Name + ".conf");
                settings.EditValue("ipaddress", Options.Address().ToString());
                settings.EditValue("subnet", Options.Subnet().ToString());
                settings.EditValue("gateway", Options.Gateway().ToString());
                settings.EditValue("dns01", Options.DNS01().ToString());
                settings.EditValue("dhcp_server", Options.Server().ToString());
                settings.PushValues();

                NetworkInit.Enable();

                if (message)
                {
                    Console.WriteLine("[DHCP CONFIG][" + networkDevice.Name + "] IP configuration applied.");
                    Console.WriteLine();
                    DHCPAsked = false;
                }
            }
        }
    }
}
