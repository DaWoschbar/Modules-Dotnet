using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Faction.Modules.Dotnet.Common;
using Faction.Modules.Dotnet.Commands;

namespace WinLibTests
{
    [TestClass]
    public class PingTests
    {
        Dictionary<string, string> Parameters = new Dictionary<string, string>();

        [TestMethod]
        public void PingInitialization()
        {
            List<Command> commands = Faction.Modules.Dotnet.Initialize.GetCommands();
            Faction.Modules.Dotnet.Common.Command pingCommand = commands[0];
            Assert.AreEqual("ping", pingCommand.Name);
        }

        [TestMethod]
        public void PingLocalHost()
        {
            Parameters.Add("ComputerName", "localhost");
            List<Command> commands = Faction.Modules.Dotnet.Initialize.GetCommands();
            Faction.Modules.Dotnet.Common.Command pingCommand = commands[0];
            CommandOutput results = pingCommand.Execute(Parameters);
            Assert.IsTrue(results.Complete);
            Assert.IsTrue(results.Success);
            // Message should equal = "[{"ComputerName":"localhost","IsUp":true}]"
            Assert.IsTrue(results.Message.Contains("localhost"));
            // We only check IsUp as ping may be disabled for localhost in some situations
            Assert.IsTrue(results.Message.Contains("IsUp"));
        }

        [TestMethod]
        public void PingMultipleHosts()
        {
            Parameters.Add("ComputerName", "c2.lol, bing.com, google.com, www.microsoft.com");
            List<Command> commands = Faction.Modules.Dotnet.Initialize.GetCommands();
            Faction.Modules.Dotnet.Common.Command pingCommand = commands[0];
            CommandOutput results = pingCommand.Execute(Parameters);
            Assert.IsTrue(results.Complete);
            Assert.IsTrue(results.Success);
            Console.WriteLine(results.Message.ToString());
            // Message should contain results for the 4 hosts we pinged.
            Assert.IsTrue(results.Message.Contains("c2.lol"));
            Assert.IsTrue(results.Message.Contains("bing.com"));
            Assert.IsTrue(results.Message.Contains("google.com"));
            Assert.IsTrue(results.Message.Contains("microsoft.com"));

            //Debugging line, remove before PR due to error with this I suspect
            Console.WriteLine(results.Message.ToString());

        }

        [TestMethod]
        public void PingTimeOutTest()
        {
            Parameters.Add("ComputerName", "c2.lol");
            Parameters.Add("Timeout", "60");
            List<Command> commands = Faction.Modules.Dotnet.Initialize.GetCommands();
            Faction.Modules.Dotnet.Common.Command pingCommand = commands[0];
            CommandOutput results = pingCommand.Execute(Parameters);
            Assert.IsTrue(results.Complete);
            Assert.IsTrue(results.Success);
            Assert.IsTrue(results.Message.Contains("c2.lol"));
            Assert.IsTrue(results.Message.Contains("IsUp"));
        }

        [TestMethod]
        public void PingThreadsTest()
        {
            Parameters.Add("ComputerName", "c2.lol, bing.com, google.com, www.microsoft.com");
            Parameters.Add("Threads", "3");
            List<Command> commands = Faction.Modules.Dotnet.Initialize.GetCommands();
            Faction.Modules.Dotnet.Common.Command pingCommand = commands[0];
            CommandOutput results = pingCommand.Execute(Parameters);
            Assert.IsTrue(results.Complete);
            Assert.IsTrue(results.Success);
            Assert.IsTrue(results.Message.Contains("c2.lol"));
            Assert.IsTrue(results.Message.Contains("IsUp"));
        }
    }
}
