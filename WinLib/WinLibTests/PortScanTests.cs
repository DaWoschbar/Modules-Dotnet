using Faction.Modules.Dotnet.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

namespace WinLibTests
{
    [TestClass]
    public class PortScanTests
    {
        Dictionary<string, string> Parameters = new Dictionary<string, string>();
        HttpListener httpListner = new HttpListener();

        private bool ScaffoldingStart()
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();

            //Determine whether or not we need to open a test port, if 8894 is already open that also works for us
            bool testPortNeeded = true;
            foreach (IPEndPoint ipep in ipEndPoints)
            {
                if (ipep.Port == 8894)
                {
                    testPortNeeded = false;
                }
            }

            // Create the test listener and start if needed
            httpListner.Prefixes.Add("http://127.0.0.1:8894/");
            if (testPortNeeded)
            {
                httpListner.Start();
            }

            return testPortNeeded;
        }

        private void Cleanup(bool testPortNeeded)
        {
            if (testPortNeeded)
            {
                httpListner.Stop();
                httpListner.Close();
            }
        }

        [TestMethod]
        public void PortScanInitialize()
        {
            List<Command> commands = Faction.Modules.Dotnet.Initialize.GetCommands();
            Faction.Modules.Dotnet.Common.Command portScanCommand = commands[1];
            Assert.AreEqual("portscan", portScanCommand.Name);
        }

        [TestMethod]
        public void PortScanTimeOut()
        {
            bool testPortNeeded = ScaffoldingStart();
            List<Command> commands = Faction.Modules.Dotnet.Initialize.GetCommands();
            Faction.Modules.Dotnet.Common.Command portScanCommand = commands[1];
            Parameters.Add("ComputerName", "localhost");
            Parameters.Add("Timeout", "60");
            //Test common ports, eventually consider opening one of our own to listen on
            Parameters.Add("Port", "21,22,80,139,443,445,3389,5432,8080,8888,8894");


            //Sample output: [{"ComputerName":"localhost","Port":445,"IsOpen":true},{"ComputerName":"localhost","Port":8894,"IsOpen":true}]
            CommandOutput results = portScanCommand.Execute(Parameters);
            Assert.IsTrue(results.Complete);
            Assert.IsTrue(results.Success);
            
            //Only open ports are returned so we don't need to look for the IsOpen
            Assert.IsTrue(results.Message.Contains("8894"));

            //Cleanup the test listener if it was created
            Cleanup(testPortNeeded);
        }

        [TestMethod]
        public void PortScanThreads()
        {
            bool testPortNeeded = ScaffoldingStart();
            List<Command> commands = Faction.Modules.Dotnet.Initialize.GetCommands();
            Faction.Modules.Dotnet.Common.Command portScanCommand = commands[1];
            Parameters.Add("ComputerName", "localhost");
            Parameters.Add("Threads", "3");
            //Test common ports, eventually consider opening one of our own to listen on
            Parameters.Add("Port", "21,22,80,139,443,445,3389,5432,8080,8888,8894");

            //Sample output: [{"ComputerName":"localhost","Port":445,"IsOpen":true},{"ComputerName":"localhost","Port":8894,"IsOpen":true}]
            CommandOutput results = portScanCommand.Execute(Parameters);
            Assert.IsTrue(results.Complete);
            Assert.IsTrue(results.Success);

            //Only open ports are returned so we don't need to look for the IsOpen
            Assert.IsTrue(results.Message.Contains("8894"));

            //Cleanup the test listener if it was created
            Cleanup(testPortNeeded);
        }

        [TestMethod]
        public void PortScanPing()
        {
            bool testPortNeeded = ScaffoldingStart();
            List<Command> commands = Faction.Modules.Dotnet.Initialize.GetCommands();
            Faction.Modules.Dotnet.Common.Command portScanCommand = commands[1];
            Parameters.Add("ComputerName", "localhost");
            Parameters.Add("Ping", "true");
            //Test common ports, eventually consider opening one of our own to listen on
            Parameters.Add("Port", "21,22,80,139,443,445,3389,5432,8080,8888,8894");

            //Sample output: [{"ComputerName":"localhost","Port":445,"IsOpen":true},{"ComputerName":"localhost","Port":8894,"IsOpen":true}]
            CommandOutput results = portScanCommand.Execute(Parameters);
            Assert.IsTrue(results.Complete);
            Assert.IsTrue(results.Success);

            //Only open ports are returned so we don't need to look for the IsOpen
            Assert.IsTrue(results.Message.Contains("8894"));

            //Cleanup the test listener if it was created
            Cleanup(testPortNeeded);
        }

        [TestMethod]
        public void PortScanMultipleComputers()
        {
            bool testPortNeeded = ScaffoldingStart();
            List<Command> commands = Faction.Modules.Dotnet.Initialize.GetCommands();
            Faction.Modules.Dotnet.Common.Command portScanCommand = commands[1];
            Parameters.Add("ComputerName", "localhost, c2.lol");
            Parameters.Add("Timeout", "60");
            //Test common ports, eventually consider opening one of our own to listen on
            Parameters.Add("Port", "21,22,80,139,443,445,3389,5432,8080,8888,8894");

            //Sample output: [{"ComputerName":"localhost","Port":445,"IsOpen":true},{"ComputerName":"localhost","Port":8894,"IsOpen":true}]
            CommandOutput results = portScanCommand.Execute(Parameters);
            Assert.IsTrue(results.Complete);
            Assert.IsTrue(results.Success);

            // This currently fails due to only one computer being scanned
            Assert.IsTrue(results.Message.Contains("c2.lol"));

            //Only open ports are returned so we don't need to look for the IsOpen
            Assert.IsTrue(results.Message.Contains("8894"));

            //Cleanup the test listener if it was created
            Cleanup(testPortNeeded);
        }
    }
}
