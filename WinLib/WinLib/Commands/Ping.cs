using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Faction.Modules.Dotnet.Common;
using Newtonsoft.Json;
using SharpSploit.Generic;
using SharpSploit.Enumeration;

// TODO: Re-write this without the SharpSploit dependency so we can include it in StandardLib
namespace Faction.Modules.Dotnet.Commands
{
  class Ping : Command
  {
    public override string Name { get { return "ping"; } }
    public override CommandOutput Execute(Dictionary<string, string> Parameters = null)
    {
      CommandOutput output = new CommandOutput();
      try
      {
        string computerName = Parameters["ComputerName"];
        List<string> computerNames = new List<string>();
        int timeout = 250;
        int threads = 25;

        if (computerName.Contains(","))
        {
          computerNames = computerName.Split(',').ToList();
        }
        else
        {
          computerNames.Add(computerName);
        }

        if (Parameters.ContainsKey("Timeout"))
        {
          timeout = Int32.Parse(Parameters["Timeout"]);
        }

        if (Parameters.ContainsKey("Threads"))
        {
          threads = Int32.Parse(Parameters["Threads"]);
        }

        SharpSploitResultList<Network.PingResult> results = Network.Ping(computerNames, timeout, threads);

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

