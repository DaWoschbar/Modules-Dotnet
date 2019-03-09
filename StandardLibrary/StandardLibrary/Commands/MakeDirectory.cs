using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Faction.Modules.Dotnet.Common;

namespace Faction.Modules.Dotnet.Commands
{
  class MakeDirectory : Command
  {
    public override string Name { get { return "mkdir"; } }
    public override CommandOutput Execute(Dictionary<string, string> Parameters = null)
    {
      CommandOutput output = new CommandOutput();
      string path = Path.GetFullPath(Parameters["Path"]);
      Directory.CreateDirectory(path);
      output.Complete = true;
      output.Success = true;
      output.Message = $"Created Directory: {path}";
      output.IOCs.Add(new IOC("file", path, "create", output.Message));
      return output;
    }
  }
}
