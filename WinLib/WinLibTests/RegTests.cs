using Faction.Modules.Dotnet.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Win32;

namespace WinLibTests
{
    [TestClass]
    public class RegTests
    {
        /// <summary>
        /// Scaffolding Method for RegistryWrite and RegistryDelete Tests
        /// </summary>
        private CommandOutput KeyOperations(string operation, string path, string value, string type)
        {
            Dictionary<string, string> Parameters = new Dictionary<string, string>();
            List<Command> commands = Faction.Modules.Dotnet.Initialize.GetCommands();
            Faction.Modules.Dotnet.Common.Command regCommand = commands[3];
            Parameters.Add("Operation", operation);
            Parameters.Add("Path", path);
            if (value != String.Empty)
            {
                Parameters.Add("Value", value);
            }

            if (type != String.Empty)
            {
                Parameters.Add("Type", type);
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


        [TestMethod]
        public void RegistryWrite()
        {
            // first key to write and then we delete value and then key
            CommandOutput writeresults = KeyOperations("write", "HKEY_CURRENT_USER\\Environment\\TestValue", "TestWrite", String.Empty);
            Assert.IsTrue(writeresults.Complete);
            Assert.IsTrue(writeresults.Success);

            //Clean up!
            RegistryKey envKey = Registry.CurrentUser.OpenSubKey("Environment", true);
            envKey.DeleteValue("TestValue");
            envKey.Close();
        }

        [TestMethod]
        public void RegistryDeleteSubKey()
        {
            //Scaffolding, Create a Key
            RegistryKey envKey = Registry.CurrentUser.OpenSubKey("Environment", true);
            RegistryKey testKey = envKey.CreateSubKey("TestKey");

            CommandOutput deleteresults = KeyOperations("delete", "HKCU\\Environment", "TestKey", "SubKey");
            Console.WriteLine(deleteresults.Message);
            Assert.IsTrue(deleteresults.Complete);
            Assert.IsTrue(deleteresults.Success);

            envKey.Close();
        }


        [TestMethod]
        public void RegistryDeleteValue()
        {
            //Scaffolding, Create a Key
            RegistryKey envKey = Registry.CurrentUser.OpenSubKey("Environment", true);
            RegistryKey testKey = envKey.CreateSubKey("TestKey");
            testKey.SetValue("TestSetting", "TestValue");

            CommandOutput deleteresults = KeyOperations("delete", "HKCU\\Environment\\TestKey", "TestSetting", "Value");
            Assert.IsTrue(deleteresults.Complete);
            Assert.IsTrue(deleteresults.Success);

            //Clean up!
            envKey.DeleteSubKey("TestKey");
            envKey.Close();
        }
    }
}
