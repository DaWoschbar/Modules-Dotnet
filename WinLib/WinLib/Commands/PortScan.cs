using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Faction.Modules.Dotnet.Common;
using Newtonsoft.Json;
using SharpSploit.Generic;
using SharpSploit.Enumeration;

namespace Faction.Modules.Dotnet.Commands
{
  class PortScan : Command
  {
    public override string Name { get { return "portscan"; } }
    public override CommandOutput Execute(Dictionary<string, string> Parameters = null)
    {
      CommandOutput output = new CommandOutput();
      try
      {
        string computerName = Parameters["ComputerName"];
        string portsParam = Parameters["Port"];
        List<string> portStrings = new List<string>();

        List<string> computerNames = new List<string>();
        List<int> ports = new List<int>();
        bool ping = true;
        int timeout = 250;
        int threads = 100;

        if (computerName.Contains(","))
        {
          computerNames = computerName.Split(',').ToList();
        }
        else
        {
          computerNames.Add(computerName);
        }

        if (portsParam.Contains(","))
        {
          portStrings = portsParam.Split(',').ToList();
        }
        else
        {
          portStrings.Add(portsParam);
        }

        foreach (string portString in portStrings)
        {
          ports.Add(Int32.Parse(portString));
        }

        if (Parameters.ContainsKey("Timeout"))
        {
          timeout = Int32.Parse(Parameters["Timeout"]);
        }

        if (Parameters.ContainsKey("Threads"))
        {
          threads = Int32.Parse(Parameters["Threads"]);
        }

        if (Parameters.ContainsKey("Ping"))
        {
          ping = Boolean.Parse(Parameters["Ping"]);
        }

        SharpSploitResultList<Network.PortScanResult> results = Network.PortScan(computerNames, ports, ping, timeout);

        output.Message = JsonConvert.SerializeObject(results);
        output.Success = true;
        output.Complete = true;
      }
      catch (Exception e)
      {
        output.Complete = true;
        output.Success = false;
        output.Message = e.Message;
      }
      return output;
    }
  }
}

