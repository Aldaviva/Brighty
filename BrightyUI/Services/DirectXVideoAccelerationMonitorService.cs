using System;
using System.Runtime.InteropServices;

#nullable enable

namespace BrightyUI.Services {

    public class DirectXVideoAccelerationMonitorService: MonitorService {

        private readonly object scanLock = new object();

        private uint currentBrightness;

        private PhysicalMonitor[]? _monitors;
        private PhysicalMonitor[] monitors {
            get {
                lock (scanLock) {
                    if (_monitors == null) {
                        initialize();
                    }
                }

                return _monitors!;
            }
        }

        public void initialize() {
            IntPtr monitor = MonitorFromPoint(new Point(0, 0), MonitorOptions.MONITOR_DEFAULTTOPRIMARY);

            if (!GetNumberOfPhysicalMonitorsFromHMONITOR(monitor, out uint monitorCount)) {
                throw new ApplicationException($"Could not get number of physical monitors from monitor {monitor}.");
            }

            _monitors = new PhysicalMonitor[monitorCount];
            GetPhysicalMonitorsFromHMONITOR(monitor, monitorCount, monitors);

            uint minBrightness = 0;
            uint maxBrightness = 0;
            GetMonitorBrightness(monitors[0].handle, ref minBrightness, ref currentBrightness, ref maxBrightness); //return brightness from first monitor, which may not be the primary
        }

        public uint brightness {
            get {
                PhysicalMonitor[] _ = monitors; //ensure initialized
                return currentBrightness;
            }
            set {
                value = Math.Min(Math.Max(0, value), 100);

                foreach (PhysicalMonitor physicalMonitor in monitors) {
                    SetMonitorBrightness(physicalMonitor.handle, value);
                }

                currentBrightness = value;
            }
        }

        private void releaseUnmanagedResources() {
            if (_monitors?.Length > 0) {
                DestroyPhysicalMonitors((uint) monitors.Length, ref _monitors);
            }
        }

        private void dispose(bool disposing) {
            releaseUnmanagedResources();
            if (disposing) {
                _monitors = null;
            }
        }

        public void Dispose() {
            dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DirectXVideoAccelerationMonitorService() {
            dispose(false);
        }

        #region external

        [DllImport("dxva2.dll")]
        private static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr monitor, uint physicalMonitorCount, [Out] PhysicalMonitor[] physicalMonitors);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr MonitorFromPoint(Point point, MonitorOptions flags);

        [DllImport("dxva2.dll")]
        private static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr monitor, out uint physicalMonitorCount);

        [DllImport("dxva2.dll")]
        public static extern bool DestroyPhysicalMonitors(uint physicalMonitorCount, ref PhysicalMonitor[] physicalMonitors);

        [DllImport("dxva2.dll")]
        public static extern bool GetMonitorBrightness(IntPtr physicalMonitorHandle, ref uint minimumBrightness, ref uint currentBrightness, ref uint maxBrightness);

        [DllImport("dxva2.dll")]
        public static extern bool SetMonitorBrightness(IntPtr physicalMonitorHandle, uint newBrightness);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct PhysicalMonitor {

            public IntPtr handle;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public readonly string description;

        }

        [StructLayout(LayoutKind.Sequential)]
        public readonly struct Point {

            public readonly int x;
            public readonly int y;

            public Point(int x, int y) {
                this.x = x;
                this.y = y;
            }

        }

        public enum MonitorOptions: uint {

            MONITOR_DEFAULTTONULL = 0x00000000,
            MONITOR_DEFAULTTOPRIMARY = 0x00000001,
            MONITOR_DEFAULTTONEAREST = 0x00000002

        }

        #endregion

    }

}