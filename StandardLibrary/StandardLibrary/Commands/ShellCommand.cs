using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Faction.Modules.Dotnet.Common;
using System.Security.Principal;
using System.Diagnostics;

namespace Faction.Modules.Dotnet.Commands
{
  class ShellCommand : Command
  {
    public override string Name { get { return "shell"; } }
    public override CommandOutput Execute(Dictionary<string, string> Parameters = null)
    {
      CommandOutput output = new CommandOutput();
      try
      {
        string cmd = Parameters["Command"];
        string path = "";
        string username = "";
        string domain = "";
        string password = "";

        if (Parameters.ContainsKey("Path")) {
          path = Parameters["Path"];
        }

        if (Parameters.ContainsKey("Username"))
        {
          username = Parameters["Username"];
        }

        if (Parameters.ContainsKey("Domain"))
        {
          domain = Parameters["Domain"];
        }

        if (Parameters.ContainsKey("Password"))
        {
          password = Parameters["Password"];
        }

        List<string> result = ShellExecuteWithPath(cmd, path, username, domain, password);
        output.Complete = true;
        output.Success = true;
        output.Message = $"{result[1]}";

        if (String.IsNullOrEmpty(username))
        {
          username = WindowsIdentity.GetCurrent().Name;
        }

        output.IOCs.Add(new IOC("process", result[0], "create", $"Process started: \"{cmd}\" under username \"{username}\""));

      }
      catch (Exception e)
      {
        output.Complete = true;
        output.Success = false;
        output.Message = e.Message;
      }
      return output;
    }

    // Taken from Cobbr's SharpSploit project: https://github.com/cobbr/SharpSploit/blob/master/SharpSploit/Execution/Shell.cs
    public static List<string> ShellExecuteWithPath(string ShellCommand, string Path, string Username = "", string Domain = "", string Password = "")
    {
      string ShellCommandName = ShellCommand.Split(' ')[0];
      string ShellCommandArguments = "";
      if (ShellCommand.Contains(" "))
      {
        ShellCommandArguments = ShellCommand.Replace(ShellCommandName + " ", "");
      }

      Process shellProcess = new Process();
      if (Username != "")
      {
        shellProcess.StartInfo.UserName = Username;
        shellProcess.StartInfo.Domain = Domain;
        System.Security.SecureString SecurePassword = new System.Security.SecureString();
        foreach (char c in Password)
        {
          SecurePassword.AppendChar(c);
        }
        shellProcess.StartInfo.Password = SecurePassword;
      }
      shellProcess.StartInfo.FileName = ShellCommandName;
      shellProcess.StartInfo.Arguments = ShellCommandArguments;
      shellProcess.StartInfo.WorkingDirectory = Path;
      shellProcess.StartInfo.UseShellExecute = false;
      shellProcess.StartInfo.CreateNoWindow = true;
      shellProcess.StartInfo.RedirectStandardOutput = true;
      shellProcess.Start();

      string output = shellProcess.StandardOutput.ReadToEnd();
      shellProcess.WaitForExit();
      List<string> result = new List<string>();
      result.Add(shellProcess.Id.ToString());
      result.Add(output);
      return result;
    }
  }
}
