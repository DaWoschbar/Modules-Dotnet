using Faction.Modules.Dotnet.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace WinLibTests
{
    [TestClass]
    public class RegTests
    {
        /// <summary>
        /// Scaffolding Method for RegistryWrite and RegistryDelete Tests
        /// </summary>
        private CommandOutput KeyOperations(string operation, string path, string value)
        {
            Dictionary<string, string> Parameters = new Dictionary<string, string>();
            List<Command> commands = Faction.Modules.Dotnet.Initialize.GetCommands();
            Faction.Modules.Dotnet.Common.Command regCommand = commands[3];
            Parameters.Add("Operation", operation);
            Parameters.Add("Path", path);
            if (value!=String.Empty)
            {
                Parameters.Add("Value", "TestWrite");
            }

            CommandOutput results = regCommand.Execute(Parameters);
            return results;
        }


        [TestMethod]
        public void RegistryInitialization()
        {
            List<Command> commands = Faction.Modules.Dotnet.Initialize.GetCommands();
            Faction.Modules.Dotnet.Common.Command regCommand = commands[3];
            Assert.AreEqual("services", regCommand.Name);
        }


        [TestMethod]
        public void RegistryRead()
        {
            Dictionary<string, string> Parameters = new Dictionary<string, string>();
            //Read this key: Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\SusClientId
            //This method intentionally contains some duplicate code, to intentionally test just read - as some registries may be "readonly" due to permissions
            List<Command> commands = Faction.Modules.Dotnet.Initialize.GetCommands();
            Faction.Modules.Dotnet.Common.Command regCommand = commands[3];
            Parameters.Add("Operation", "read");
            Parameters.Add("Path", "HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\DevicePath");
            CommandOutput results = regCommand.Execute(Parameters);

            //Sample output: C:\WINDOWS\inf
            Assert.IsTrue(results.Complete);
            Assert.IsTrue(results.Success);
            Assert.IsTrue(results.Message.Contains("WINDOWS")); //inf SHOULD always be in Windows, consider a better directory in the future
        }


        // This method intentionally fails at the moment due to deletion not properly parsing the written value
        // The failure is evidence of a bug
        [TestMethod]
        public void RegistryWrite()
        {
            CommandOutput writeresults = KeyOperations("write", "HKEY_CURRENT_USER\\Environment\\TestValue", "TestWrite");
            Assert.IsTrue(writeresults.Complete);
            Assert.IsTrue(writeresults.Success);

            CommandOutput deleteresults = KeyOperations("delete", "HKCU\\Environment\\TestValue", String.Empty);
            Assert.IsTrue(deleteresults.Complete);
            Assert.IsTrue(deleteresults.Success);
        }
    }
}
