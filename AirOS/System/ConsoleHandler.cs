using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AirOS.System
{

    class ConsoleHandler
    {
public static void DeleteUser(string input)
        {
            string strFilePath = @"0:\User.cfg";
            string strSearchText = input;
            string strOldText;
            string n = "";
            StreamReader sr = File.OpenText(strFilePath);
            while ((strOldText = sr.ReadLine()) != null)
            {
                if (!strOldText.Contains(strSearchText))
                {
                    n += strOldText + Environment.NewLine;
                }
            }
            sr.Close();
            File.WriteAllText(strFilePath, n);
        }
        public static String FindUser(string input)
          {
              List<string> found = new List<string>();
              string line;
              using (StreamReader file = new StreamReader(@"0:\User.cfg"))
              {
                  while ((line = file.ReadLine()) != null)
                  {
                      if (line.Contains(input))
                      {
                       found.Add(line);
                      }
                  }
               
                if (found.Count == 0)
                {
                    return "User not Found";
                }
                else
                { 
                    return found[0];
                }

              }
          }
        public static void HandleCommand(string input)
        {
            if (input == "poweroff")
            {
                Cosmos.System.Power.Shutdown();
            }
            else if (input == "restart")
            {
                Cosmos.System.Power.Reboot();
            }
            else if (input.StartsWith("cat"))
            {
                string catfile = input.Remove(0, 4);
                Console.WriteLine(File.ReadAllText(@"0:\" + catfile));
            }
            else if (input == "beeper-test")
            {
                Cosmos.System.PCSpeaker.Beep();
            }
            else if (input == "deluser")
            {
                if (!Utilities.isAdmin(Kernel.currank)) { Console.WriteLine("No permission to do this."); return; }
                Console.Write("Username to Delete: ");
                var username = Console.ReadLine();
                string deluser = FindUser(username);
                if (deluser != "User not Found")
                {
                    if (username == Kernel.loggeduser)
                    {
                        Console.WriteLine("You cannot delete your own account");
                    }
                    else
                    {
                        Console.Write("Are you sure you want to Delete " + username + " ? ");
                        if (Console.ReadLine() == "y")
                        {
                            DeleteUser(deluser);
                        }
                        else
                        {
                            Console.WriteLine("Aborted!");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("The User you tried to Delete does not exist!");
                }
            }
            else if (input == "createuser")
            {
                if (!Utilities.isAdmin(Kernel.currank)) { Console.WriteLine("No permission to do this."); return; }
                Console.Write("Enter Username: ");
                var newuser = Console.ReadLine();
                Console.Write("Enter Password: ");
                var newuserpass = Console.ReadLine();
                string UserExists = FindUser(newuser);
                if (UserExists == "User not Found")
                {
                    //   File.AppendAllText(@"0:\User.cfg", "\n" + newuser + ":" + newuserpass);
                    using (StreamWriter w = File.AppendText(@"0:\User.cfg"))
                    {
                        w.WriteLine(newuser + ":" + newuserpass + ":User" + Environment.NewLine);
                    }
                    string inptuFileName = @"0:\User.cfg";
                    var tempFileName = Path.GetTempFileName();
                    try
                    {
                        using (var streamReader = new StreamReader(inptuFileName))
                        using (var streamWriter = new StreamWriter(tempFileName))
                        {
                            string line;
                            while ((line = streamReader.ReadLine()) != null)
                            {
                                if (!string.IsNullOrWhiteSpace(line))
                                    streamWriter.WriteLine(line);
                            }
                        }
                        File.Copy(tempFileName, inptuFileName, true);
                    }
                    finally
                    {
                        File.Delete(tempFileName);
                    }
                    Console.WriteLine("User: " + newuser + " has been registered with the System, their password is: " + newuserpass);
                }
                else
                {
                    Console.WriteLine("User already exists");
                }
            }
            else if (input.StartsWith("finduser"))
            {
                try
                {
                    string UserToFind = input.Remove(0, 9);
                    if (UserToFind == "") { Console.WriteLine("You need to pass an argument to 'finduser'"); }
                    else
                    {
                        string[] finaluser = FindUser(UserToFind).Split(":");

                        Console.WriteLine(finaluser[0] + " Rank: " + finaluser[2]);

                    }
                }
                catch
                {
                    Console.WriteLine("Invalid Syntax");
                }
            }
            else if (input == "passwd")
            {
                Console.Write("Enter your new Password: ");
                var newpass1 = Console.ReadLine();
                Console.Write("Repeat your new Password: ");
                var newpass2 = Console.ReadLine();
                if (newpass1 == Kernel.currentpass)
                {
                    Console.WriteLine("New Password matches current password. Aborting!");
                }
                else
                {
                    if (newpass2 == newpass1)
                    {
                        string user = FindUser(Kernel.loggeduser);
                        string text = File.ReadAllText(@"0:\User.cfg");
                        text = text.Replace(user, Kernel.loggeduser + ":" + newpass1 + ":" + Kernel.currank);
                        File.WriteAllText(@"0:\User.cfg", text);
                        //  Utilities.WaitSeconds(5);
                        // Cosmos.System.Power.Reboot();
                    }
                    else
                    {
                        Console.WriteLine("Passwords dont match!");
                    }
                }
            }




            else if (input.StartsWith("format"))
            {
                if (!Utilities.isAdmin(Kernel.currank)) { Console.WriteLine("No permission to do this."); return; }
                string drive = input.Remove(0, 6);
                if (Kernel.fs.IsValidDriveId(drive))
                {
                    Console.WriteLine("Formatting " + Kernel.main_part + "! Please wait....");
                    Kernel.fs.Format(drive, "FAT32", true);
                    Kernel.fs.SetFileSystemLabel(Kernel.main_part, Kernel.SystemName);
                    Console.WriteLine("Format Finished\nRestarting in 5 Seconds!\nAfter the Reboot you will be guided through the Setup Process");
                    Utilities.WaitSeconds(5);
                    Cosmos.System.Power.Reboot();
                }
                else
                {
                    Console.WriteLine(drive + " is not a valid Drive!");
                }
            }
            else if (input == "clear")
            {
                Console.Clear();
            }
         /*   else if (input.StartsWith("ipconfig"))
            {
                Network.IPConfig.c_IPConfig(input);
            }
            else if (input.StartsWith("ping "))
                {
                    Network.Ping.c_Ping(input.Split(" ")[1]);
                   
                }
         */
                else if (input == "sysinfo")
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("OS Name: " + Kernel.SystemName);
                Console.WriteLine("OS Version: " + Kernel.SystemVersion);
                Console.WriteLine("GUI Version: " + Kernel.GuiVersion);
                Console.WriteLine("Logged in User: " + Kernel.loggeduser);
                Console.WriteLine("User Permission Level: " + Kernel.currank);
                Console.WriteLine("Keyboard Layout: " + File.ReadAllText(@"0:\lang.cfg"));
                Console.WriteLine("Filesystem: " + Kernel.fs.GetFileSystemType("0"));
                Console.WriteLine("Partition Label: " + Kernel.fs.GetFileSystemLabel("0"));
                Console.WriteLine("Support Enabled: " + Kernel.Support);
                Console.WriteLine("CPU: " + Utilities.CPUName().Trim());
                Console.WriteLine("CPU Vendor: " + Utilities.CPUVendor());
                Console.WriteLine("Installed Memory: " + Utilities.GetTotalMemory() + " MB");
                Console.ResetColor();

            }
            else
            {
                Console.WriteLine("{0} : Command not found!", input);
            }

        }
    }
}
