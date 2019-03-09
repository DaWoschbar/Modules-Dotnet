using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Faction.Modules.Dotnet.Common;

namespace Faction.Modules.Dotnet.Commands
{
  class F2File
  {
    public string Name;
    public string Type;
    public long? Length;
    public DateTime LastAccessTime;
    public DateTime LastWriteTime;
  }

  class ListFiles : Command
  {
    public override string Name { get { return "ls"; } }
    public override CommandOutput Execute(Dictionary<string, string> Parameters=null)
    {
      CommandOutput output = new CommandOutput();
      string pwd = Directory.GetCurrentDirectory();
      if (Parameters.ContainsKey("Path"))
      {
        pwd = Parameters["Path"];
      }

      try
      {
        string[] entries = Directory.GetFileSystemEntries(pwd);
        List<F2File> results = new List<F2File>();

        foreach (string entry in entries)
        {
          F2File f2File = new F2File();
          FileAttributes attr = File.GetAttributes(entry);
          if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
          {
            f2File.Type = "Directory";
            f2File.Name = new DirectoryInfo(entry).Name;
            f2File.Length = null;
            f2File.LastAccessTime = Directory.GetLastAccessTimeUtc(entry);
            f2File.LastWriteTime = Directory.GetLastWriteTime(entry);
          }
          else
          {
            f2File.Type = "File";
            f2File.Name = Path.GetFileName(entry);
            f2File.Length = new FileInfo(entry).Length;
            f2File.LastAccessTime = File.GetLastAccessTimeUtc(entry);
            f2File.LastWriteTime = File.GetLastWriteTimeUtc(entry);
          }
          results.Add(f2File);
        }

        output.Complete = true;
        output.Success = true;
        output.Message = JsonConvert.SerializeObject(results);
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
