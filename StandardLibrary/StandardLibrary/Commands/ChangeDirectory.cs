using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Faction.Modules.Dotnet.Common;

namespace Faction.Modules.Dotnet.Commands
{
  class ChangeDirectory : Command
  {
    public override string Name { get { return "cd"; } }
    public override CommandOutput Execute(Dictionary<string, string> Parameters = null)
    {
      CommandOutput output = new CommandOutput();
      try
      {
        string path = Parameters["Path"];
        Directory.SetCurrentDirectory(path);
        output.Complete = true;
        output.Success = true;
        output.Message = $"Current directory is now: {path}";

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
