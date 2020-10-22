using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using Cosmos.System.ScanMaps;
using AirOS.System;

namespace AirOS
{
    public class Kernel : Sys.Kernel
    {
        public static CosmosVFS fs;
        public static string SystemName = "AirOS";
        public static string SystemVersion = "0.1 Alpha";
        public static string GuiVersion = "0.0.1 Alpha";
        public static string Support = "No";
        public static string current_dir = @"0:\";
        public static string main_part = @"0:\";
        public static string loguser;
        private static string logpass;
        public static string loggeduser;
        public static string currentpass;
        public static string currank;
        public static AirOS.Network.IPV4.Config LocalNetworkConfig;


        protected override void BeforeRun()
        {
            
            fs = new CosmosVFS();
            VFSManager.RegisterVFS(fs);
            Console.Clear();
            if (!File.Exists(main_part + "AirOS.cfg"))
            {
                Setup.StartSetup.Step1();
            }
            else
            {
                if(File.Exists(main_part+"lang.cfg"))
                {
                    string lang = File.ReadAllText(main_part + "lang.cfg");
                    if(lang == "de") { SetKeyboardScanMap(new DE_Standard()); }
                    else if(lang == "us") { SetKeyboardScanMap(new US_Standard()); }
                }
                Console.Write("Enter Username: ");
                loguser = Console.ReadLine();
                string fileuser = ConsoleHandler.FindUser(loguser);
                string[] newuser = fileuser.Split(":");
                    if (loguser == newuser[0])
                    {

                        Console.Write("Enter Password: ");
                        logpass = Console.ReadLine();
                        if (logpass == newuser[1].Trim())
                        {
                            loggeduser = newuser[0];
                            currentpass = newuser[1];
                            currank = newuser[2];
                            Console.Clear();
                            Console.WriteLine("Welcome Back {0}", newuser[0]);
               /*         Network.NetworkInit.Init();
                        Network.NetworkInit.Enable();
                        System.Utilities.WaitSeconds(1);
                        Network.NetworkInterfaces.Init();
                        System.Utilities.WaitSeconds(3);*/
                        return;
                        }
                        else
                        {
                            System.Utilities.ErrorScreen("0x101", "Your Login Details are incorrect!", 5, true);
                        }
                    }
                    else
                    {
                        System.Utilities.ErrorScreen("0x101", "Your Login Details are incorrect!", 5, true);
                    }
                
            }


        }

        protected override void Run()
        {
            Console.Write("Console: ");
            var input = Console.ReadLine();
            System.ConsoleHandler.HandleCommand(input);
        }
    }
}
