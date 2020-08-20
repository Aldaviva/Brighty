using System;
using System.Runtime.InteropServices;
using Xunit;

namespace Tests
{
    public class Class1
    {

        [Fact]
        public void MonitorFromWindowTest() {
            IntPtr actual = MonitorFromPoint(new POINT(0,0), 1);
            Assert.NotEqual(IntPtr.Zero, actual);
        }

        [Fact]
        public void GetNumberOfPhysicalMonitorsFromHMONITORTest() {
            IntPtr hmonitor = MonitorFromPoint(new POINT(0, 0), 1);
            bool success = GetNumberOfPhysicalMonitorsFromHMONITOR(hmonitor, out uint numberOfPhysicalMonitors);
            Assert.True(success, "success");
            Assert.Equal((uint) 1, numberOfPhysicalMonitors);
        }

        [Fact]
        public void GetPhysicalMonitorsFromHMONITORTest() {
            IntPtr hmonitor = MonitorFromPoint(new POINT(0, 0), 1);
            // var pPhysicalMonitorArray = new PHYSICAL_MONITOR[1];
            // pPhysicalMonitorArray[0] = new PHYSICAL_MONITOR();
            PHYSICAL_MONITOR2[] pPhysicalMonitorArray = new PHYSICAL_MONITOR2[1];
            bool success = GetPhysicalMonitorsFromHMONITOR(hmonitor, (uint) pPhysicalMonitorArray.Length, pPhysicalMonitorArray);
            Assert.True(success, "success");
            Assert.NotNull(pPhysicalMonitorArray[0].szPhysicalMonitorDescription);
            // Assert.NotEqual(IntPtr.Zero, pPhysicalMonitorArray[0].pPhysicalMonitor);
        }

        [DllImport("dxva2.dll", EntryPoint = "GetPhysicalMonitorsFromHMONITOR")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hmonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR2[] pPhysicalMonitorArray);

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromPoint(POINT point, uint dwFlags);

        [DllImport("Dxva2.dll")]
        private static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hmonitor, out uint pdwNumberOfPhysicalMonitors);

        
        

    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct PHYSICAL_MONITOR2 {

        public IntPtr pPhysicalMonitor;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szPhysicalMonitorDescription;

    }

    public struct POINT {

        public long x;
        public long y;

        public POINT(long x, long y) {
            this.x = x;
            this.y = y;
        }
    }
}
