using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using KoKo.Property;

#nullable enable

namespace BrightyUI.Services {

    public class DirectXVideoAccelerationMonitorService: MonitorService {

        private PhysicalMonitor[]? _monitors;

        private readonly object scanLock = new object();

        public Property<bool> isInitialized { get; }
        private readonly StoredProperty<bool> _isInitialized = new StoredProperty<bool>();

        private uint currentBrightness;

        public DirectXVideoAccelerationMonitorService() {
            isInitialized = _isInitialized;
        }

        public uint brightness {
            get {
                PhysicalMonitor[] _ = monitors; //scan if not already scanned
                return currentBrightness;
            }
            set {
                value = Math.Min(Math.Max(0, value), 100);

                // scale values according to the first monitor's brightness range, which may not be the same for all monitors
                foreach (PhysicalMonitor physicalMonitor in monitors) {
                    SetMonitorBrightness(physicalMonitor.handle, value);
                }

                currentBrightness = value;
            }
        }

        private PhysicalMonitor[] monitors {
            get {
                lock (scanLock) {
                    if (_monitors == null) {
                        _isInitialized.Value = false;
                        scan();
                        _isInitialized.Value = true;
                    }

                    return _monitors!;
                }
            }
        }

        private void scan() {
            var stopwatch = Stopwatch.StartNew();
            IntPtr monitor = MonitorFromPoint(new Point(0, 0), MonitorOptions.MONITOR_DEFAULTTOPRIMARY);
            if (!GetNumberOfPhysicalMonitorsFromHMONITOR(monitor, out uint monitorCount)) {
                throw new ApplicationException($"Could not get number of physical monitors from monitor {monitor}.");
            }

            _monitors = new PhysicalMonitor[monitorCount];
            GetPhysicalMonitorsFromHMONITOR(monitor, monitorCount, _monitors);

            uint minBrightness = 0;
            uint maxBrightness = 0;
            GetMonitorBrightness(monitors[0].handle, ref minBrightness, ref currentBrightness, ref maxBrightness); //return brightness from first monitor, which may not be the primary

            stopwatch.Stop();
            Trace.WriteLine($"Monitors scanned in {stopwatch.ElapsedMilliseconds:N0} ms.");

            _isInitialized.Value = true;
        }

        private void releaseUnmanagedResources() {
            if (_monitors?.Length > 0) {
                DestroyPhysicalMonitors((uint) monitors.Length, ref _monitors);
            }
        }

        ~DirectXVideoAccelerationMonitorService() {
            releaseUnmanagedResources();
        }

        public void Dispose() {
            releaseUnmanagedResources();
            GC.SuppressFinalize(this);
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
            public string description;

        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Point {

            public int x;
            public int y;

            public Point(int x, int y) {
                this.x = x;
                this.y = y;
            }

        }

        public enum MonitorOptions : uint
        {
            MONITOR_DEFAULTTONULL = 0x00000000,
            MONITOR_DEFAULTTOPRIMARY = 0x00000001,
            MONITOR_DEFAULTTONEAREST = 0x00000002
        }

        #endregion
    }

}