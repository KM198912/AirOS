using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Sys = Cosmos.System;
using Cosmos.System.ScanMaps;
using AirOS.System;

namespace AirOS.Setup
{
    class StartSetup
    {
        private static string UserName;
        private static string Password; 
        public static void Step1()
        {
            if(!File.Exists(Kernel.current_dir+"AirOS.cfg"))
            {
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Clear();
                Console.WriteLine(Kernel.SystemName + " needs to Format " + Kernel.main_part + " in order to function properly.");
                Console.WriteLine("would you like to do so now?");
                Console.Write("Y/N: ");
                if(Console.ReadLine() == "Y")
                {
                    Console.WriteLine("Formatting....");
                    Kernel.fs.Format(Kernel.main_part, "FAT32", true);
                    FileStream writeStream = File.Create(Kernel.current_dir+"AirOS.cfg");
                    Kernel.fs.SetFileSystemLabel(Kernel.main_part, Kernel.SystemName);
                    byte[] toWrite = Encoding.ASCII.GetBytes("true");
                    writeStream.Write(toWrite, 0, toWrite.Length);
                    writeStream.Close();
                    Console.WriteLine("Finished Formatting.");

                    Console.Clear();
                 //   return;
                    Step2();
                }
                else
                {
                    System.Utilities.ErrorScreen("0x100", Kernel.SystemName + " Setup cannot Continue without Formatting your Hard Drive!\nRebooting in 5 Seconds", 5, true);
                }
            }
        }
        public static void Step2()
        {
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();
            if (!File.Exists(@"0:\lang.cfg"))
            {
                File.Create(@"0:\lang.cfg");
                Console.Write("Set Keyboard Layout: 1 = de_DE / 2 - en_US : ");
                if (Console.ReadLine() == "1")
                {
                    Console.WriteLine("Keyboard Layout set to: de_DE");
                    File.WriteAllText(@"0:\lang.cfg", "de");
                    Console.WriteLine("Language Set to de_DE!");
                    Step3();
                }
                else if (Console.ReadLine() == "2")
                {
                    Console.WriteLine("Keyboard Layout set to: en_US");
                    File.WriteAllText(@"0:\lang.cfg", "us");
                    Console.WriteLine("Language Set to en_US!");
                    Step3();
                }
                else
                {
                    Console.WriteLine("Invalid input!\nDefaulting to en_US");
                    File.WriteAllText(@"0:\lang.cfg", "us");
                    Console.WriteLine("Language Set to en_US!");
                    Step3();
                }
            }
        }
        public static void Step3()
        {
            if (!File.Exists(Kernel.main_part + "User.cfg"))
            {
                File.Create(Kernel.main_part + "User.cfg");
                Console.Write("Enter your Username: ");
                UserName = Console.ReadLine();
                Console.Write("Select a Password: ");
                Password = Console.ReadLine();
                File.WriteAllText(Kernel.main_part + "User.cfg", UserName + ":" + Password + ":Administrator" + Environment.NewLine);
                Console.WriteLine("User Created! Rebooting in 5 Seconds!");
                Utilities.WaitSeconds(5);
                Cosmos.System.Power.Reboot();

            }
        }
    }
}
