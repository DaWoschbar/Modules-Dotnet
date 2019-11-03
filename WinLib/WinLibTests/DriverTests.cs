using Faction.Modules.Dotnet.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Net;
using System.IO;
using System.Security.Principal;

namespace WinLibTests
{
    [TestClass]
    public class DriverTests
    {

        // Scaffolding Methods
        private bool PermissionCheck()
        {
            bool isElevated;
            List<string> deviceDrivers = new List<string>();

            // Check privileges, admin is required to work with drivers for this module
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            return isElevated;
        }

        private string DriverSetup()
        {
            // This method has some scaffolding in that we download a file, it will require internet to run
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string tempPath = System.IO.Path.GetTempPath();
            string processhacker = string.Empty;
            WebClient webClient = new WebClient();
            if (Environment.Is64BitOperatingSystem)
            {
                processhacker = "https://github.com/processhacker/processhacker/raw/master/KProcessHacker/bin-signed/amd64/kprocesshacker.sys";
            }
            else
            {
                processhacker = "https://github.com/processhacker/processhacker/raw/master/KProcessHacker/bin-signed/i386/kprocesshacker.sys";
            }
            tempPath += "\\kprocesshacker.sys"; //use this path for tests, could change it f
            webClient.DownloadFile(processhacker, tempPath);

            return tempPath;
        }

        [TestMethod]
        public void DriverInitialization()
        {
            List<Command> commands = Faction.Modules.Dotnet.Initialize.GetCommands();
            Faction.Modules.Dotnet.Common.Command driverCommand = commands[6];
            Assert.AreEqual("driver", driverCommand.Name);
        }

        [TestMethod]
        public void DriverEnumerate()
        {
            Dictionary<string, string> Parameters = new Dictionary<string, string>();
            List<Command> commands = Faction.Modules.Dotnet.Initialize.GetCommands();
            Faction.Modules.Dotnet.Common.Command driverCommand = commands[6];
            Parameters.Add("Operation", "Enumerate");
            CommandOutput results = driverCommand.Execute(Parameters);
            Console.WriteLine(results.Message);
            Assert.AreEqual("driver", driverCommand.Name);
            Assert.IsTrue(results.Message.Contains("ntoskrnl.exe"));
            Assert.IsTrue(results.Complete);
            Assert.IsTrue(results.Success);
        }

        [TestMethod]
        public void DriverInstall()
        {
            Assert.IsTrue(PermissionCheck());
            string tempPath = DriverSetup();
            Dictionary<string, string> Parameters = new Dictionary<string, string>();
            List<Command> commands = Faction.Modules.Dotnet.Initialize.GetCommands();
            Faction.Modules.Dotnet.Common.Command driverCommand = commands[6];
            Parameters.Add("Operation", "Install");
            Parameters.Add("DriverPath", tempPath);
            Parameters.Add("ServiceName", "ProcessHacker");
            CommandOutput results = driverCommand.Execute(Parameters);
            Console.WriteLine(results.Message);

            // Clean up
            System.Diagnostics.Process createService = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C sc stop ProcessHacker";
            createService.StartInfo = startInfo;
            createService.Start();
            createService.WaitForExit(2000);
            createService.Close();

            System.Diagnostics.Process startService = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startServiceProcessInfo = new System.Diagnostics.ProcessStartInfo();
            startServiceProcessInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startServiceProcessInfo.FileName = "cmd.exe";
            startServiceProcessInfo.Arguments = "/C sc delete ProcessHacker";
            startService.StartInfo = startServiceProcessInfo;
            startService.Start();
            startService.WaitForExit(2000);
            startService.Close();

            File.Delete(tempPath);
            Assert.AreEqual("driver", driverCommand.Name);
            Assert.IsTrue(results.Complete);
            Assert.IsTrue(results.Success);
        }

        [TestMethod]
        public void DriverUnload()
        {
            // sc create ProcessHacker binPath="D:\temp\kprocesshacker.sys" type=kernel start=demand
            // sc start ProcessHacker
            Assert.IsTrue(PermissionCheck());
            string tempPath = DriverSetup();
            System.Diagnostics.Process createService = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C sc create ProcessHacker binPath=" + tempPath + " type=kernel start=demand";
            createService.StartInfo = startInfo;
            createService.Start();
            createService.WaitForExit(2000);
            createService.Close();

            System.Diagnostics.Process startService = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startServiceProcessInfo = new System.Diagnostics.ProcessStartInfo();
            startServiceProcessInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startServiceProcessInfo.FileName = "cmd.exe";
            startServiceProcessInfo.Arguments = "/C sc start \"ProcessHacker\"";
            startService.StartInfo = startServiceProcessInfo;
            startService.Start();
            startService.WaitForExit(2000);
            startService.Close();

            Dictionary<string, string> Parameters = new Dictionary<string, string>();
            List<Command> commands = Faction.Modules.Dotnet.Initialize.GetCommands();
            Faction.Modules.Dotnet.Common.Command driverCommand = commands[6];
            Parameters.Add("Operation", "Unload");
            Parameters.Add("ServiceName", "ProcessHacker");
            CommandOutput results = driverCommand.Execute(Parameters);
            Console.WriteLine(results.Message);
            File.Delete(tempPath);
            Assert.AreEqual("driver", driverCommand.Name);
            Assert.IsTrue(results.Complete);
            Assert.IsTrue(results.Success);
        }
    }
}
