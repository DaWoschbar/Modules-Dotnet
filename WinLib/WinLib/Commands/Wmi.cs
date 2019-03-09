using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Faction.Modules.Dotnet.Common;
using Newtonsoft.Json;
using System.Management;

namespace Faction.Modules.Dotnet.Commands
{
  class WmiObject
  {
    public string Key;
    public string Value;
  }
  class Wmi : Command
  {
    public override string Name { get { return "wmi"; } }
    public override CommandOutput Execute(Dictionary<string, string> Parameters = null)
    {
      CommandOutput output = new CommandOutput();
      List<WmiObject> results = new List<WmiObject>();

      string ComputerName = "localhost";
      if (Parameters.ContainsKey("ComputerName"))
      {
        ComputerName = Parameters["ComputerName"];
      }

      string Namespace = @"root\cimv2";
      if (Parameters.ContainsKey("Namespace"))
      {
        Namespace = Parameters["Namespace"];
      }

      string query = Parameters["Query"];
      
      string scope = $"\\\\{ComputerName}\\{Namespace}";
      ManagementScope managementScope = new ManagementScope(scope);
      managementScope.Connect();
      ObjectQuery objectQuery = new ObjectQuery(query);
      ManagementObjectSearcher searcher = new ManagementObjectSearcher(managementScope, objectQuery);
      ManagementObjectCollection queryResults = searcher.Get();

      foreach (ManagementObject managementObject in queryResults)
      {
        PropertyDataCollection propertyCollection = managementObject.Properties;
        foreach (PropertyData data in propertyCollection)
        {
          WmiObject result = new WmiObject();
          result.Key = data.Name;
          try
          {
            result.Value = data.Value.ToString();
          }
          catch
          {
            result.Value = "Invalid Value";
          }
          results.Add(result);
        }
      }

      output.Message = JsonConvert.SerializeObject(results);
      output.Success = true;
      output.Complete = true;
      return output;
    }
  }
}

