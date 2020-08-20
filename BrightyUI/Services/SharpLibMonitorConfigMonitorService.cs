using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using KoKo.Property;
using SharpLib.MonitorConfig;

#nullable enable

namespace BrightyUI.Services {

    public class SharpLibMonitorConfigMonitorServiceImpl: MonitorService {

        private Monitors? _monitors;

        private readonly object scanLock = new object();

        private readonly StoredProperty<bool> _isInitialized = new StoredProperty<bool>();
        public Property<bool> isInitialized { get; }

        public SharpLibMonitorConfigMonitorServiceImpl() {
            isInitialized = _isInitialized;
        }

        public uint brightness {
            get {
                return monitors.VirtualMonitors
                    .Find(monitor => monitor.IsPrimary())
                    .PhysicalMonitors
                    .First(monitor => monitor.SupportsBrightness)
                    .Brightness
                    .Current;
            }
            set {
                value = Math.Min(Math.Max(0, value), 100);

                IEnumerable<PhysicalMonitor> monitorsToSet = monitors.VirtualMonitors
                    .SelectMany(monitor => monitor.PhysicalMonitors)
                    .Where(monitor => monitor.SupportsBrightness);

                foreach (PhysicalMonitor monitor in monitorsToSet) {
                    SetLastError(0);

                    monitor.Brightness = monitor.Brightness.withCurrent(value);

                    int error = Marshal.GetLastWin32Error();
                    if (error != 0) {
                        monitors.Scan();
                        brightness = value;
                    }
                }
            }
        }

        private Monitors monitors {
            get {
                lock (scanLock) {
                    _monitors ??= new Monitors();

                    if (_monitors.VirtualMonitors.Sum(monitor => monitor.PhysicalMonitors.Count) == 0) {
                        _isInitialized.Value = false;
                        _monitors.Scan(); //takes a second to run
                        _isInitialized.Value = true;
                    }

                    return _monitors;
                }
            }
        }

        public void Dispose() {
            _monitors?.Dispose();
        }

        [DllImport("kernel32.dll")]
        private static extern void SetLastError(uint errorCode);

    }

}