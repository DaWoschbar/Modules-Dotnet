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

        public static bool RegistryDelete(string valuePath, RegistryKey registryKey, string type)
        {
            switch (type)
            {
                case "DeleteValue":
                    registryKey.DeleteValue(valuePath);
                    break;
                case "DeleteSubKey":
                    registryKey.DeleteSubKey(valuePath);
                    break;
                case "DeleteTree":
                    registryKey.DeleteSubKeyTree(valuePath);
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
                string type = "DeleteValue";

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
                    if (Parameters.ContainsKey("Type"))
                    {
                        type = Parameters["Type"];
                    }
                    RegistryKey registryKey = null;
                    if (regPath.StartsWith("HKLM"))
                    {
                        registryKey = Registry.LocalMachine;
                    }
                    else if (regPath.StartsWith("HKCU"))
                    {
                        registryKey = Registry.CurrentUser;
                    else
                    {
                        output.Success = false;
                        output.Message = $"Registry path does not begin with HKLM or HKCU. No idea what to do with this.";
                    }

                    if (registryKey != null)
                    {
                        string valuePath = regPath.Remove(0, 5);
                        if (RegistryDelete(valuePath, registryKey, type))
                        {
                            output.Success = true;
                            output.Message = $"Deleted registry value at {regPath}";

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

