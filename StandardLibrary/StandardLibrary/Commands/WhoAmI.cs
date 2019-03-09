using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Faction.Modules.Dotnet.Common;
using System.Security.Principal;

namespace Faction.Modules.Dotnet.Commands
{
  class WhoAmI : Command
  {
    public override string Name { get { return "whoami"; } }
    public override CommandOutput Execute(Dictionary<string, string> Parameters = null)
    {
      CommandOutput output = new CommandOutput();
      try
      {
        string username = WindowsIdentity.GetCurrent().Name;
        output.Complete = true;
        output.Success = true;
        output.Message = $"{username}";

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
