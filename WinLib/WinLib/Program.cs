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
      commands.Add(new Ping());
      commands.Add(new PortScan());
      commands.Add(new PowerShell());
      commands.Add(new Reg());
      commands.Add(new Services());
      commands.Add(new Wmi());
      commands.Add(new Driver());
      return commands;
    }
  }
}
