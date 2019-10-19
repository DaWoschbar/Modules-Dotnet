using Faction.Modules.Dotnet.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace WinLibTests
{
    [TestClass]
    public class WmiTests
    {
        Dictionary<string, string> Parameters = new Dictionary<string, string>();

        [TestMethod]
        public void WmiInitialization()
        {
            List<Command> commands = Faction.Modules.Dotnet.Initialize.GetCommands();
            Faction.Modules.Dotnet.Common.Command wmiCommand = commands[5];
            Assert.AreEqual("wmi", wmiCommand.Name);
        }

        [TestMethod]
        public void WmiQuery()
        {
            List<Command> commands = Faction.Modules.Dotnet.Initialize.GetCommands();
            Faction.Modules.Dotnet.Common.Command wmiCommand = commands[5];
            Parameters.Add("ComputerName", "localhost");
            Parameters.Add("Namespace", "root\\cimv2");
            Parameters.Add("Query", "Select * From Win32_Process");

            CommandOutput results = wmiCommand.Execute(Parameters);
            
            //We look for the system idle process since it will always be present on Windows
            Assert.IsTrue(results.Complete);
            Assert.IsTrue(results.Success);
            Assert.IsTrue(results.Message.Contains("System Idle Process"));
        }
    }
}
