using Faction.Modules.Dotnet.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace WinLibTests
{
    [TestClass]
    public class PortScanTests
    {
        Dictionary<string, string> Parameters = new Dictionary<string, string>();
        [TestMethod]
        public void PortScanInitialize()
        {
            List<Command> commands = Faction.Modules.Dotnet.Initialize.GetCommands();
            Faction.Modules.Dotnet.Common.Command pingCommand = commands[1];
            Assert.AreEqual("portscan", pingCommand.Name);
        }

        [TestMethod]
        public void PortScanTimeOut()
        {
        }

        [TestMethod]
        public void PortScanThreads()
        {
        }

        [TestMethod]
        public void PortScanPing()
        {
        }

        [TestMethod]
        public void PortScanMultipleComputers()
        {
        }
    }
}
