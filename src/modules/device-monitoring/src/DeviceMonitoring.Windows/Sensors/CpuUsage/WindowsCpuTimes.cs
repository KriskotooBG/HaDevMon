using System.ComponentModel;
using System.Runtime.InteropServices;

namespace DeviceMonitoring.Windows.Sensors.CpuUsage
{
    internal readonly record struct CpuTimes(ulong Idle, ulong Kernel, ulong User);

    internal static partial class WindowsCpuTimes
    {
        public static CpuTimes Read()
        {
            var result = GetSystemTimes(out var idle, out var kernel, out var user);
            if (result == 0)
                throw new Win32Exception(Marshal.GetLastPInvokeError());

            return new CpuTimes(idle.ToUInt64(), kernel.ToUInt64(), user.ToUInt64());
        }



        [LibraryImport("kernel32.dll", SetLastError = true)]
        private static partial int GetSystemTimes(out FileTime idleTime, out FileTime kernelTime, out FileTime userTime);

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct FileTime
        {
            private readonly uint _low;
            private readonly uint _high;

            public ulong ToUInt64() => ((ulong)_high << 32) | _low;
        }
    }
}
