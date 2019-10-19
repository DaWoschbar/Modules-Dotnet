using Faction.Modules.Dotnet.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace WinLibTests
{
    [TestClass]
    public class PowerShellTests
    {
        Dictionary<string, string> Parameters = new Dictionary<string, string>();

        [TestMethod]
        public void PowerShellInitialize()
        {
            List<Command> commands = Faction.Modules.Dotnet.Initialize.GetCommands();
            Faction.Modules.Dotnet.Common.Command powershellCommand = commands[2];
            Assert.AreEqual("powershell", powershellCommand.Name);
        }

        /// <summary>
        /// This does not properly test the bypass logging at this time
        /// </summary>
        [TestMethod]
        public void PowershellBypassLogging()
        {
            List<Command> commands = Faction.Modules.Dotnet.Initialize.GetCommands();
            Faction.Modules.Dotnet.Common.Command powershellCommand = commands[2];
            Parameters.Add("Command", "Test-NetConnection 127.0.0.1");
            Parameters.Add("BypassLogging", "true");
            EventLog log = new EventLog("Security");
            CommandOutput results = powershellCommand.Execute(Parameters);
            
        }

        /// <summary>
        /// This does not properly test bypassing amsi at this time
        /// </summary>
        [TestMethod]
        public void PowershellBypassAMSI()
        {
            List<Command> commands = Faction.Modules.Dotnet.Initialize.GetCommands();
            Faction.Modules.Dotnet.Common.Command powershellCommand = commands[2];
            Parameters.Add("Command", "Test-NetConnection 127.0.0.1");
            Parameters.Add("BypassAmsi", "true");
            EventLog log = new EventLog("Security");
            CommandOutput results = powershellCommand.Execute(Parameters);
        }

        /// <summary>
        /// This does not properly test outstring at this time
        /// </summary>
        [TestMethod]
        public void PowershellOutString()
        {
            List<Command> commands = Faction.Modules.Dotnet.Initialize.GetCommands();
            Faction.Modules.Dotnet.Common.Command powershellCommand = commands[2];
            Parameters.Add("Command", "Test-NetConnection 127.0.0.1");
            Parameters.Add("OutString", "true");
            EventLog log = new EventLog("Security");
            CommandOutput results = powershellCommand.Execute(Parameters);
        }
    }
}
