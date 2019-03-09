using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Faction.Modules.Dotnet.Common;
using Newtonsoft.Json;
using System.ServiceProcess;

namespace Faction.Modules.Dotnet.Commands
{
  class ServiceObject
  {
    public string Name;
    public string DisplayName;
    public string Status;
  }

  class Services : Command
  {
    public override string Name { get { return "services"; } }
    public override CommandOutput Execute(Dictionary<string, string> Parameters = null)
    {
      CommandOutput output = new CommandOutput();
      List<ServiceObject> results = new List<ServiceObject>();

      string ComputerName = "localhost";
      if (Parameters.ContainsKey("ComputerName"))
      {
        ComputerName = Parameters["ComputerName"];
      }

      ServiceController[] services = ServiceController.GetServices(ComputerName);
      foreach (ServiceController sc in services)
      {
        ServiceObject result = new ServiceObject();
        result.Name = sc.ServiceName;
        result.DisplayName = sc.DisplayName;
        result.Status = sc.Status.ToString();
        results.Add(result);
      }
      output.Message = JsonConvert.SerializeObject(results);
      output.Success = true;
      output.Complete = true;
      return output;
    }
  }
}

