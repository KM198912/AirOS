using System;
using System.Collections.Generic;
using System.Text;
using Cosmos;
using Cosmos.Core;

namespace AirOS.System
{
    class Utilities
    {
        public static CPU read = new CPU();
        public static uint GetTotalMemory()
        {
          return CPU.GetAmountOfRAM();
        }
        public static ulong Uptime()
        {
            return CPU.GetCPUUptime();
        }
        public static String CPUName()
        {
          return CPU.GetCPUBrandString();
        }
        public static string CPUVendor()
        {
            return CPU.GetCPUVendorName();
        }
        public static bool isAdmin(string input)
        { 
        if(input == "User") { return false; }
        else if(input == "Administrator") { return true; }
        else { return false; }
        }
        public static void ErrorScreen(string code, string message, int wait, bool Reboot = false)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Clear();
            Console.WriteLine("Error: " + code);
            Console.WriteLine(message);
            WaitSeconds(wait);
            if (Reboot)
                Cosmos.System.Power.Reboot();
            else
                Cosmos.System.Power.Shutdown();


        }
        public static void WaitSeconds(int secNum)
        {
            int StartSec = Cosmos.HAL.RTC.Second;
            int EndSec;
            if (StartSec + secNum > 59)
            {
                EndSec = 0;
            }
            else
            {
                EndSec = StartSec + secNum;
            }
            while (Cosmos.HAL.RTC.Second != EndSec) { }
        }
    }
}
