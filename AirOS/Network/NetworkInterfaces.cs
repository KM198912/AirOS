using AirOS.Network.Drivers;
using System.Collections.Generic;
using AirOS.Utilities;
using Cosmos.System;

namespace AirOS.Network
{
    class NetworkInterfaces
    {
        public static List<string> PCIName = new List<string>();
        public static List<string> CustomName = new List<string>();

        public static void Init()
        {
            System.Utilities.PrintConsole("");
            System.Utilities.PrintConsole("Initializing network interfaces...");

            int ID = 0;
            Settings interfaces = new Settings(@"0:\netinterface.cfg");

            foreach (NetworkDevice networkDevice in NetworkDevice.Devices)
            {
                PCIName.Add(networkDevice.Name);
                if (networkDevice.CardType == CardType.Ethernet)
                {
                    System.Utilities.PrintConsole("Found network interface of type ethernet, ID " + ID);
                    CustomName.Add("eth" + ID);
                }
                else if (networkDevice.CardType == CardType.Wireless)
                {
                    System.Utilities.PrintConsole("Found network interface of type wireless, ID " + ID);
                    CustomName.Add("wls" + ID);
                }
                else
                {
                    System.Utilities.PrintConsole("Found network interface of type unknown, ID " + ID);
                    CustomName.Add("unk" + ID);
                }
                Save(ID, interfaces);
                ID++;
            }
            interfaces.Push();
        }

        public static string Interface(string interfaceName)
        {
            Settings interfaces = new Settings(@"0:\netinterface.cfg");
            return interfaces.Get(interfaceName);
        }

        private static void Save(int ID, Settings interfaces)
        {
            System.Utilities.PrintConsole("Added item to interfaces config file: " + CustomName[ID] + ":" + PCIName[ID]);
            interfaces.Edit(CustomName[ID], PCIName[ID]);
        }
    }
}
