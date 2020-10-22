using Cosmos.HAL;
using AirOS.Utilities;
using Cosmos.System;

namespace AirOS.Network
{
    class NetworkInit
    {
        public static void Enable()
        {
            if (RTL8168NIC != null)
            {
                Settings settings = new Settings(@"0:\" + RTL8168NIC.Name + ".cfg");
                if (!IsSavedConf(RTL8168NIC.Name))
                {
                    Kernel.LocalNetworkConfig = new Network.IPV4.Config(new Network.IPV4.Address(0, 0, 0, 0), new Network.IPV4.Address(0, 0, 0, 0), new Network.IPV4.Address(0, 0, 0, 0));
                    Network.NetworkStack.ConfigIP(RTL8168NIC, Kernel.LocalNetworkConfig);
                }
                else
                {
                    Kernel.LocalNetworkConfig = new Network.IPV4.Config(Network.IPV4.Address.Parse(settings.GetValue("ipaddress")), Network.IPV4.Address.Parse(settings.GetValue("subnet")), Network.IPV4.Address.Parse(settings.GetValue("gateway")));
                    Network.NetworkStack.ConfigIP(RTL8168NIC, Kernel.LocalNetworkConfig);
                }
            }
            if (AMDPCNetIINIC != null)
            {
                Settings settings = new Settings(@"0:\" + AMDPCNetIINIC.Name + ".cfg");
                if (!IsSavedConf(AMDPCNetIINIC.Name))
                {
                    Kernel.LocalNetworkConfig = new Network.IPV4.Config(new Network.IPV4.Address(0, 0, 0, 0), new Network.IPV4.Address(0, 0, 0, 0), new Network.IPV4.Address(0, 0, 0, 0));
                    Network.NetworkStack.ConfigIP(AMDPCNetIINIC, Kernel.LocalNetworkConfig);
                }
                else
                {
                    Kernel.LocalNetworkConfig = new Network.IPV4.Config(Network.IPV4.Address.Parse(settings.GetValue("ipaddress")), Network.IPV4.Address.Parse(settings.GetValue("subnet")), Network.IPV4.Address.Parse(settings.GetValue("gateway")));
                    Network.NetworkStack.ConfigIP(AMDPCNetIINIC, Kernel.LocalNetworkConfig);
                }
            }
        }

        static Drivers.RTL8168 RTL8168NIC;
        static Drivers.AMDPCNetII AMDPCNetIINIC;

        public static void Init(bool debug = true)
        {
            if (debug)
            {
                System.Utilities.PrintConsole("Finding NIC...");
            }

            PCIDevice RTL8168 = PCI.GetDevice((VendorID)0x10EC, (DeviceID)0x8168);
            if (RTL8168 != null)
            {
                if (debug)
                {
                    System.Utilities.PrintConsole("Found RTL8168 NIC on PCI " + RTL8168.bus + ":" + RTL8168.slot + ":" + RTL8168.function);
                    System.Utilities.PrintConsole("NIC IRQ: " + RTL8168.InterruptLine);
                }
                RTL8168NIC = new Drivers.RTL8168(RTL8168);
                if (debug)
                {
                    System.Utilities.PrintConsole("NIC MAC Address: " + RTL8168NIC.MACAddress.ToString());
                }
                Network.NetworkStack.Init();
                RTL8168NIC.Enable();
            }
            PCIDevice AMDPCNETII = PCI.GetDevice(VendorID.AMD, DeviceID.PCNETII);
            if (AMDPCNETII != null)
            {
                if (debug)
                {
                    System.Utilities.PrintConsole("Found AMDPCNETII NIC on PCI " + AMDPCNETII.bus + ":" + AMDPCNETII.slot + ":" + AMDPCNETII.function);
                    System.Utilities.PrintConsole("NIC IRQ: " + AMDPCNETII.InterruptLine);
                }
                AMDPCNetIINIC = new Drivers.AMDPCNetII(AMDPCNETII);
                if (debug)
                {
                    System.Utilities.PrintConsole("NIC MAC Address: " + AMDPCNetIINIC.MACAddress.ToString());
                }
                Network.NetworkStack.Init();
                AMDPCNetIINIC.Enable();
            }
            if (RTL8168 == null && AMDPCNETII == null)
            {
                System.Utilities.PrintConsole("No supported network card found!!");
                return;
            }
            if (debug)
            {
                System.Utilities.PrintConsole("Network initialization done!");
            }
        }

        static bool IsSavedConf(string device)
        {
                Settings settings = new Settings(@"0:\" + device + ".cfg");
                if ((settings.GetValue("ipaddress") != "0.0.0.0") || (settings.GetValue("subnet") != "0.0.0.0") || (settings.GetValue("gateway") != "0.0.0.0"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        

    }
}
