#nullable enable

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SharpLib.MonitorConfig;

namespace Brighty {

    public interface MonitorService: IDisposable {

        uint brightness { get; set; }

    }

    public class MonitorServiceImpl: MonitorService {

        private Monitors? _monitors;

        public MonitorServiceImpl() {
            Task.Run(() => monitors); //eagerly scan monitors in the background when initializing
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
                monitors.VirtualMonitors.ForEach(virtualMonitor => {
                    virtualMonitor.PhysicalMonitors.ForEach(monitor => {
                        if (monitor.SupportsBrightness) {
                            SetLastError(0);

                            monitor.Brightness = monitor.Brightness.withCurrent(value);

                            int newError = Marshal.GetLastWin32Error();
                            if (newError != 0) {
                                monitors.Scan();
                                brightness = value;
                            }
                        }
                    });
                });
            }
        }

        private Monitors monitors {
            get {
                _monitors ??= new Monitors();

                if (_monitors.VirtualMonitors.Sum(monitor => monitor.PhysicalMonitors.Count) == 0) {
                    _monitors.Scan(); //takes a second to run
                }

                return _monitors;
            }
        }

        public void Dispose() {
            _monitors?.Dispose();
        }

        [DllImport("kernel32.dll")]
        private static extern void SetLastError(uint errorCode);

    }

}