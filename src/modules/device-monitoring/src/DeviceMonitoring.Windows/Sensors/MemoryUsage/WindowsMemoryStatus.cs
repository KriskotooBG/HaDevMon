using System.ComponentModel;
using System.Runtime.InteropServices;

namespace DeviceMonitoring.Windows.Sensors.MemoryUsage
{
    internal readonly record struct MemoryStatus(ulong TotalPhysical, ulong AvailablePhysical)
    {
        public ulong UsedPhysical => TotalPhysical - AvailablePhysical;
    } 

    internal static partial class WindowsMemoryStatus
    {
        public static MemoryStatus Read()
        {
            var status = new MemoryStatusEx
            {
                Length = (uint)Marshal.SizeOf<MemoryStatusEx>()
            };

            var result = GlobalMemoryStatusEx(ref status);
            if (result == 0)
                throw new Win32Exception(Marshal.GetLastPInvokeError());

            return new MemoryStatus(status.TotalPhysical, status.AvailablePhysical);
        }

        [LibraryImport("kernel32.dll", SetLastError = true)]
        private static partial int GlobalMemoryStatusEx(ref MemoryStatusEx buffer);

        [StructLayout(LayoutKind.Sequential)]
        private struct MemoryStatusEx
        {
            public uint Length;
            public uint MemoryLoad;

            public ulong TotalPhysical;
            public ulong AvailablePhysical;

            public ulong TotalPageFile;
            public ulong AvailablePageFile;

            public ulong TotalVirtual;
            public ulong AvailableVirtual;

            public ulong AvailableExtendedVirtual;
        }
    }
}
