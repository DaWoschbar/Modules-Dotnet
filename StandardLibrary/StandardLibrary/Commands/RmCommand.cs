using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Faction.Modules.Dotnet.Common;

namespace Faction.Modules.Dotnet.Commands
{
  class RmCommand : Command
  {
    public override string Name { get { return "rm"; } }
    public override CommandOutput Execute(Dictionary<string, string> Parameters = null)
    {
      CommandOutput output = new CommandOutput();
      string path = Path.GetFullPath(Parameters["Path"]);
      string type = "file";
      // get the file attributes for file or directory
      FileAttributes attr = File.GetAttributes(path);

      //detect whether its a directory or file
      if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
      {
        Directory.Delete(path, true);
        type = "directory";
      }
      else
      {
        File.Delete(path);
      }


      output.Complete = true;
      output.Success = true;
      output.Message = $"Deleted {path}";
      output.IOCs.Add(new IOC("file", path, "delete", $"Deleted {type} {path}"));

      return output;
    }
  }
}
