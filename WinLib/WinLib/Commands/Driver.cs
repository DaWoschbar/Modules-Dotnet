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
using System.Security.Principal;
using System.ServiceProcess;

namespace Faction.Modules.Dotnet.Commands
{
    class Driver : Command
    {
        //pinvoke signatures required
        [DllImport("psapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool EnumDeviceDrivers(
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)] [In][Out] UInt32[] ddAddresses, 
            UInt32 arraySizeBytes,
            [MarshalAs(UnmanagedType.U4)] out UInt32 bytesNeeded
        );

        [DllImport("psapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetDeviceDriverBaseName(
            UInt32 ddAddress,
            StringBuilder ddBaseName,
            int baseNameStringSizeChars
        );

        // http://pinvoke.net/default.aspx/advapi32/CreateService.html
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateService(
            IntPtr hSCManager,
            string lpServiceName,
            string lpDisplayName,
            uint dwDesiredAccess,
            uint dwServiceType,
            uint dwStartType,
            uint dwErrorControl,
            string lpBinaryPathName,
            string lpLoadOrderGroup,
            string lpdwTagId,
            string lpDependencies,
            string lpServiceStartName,
            string lpPassword);

        /// <summary>
        /// Copies the driver to the proper system location if needed
        /// This does a file system check rather than enumeration check in case the driver
        /// is in a different location or we want to load a different version from elsewhere
        /// </summary>
        /// <param name="driverPath"></param>
        /// <returns></returns>
        protected void CopyDriverToRightLocationWin64(string driverPath)
        {
            string driverFileName;
            string driverWin64Path = "C:\\Windows\\System32\\drivers";

            if (!driverPath.Contains(driverWin64Path))
            {
                driverFileName = System.IO.Path.GetFileName(driverPath);
                driverWin64Path += driverFileName;
                System.IO.File.Copy(driverPath, driverWin64Path, true);
            }
        }

        /// <summary>
        /// Enumerates the installed drivers; this is done via the Win32 method as it returns the actual sys files instead of just the service names.
        /// The service controller method -> that can be used to return drivers if you call ServiceController.GetDevices but that does not return
        /// the actual sys file that is associated with the driver; this method does this and is needed if you're looking to load or work with an installed sys
        /// file via createfile and deviceiocontrol
        /// </summary>
        /// <param name="deviceDrivers"></param>
        protected void EnumerateDeviceDrivers(List<string> deviceDrivers)
        {
            bool success;
            UInt32 driverArraySize;
            UInt32 driverArraySizeBytes;
            UInt32[] ddAddresses;
            UInt32 driverArrayBytesNeeded;

            // Enumerate the installed drivers to see if this is already present
            // https://docs.microsoft.com/en-us/windows/win32/psapi/enumerating-all-device-drivers-in-the-system

            success = EnumDeviceDrivers(null, 0, out driverArrayBytesNeeded);
            
            if (!success)
            {
                throw new ArgumentException("Call to EnumDeviceDrivers Failed.");
            }
            
            if (driverArrayBytesNeeded == 0)
            {
                throw new ArgumentException("No drivers found on system, possibly caught by a sandbox.");
            }

            driverArraySize = driverArrayBytesNeeded / 4;
            driverArraySizeBytes = driverArrayBytesNeeded;
            ddAddresses = new UInt32[driverArraySize];
            success = EnumDeviceDrivers(ddAddresses, driverArraySizeBytes, out driverArrayBytesNeeded);

            if(!success)
            {
                throw new ArgumentException("Call to EnumDeviceDrivers Failed.");
            }

            // Get the Base Name of all device drivers present
            for (int i = 0; i < driverArraySize; i++)
            {
                StringBuilder sb = new StringBuilder(1024);
                int result = GetDeviceDriverBaseName(ddAddresses[i], sb, sb.Capacity);
                if (result!=0)
                {
                    deviceDrivers.Add(sb.ToString());
                }
            }
        }

        private void DriverPathValidation(string driverPath)
        {
            if (!Directory.Exists(driverPath) || File.Exists(driverPath))
            {
                throw new ArgumentException("Invalid driver path for DriverPath parameter.");
            }
        }

        public override string Name { get { return "driver"; } }

        public override CommandOutput Execute(Dictionary<string, string> Parameters = null)
        {
            // Reference information:
            // https://docs.microsoft.com/en-us/windows/win32/api/psapi/nf-psapi-enumdevicedrivers
            // https://docs.microsoft.com/en-us/windows/win32/psapi/device-driver-information
            // https://www.codeproject.com/Articles/293900/How-to-install-driver-dynamically-or-install-drive
            // https://docs.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-createfilea?redirectedfrom=MSDN
            // https://docs.microsoft.com/en-us/windows/win32/api/ioapiset/nf-ioapiset-deviceiocontrol?redirectedfrom=MSDN

            CommandOutput output = new CommandOutput();
            bool isElevated;
            List<string> deviceDrivers = new List<string>();

            // Check privileges, admin is required to work with drivers for this module
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            if (!isElevated)
            {
                output.Complete = true;
                output.Success = false;
                output.Message = "Insufficent privileges to execute module.";
                return output;
            }

            try
            {
                string operation = "Enumerate"; //set a default to Enumerate
                string driverPath = "";
                string method = "";
                operation = Parameters["Operation"];
                switch(operation)
                {
                    case "Enumerate":
                        EnumerateDeviceDrivers(deviceDrivers);
                        output.Message = JsonConvert.SerializeObject(deviceDrivers);
                        break;
                    case "Install":
                        driverPath = Parameters["DriverPath"];
                        DriverPathValidation(driverPath);
                        CopyDriverToRightLocationWin64(driverPath);
                        break;
                    case "Call":
                        driverPath = Parameters["DriverPath"];
                        method = Parameters["Method"];
                        DriverPathValidation(driverPath);
                        CopyDriverToRightLocationWin64(driverPath);
                        break;
                    case "Unload":
                        driverPath = Parameters["DriverPath"];
                        DriverPathValidation(driverPath);
                        break;
                    default:
                        break;
                }

                //string results = LoadAndCallDriver(driverPath, operation);
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
            //step 1: make sure that the driver is in C:\\Windows\\System32\\drivers

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

