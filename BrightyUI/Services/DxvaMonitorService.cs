using System;
using System.Runtime.InteropServices;

#nullable enable

namespace BrightyUI.Services {

    public class DxvaMonitorService: MonitorService {

        private const uint DEFAULT_MINIMUM_BRIGHTNESS = 0;
        private const uint DEFAULT_MAXIMUM_BRIGHTNESS = 100;

        private readonly object scanLock = new();

        private uint currentBrightness;
        private uint minimumBrightness;
        private uint maximumBrightness;

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

        private void initialize() {
            IntPtr hmonitor = MonitorFromPoint(new Point(0, 0), MonitorOptions.MONITOR_DEFAULTTOPRIMARY);

            GetNumberOfPhysicalMonitorsFromHMONITOR(hmonitor, out uint monitorCount);

            _monitors = new PhysicalMonitor[monitorCount];
            GetPhysicalMonitorsFromHMONITOR(hmonitor, monitorCount, _monitors);

            //return brightness from first monitor, which may not be the primary, or the same for all monitors
            GetMonitorBrightness(monitors[0].handle, ref minimumBrightness, ref currentBrightness, ref maximumBrightness);

            if ((minimumBrightness != DEFAULT_MINIMUM_BRIGHTNESS) || (maximumBrightness != DEFAULT_MAXIMUM_BRIGHTNESS)) {
                currentBrightness = (uint) ((double) (currentBrightness - minimumBrightness) / (maximumBrightness - minimumBrightness));
            }
        }

        public uint brightness {
            get {
                PhysicalMonitor[] _ = monitors; //ensure initialized
                return currentBrightness;
            }
            set {
                value = Math.Min(Math.Max(DEFAULT_MINIMUM_BRIGHTNESS, value), DEFAULT_MAXIMUM_BRIGHTNESS); // 0 <= value <= 100

                if ((minimumBrightness != DEFAULT_MINIMUM_BRIGHTNESS) || (maximumBrightness != DEFAULT_MAXIMUM_BRIGHTNESS)) {
                    value = (uint) ((double) value * (maximumBrightness - minimumBrightness) + minimumBrightness);
                }

                foreach (PhysicalMonitor physicalMonitor in monitors) {
                    SetMonitorBrightness(physicalMonitor.handle, value);
                }

                currentBrightness = value;
            }
        }

        private void releaseUnmanagedResources() {
            if (_monitors?.Length > 0) {
                DestroyPhysicalMonitors((uint) _monitors.Length, ref _monitors);
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

        ~DxvaMonitorService() {
            dispose(false);
        }

        #region external

        [DllImport("dxva2.dll")]
        private static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr monitor, uint physicalMonitorCount, [Out] PhysicalMonitor[] physicalMonitors);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr MonitorFromPoint(Point point, MonitorOptions flags);

        [DllImport("dxva2.dll")]
        private static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr monitor, out uint physicalMonitorCount);

        [DllImport("dxva2.dll")]
        private static extern bool DestroyPhysicalMonitors(uint physicalMonitorCount, ref PhysicalMonitor[] physicalMonitors);

        [DllImport("dxva2.dll")]
        private static extern bool GetMonitorBrightness(IntPtr physicalMonitorHandle, ref uint minimumBrightness, ref uint currentBrightness, ref uint maximumBrightness);

        [DllImport("dxva2.dll")]
        private static extern bool SetMonitorBrightness(IntPtr physicalMonitorHandle, uint newBrightness);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private readonly struct PhysicalMonitor {

            public readonly IntPtr handle;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            private readonly string description;

        }

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct Point {

            private readonly int x;
            private readonly int y;

            public Point(int x, int y) {
                this.x = x;
                this.y = y;
            }

        }

        private enum MonitorOptions: uint {

            MONITOR_DEFAULTTONULL    = 0x0,
            MONITOR_DEFAULTTOPRIMARY = 0x1,
            MONITOR_DEFAULTTONEAREST = 0x2

        }

        #endregion

    }

}