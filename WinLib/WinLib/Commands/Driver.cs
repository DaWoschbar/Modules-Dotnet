using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Faction.Modules.Dotnet.Common;
using Newtonsoft.Json;
using SharpSploit.Generic;
using SharpSploit.Enumeration;
using System.IO;
using System.Threading;

namespace Faction.Modules.Dotnet.Commands
{
    class Driver : Command
    {
        public override string Name { get { return "driver"; } }
        public override CommandOutput Execute(Dictionary<string, string> Parameters = null)
        {
            // Check  to see loaded device drivers
            // https://docs.microsoft.com/en-us/windows/win32/api/psapi/nf-psapi-enumdevicedrivers
            // How this works: https://docs.microsoft.com/en-us/windows/win32/psapi/device-driver-information

            // Kernel32 Method:
            // https://docs.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-createfilea?redirectedfrom=MSDN
            // https://docs.microsoft.com/en-us/windows/win32/api/ioapiset/nf-ioapiset-deviceiocontrol?redirectedfrom=MSDN
            CommandOutput output = new CommandOutput();
            try
            {
                string driverPath = Parameters["DriverPath"];
                string action = Parameters["Action"];
                string results = LoadAndCallDriver(driverPath, action);
                output.Message = JsonConvert.SerializeObject(results);
                output.Success = true;
                output.Complete = true;
            }
            catch (Exception e)
            {
                output.Complete = true;
                output.Success = false;
                output.Message = e.Message;
            }
            return output;
        }

        private string LoadAndCallDriver(string driverPath, string action)
        {
            string results = "";
            try
            {
                // https://docs.microsoft.com/en-us/windows/win32/fileio/creating-and-opening-files
                IntPtr fileHandle = CreateFile(driverPath, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
            }
            catch (FileNotFoundException ex)
            {
                throw ex;
            }


            return results;
        }

        /// <summary>
        /// Pinvoke Signatures Below, needed to call CreateFileA to load the driver
        /// Needed to then call deviceIOControl to interact with it
        /// https://www.pinvoke.net/default.aspx/kernel32/CreateFile.html
        /// </summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateFile(
             [MarshalAs(UnmanagedType.LPTStr)] string filename,
             [MarshalAs(UnmanagedType.U4)] FileAccess access,
             [MarshalAs(UnmanagedType.U4)] FileShare share,
             IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
             [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
             [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
             IntPtr templateFile); //ignored on file open

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr CreateFileA(
             [MarshalAs(UnmanagedType.LPStr)] string filename,
             [MarshalAs(UnmanagedType.U4)] FileAccess access,
             [MarshalAs(UnmanagedType.U4)] FileShare share,
             IntPtr securityAttributes,
             [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
             [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
             IntPtr templateFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateFileW(
             [MarshalAs(UnmanagedType.LPWStr)] string filename,
             [MarshalAs(UnmanagedType.U4)] FileAccess access,
             [MarshalAs(UnmanagedType.U4)] FileShare share,
             IntPtr securityAttributes,
             [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
             [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
             IntPtr templateFile);

        /// <summary>
        /// https://www.pinvoke.net/default.aspx/kernel32/DeviceIoControl.html
        /// </summary>
        /// <param name="hDevice"></param>
        /// <param name="dwIoControlCode"></param>
        /// <param name="InBuffer"></param>
        /// <param name="nInBufferSize"></param>
        /// <param name="OutBuffer"></param>
        /// <param name="nOutBufferSize"></param>
        /// <param name="pBytesReturned"></param>
        /// <param name="lpOverlapped"></param>
        /// <returns></returns>
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DeviceIoControl(
                IntPtr hDevice,
                uint dwIoControlCode,
                ref long InBuffer,
                int nInBufferSize,
                ref long OutBuffer,
                int nOutBufferSize,
                ref int pBytesReturned,
                [In] ref NativeOverlapped lpOverlapped);

    }
}

