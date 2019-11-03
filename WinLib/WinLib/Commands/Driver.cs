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

        public enum EFileAccess : uint
        {

            AccessSystemSecurity = 0x1000000,
            MaximumAllowed = 0x2000000,

            Delete = 0x10000,
            ReadControl = 0x20000,
            WriteDAC = 0x40000,
            WriteOwner = 0x80000,
            Synchronize = 0x100000,

            StandardRightsRequired = 0xF0000,
            StandardRightsRead = ReadControl,
            StandardRightsWrite = ReadControl,
            StandardRightsExecute = ReadControl,
            StandardRightsAll = 0x1F0000,
            SpecificRightsAll = 0xFFFF,

            FILE_READ_DATA = 0x0001,
            FILE_LIST_DIRECTORY = 0x0001,
            FILE_WRITE_DATA = 0x0002,
            FILE_ADD_FILE = 0x0002,
            FILE_APPEND_DATA = 0x0004,
            FILE_ADD_SUBDIRECTORY = 0x0004,
            FILE_CREATE_PIPE_INSTANCE = 0x0004,
            FILE_READ_EA = 0x0008,
            FILE_WRITE_EA = 0x0010,
            FILE_EXECUTE = 0x0020,
            FILE_TRAVERSE = 0x0020,
            FILE_DELETE_CHILD = 0x0040,
            FILE_READ_ATTRIBUTES = 0x0080,
            FILE_WRITE_ATTRIBUTES = 0x0100,

            GenericRead = 0x80000000,
            GenericWrite = 0x40000000,
            GenericExecute = 0x20000000,
            GenericAll = 0x10000000,

            SPECIFIC_RIGHTS_ALL = 0x00FFFF,
            FILE_ALL_ACCESS =
            StandardRightsRequired |
            Synchronize |
            0x1FF,

            FILE_GENERIC_READ =
            StandardRightsRead |
            FILE_READ_DATA |
            FILE_READ_ATTRIBUTES |
            FILE_READ_EA |
            Synchronize,

            FILE_GENERIC_WRITE =
            StandardRightsWrite |
            FILE_WRITE_DATA |
            FILE_WRITE_ATTRIBUTES |
            FILE_WRITE_EA |
            FILE_APPEND_DATA |
            Synchronize,

            FILE_GENERIC_EXECUTE =
            StandardRightsExecute |
              FILE_READ_ATTRIBUTES |
              FILE_EXECUTE |
              Synchronize
        }

        public enum EFileShare : uint
        {
            None = 0x00000000,
            Read = 0x00000001,
            Write = 0x00000002,
            Delete = 0x00000004
        }

        public enum ECreationDisposition : uint
        {
            New = 1,
            CreateAlways = 2,
            OpenExisting = 3,
            OpenAlways = 4,
            TruncateExisting = 5
        }

        public enum EFileAttributes : uint
        {
            Readonly = 0x00000001,
            Hidden = 0x00000002,
            System = 0x00000004,
            Directory = 0x00000010,
            Archive = 0x00000020,
            Device = 0x00000040,
            Normal = 0x00000080,
            Temporary = 0x00000100,
            SparseFile = 0x00000200,
            ReparsePoint = 0x00000400,
            Compressed = 0x00000800,
            Offline = 0x00001000,
            NotContentIndexed = 0x00002000,
            Encrypted = 0x00004000,
            Write_Through = 0x80000000,
            Overlapped = 0x40000000,
            NoBuffering = 0x20000000,
            RandomAccess = 0x10000000,
            SequentialScan = 0x08000000,
            DeleteOnClose = 0x04000000,
            BackupSemantics = 0x02000000,
            PosixSemantics = 0x01000000,
            OpenReparsePoint = 0x00200000,
            OpenNoRecall = 0x00100000,
            FirstPipeInstance = 0x00080000
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

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CreateFile(
            [MarshalAs(UnmanagedType.LPWStr)] string filename,
            [MarshalAs(UnmanagedType.U4)] FileAccess access,
            [MarshalAs(UnmanagedType.U4)] FileShare share,
            IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
            IntPtr templateFile); //ignored on file open

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool DeviceIoControl(
        IntPtr hDevice,
        uint dwIoControlCode,
        ref long InBuffer,
        int nInBufferSize,
        ref long OutBuffer,
        int nOutBufferSize,
        ref int pBytesReturned,
        [In] ref NativeOverlapped lpOverlapped);

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
                    (uint)SERVICE_ACCESS.SERVICE_ALL_ACCESS,
                    (uint)SERVICE_TYPE.SERVICE_KERNEL_DRIVER,
                    (uint)SERVICE_START.SERVICE_DEMAND_START,
                    (uint)SERVICE_ERROR.SERVICE_ERROR_IGNORE,
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
                SC_SERVICE = OpenService(SC_MANAGER, serviceName, (uint)SERVICE_ACCESS.SERVICE_STOP | (uint)SERVICE_ACCESS.DELETE | (uint)SERVICE_ACCESS.SERVICE_STOP);
                if (SC_SERVICE == IntPtr.Zero)
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

        private bool CallDriverMethod(string driverPath, uint controlCode, long deviceBuffer, ref long responseBuffer)
        {
            bool status = false;
            int dummy = 0;
            NativeOverlapped foo = new NativeOverlapped();
            GCHandle gch = GCHandle.Alloc(foo);
            IntPtr fileHandle = CreateFile(
                driverPath,
                FileAccess.ReadWrite,
                FileShare.None,
                IntPtr.Zero,
                FileMode.Open,
                FileAttributes.Normal,
                IntPtr.Zero
                );

            status = DeviceIoControl(
                fileHandle,
                controlCode,
                ref deviceBuffer,
                Marshal.SizeOf(deviceBuffer),
                ref responseBuffer,
                Marshal.SizeOf(responseBuffer),
                ref dummy,
                ref foo);

            gch.Free();
            return status;
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
                string operation = "Enumerate";
                string driverPath = string.Empty;
                string controlCode = string.Empty;
                string buffer = string.Empty;
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
                        controlCode = Parameters["ControlCode"];
                        buffer = Parameters["Buffer"];
                        long responseBuffer = 0;
                        if (driverPath == string.Empty)
                        {
                            throw new ArgumentException("DriverPath is empty.");
                        }
                        if (controlCode == string.Empty)
                        {
                            throw new ArgumentException("ControlCode is empty.");
                        }
                        if (buffer == string.Empty)
                        {
                            throw new ArgumentException("Buffer is empty.");
                        }
                        long inputBuffer = Convert.ToInt64(buffer);
                        DriverPathValidation(driverPath);
                        output.Success = CallDriverMethod(driverPath, Convert.ToUInt32(controlCode), inputBuffer, ref responseBuffer);
                        output.Message = JsonConvert.SerializeObject(responseBuffer);
                        break;
                    case "Unload":
                        serviceName = Parameters["ServiceName"];
                        if (serviceName == string.Empty)
                        {
                            throw new ArgumentException("ServiceName is empty.");
                        }
                        output.Success = StopAndUnloadDriver(serviceName);
                        break;
                    default:
                        output.Message = "No viable option selected";
                        output.Success = false;
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
    }
}