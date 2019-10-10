using Faction.Modules.Dotnet.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace WinLibTests
{
    [TestClass]
    public class ServicesTests
    {
        [TestMethod]
        public void ServicesGet()
        {
            Dictionary<string, string> Parameters = new Dictionary<string, string>();
            List<Command> commands = Faction.Modules.Dotnet.Initialize.GetCommands();
            Faction.Modules.Dotnet.Common.Command servicesCommand = commands[4];
            Parameters.Add("ComputerName", "localhost"); //added to test if statement

            CommandOutput results = servicesCommand.Execute(Parameters);

            Assert.IsTrue(results.Complete);
            Assert.IsTrue(results.Success);
            Assert.IsTrue(results.Message.Contains("Windows Update")); //check to see if Windows Update is present, should work for most systems
        }
    }
}
