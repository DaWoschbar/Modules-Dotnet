using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Faction.Modules.Dotnet.Common;
using Microsoft.Win32;

namespace Faction.Modules.Dotnet.Commands
{
    class Reg : Command
    {

        // The following two functions were taken and modified from SharpSploit
        public static string RegistryRead(string RegPath)
        {
            var split = RegPath.Split(Path.DirectorySeparatorChar);
            string valueName = split[split.Length - 1];
            string keyName = RegPath.Substring(0, RegPath.IndexOf(valueName));
            object reg = Registry.GetValue(keyName, valueName, null);
            if (reg == null)
            {
                return null;
            }
            return reg.ToString();
        }

        public static bool RegistryWrite(string RegPath, object Value)
        {
            var split = RegPath.Split(Path.DirectorySeparatorChar);
            string valueName = split[split.Length - 1];
            string keyName = RegPath.Substring(0, RegPath.IndexOf(valueName));
            Registry.SetValue(keyName, valueName, Value);
            return true;
        }

        public static bool RegistryDelete(string value, RegistryKey registryKey, string type)
        {
            switch (type)
            {
                case "Value":
                    registryKey.DeleteValue(value);
                    break;
                case "SubKey":
                    registryKey.DeleteSubKey(value);
                    break;
                default:
                    break;
            }
            return true;
        }



        public override string Name { get { return "reg"; } }
        public override CommandOutput Execute(Dictionary<string, string> Parameters = null)
        {
            CommandOutput output = new CommandOutput();
            try
            {
                string operation = Parameters["Operation"].ToLower();
                string regPath = Parameters["Path"];
                string value = "";
                string type = "";

                if (Parameters.ContainsKey("Value"))
                {
                    value = Parameters["Value"];
                }

                if (operation == "write")
                {
                    if (String.IsNullOrEmpty(value))
                    {
                        output.Success = false;
                        output.Message = "Can not perform a write operation without a value. Use /value:<value> to specify a value.";
                    }
                    {
                        if (RegistryWrite(regPath, value))
                        {
                            output.Success = true;
                            output.Message = $"{regPath} updated with value: {value}";

                            //IOC regWriteIOC = new IOC("registry", regPath, "modify", output.Message);
                            //output.IOCs.Add(regWriteIOC);
                        }
                    }
                }
                else if (operation == "read")
                {
                    output.Success = true;
                    output.Message = RegistryRead(regPath);
                }
                else if (operation == "delete")
                {
                    // make sure they have selected the type of delete to do
                    if (Parameters.ContainsKey("Type") && Parameters.ContainsKey("Path"))
                    {
                        type = Parameters["Type"];
                        regPath = Parameters["Path"];
                    }
                    else
                    {
                        output.Success = false;
                        output.Message = "Missing parameters for registry deletion: type and path";
                        return output;
                    }



                    RegistryKey registryKey = null;
                    // This properly gets a reference to the registry key for key or value deletion
                    if (regPath.StartsWith("HKLM"))
                    {
                        string valuePath = regPath.Remove(0, 5);
                        registryKey = Registry.LocalMachine.OpenSubKey(valuePath, true);
                    }
                    else if (regPath.StartsWith("HKCU"))
                    {
                        string valuePath = regPath.Remove(0, 5);
                        registryKey = Registry.CurrentUser.OpenSubKey(valuePath, true);
                    }
                    else
                    {
                        output.Success = false;
                        output.Message = $"Registry path does not begin with HKLM or HKCU. No idea what to do with this.";
                    }

                    if (registryKey != null)
                    {
                        string valuePath = regPath.Remove(0, 5);
                        if (RegistryDelete(value, registryKey, type))
                        {
                            output.Success = true;
                            output.Message = $"Deleted registry {type} at {regPath}";

                            //IOC regDeleteIOC = new IOC("registry", regPath, "delete", output.Message);
                            //output.IOCs.Add(regDeleteIOC);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Message = e.Message;
            }
            output.Complete = true;
            return output;
        }
    }
}

