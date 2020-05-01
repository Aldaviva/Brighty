#nullable enable

using System;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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

        public void Dispose() {
            _monitors?.Dispose();
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
                monitors.VirtualMonitors.ForEach(virtualMonitor => {
                    virtualMonitor.PhysicalMonitors.ForEach(monitor => {
                        if (monitor.SupportsBrightness) {
                            monitor.Brightness = monitor.Brightness.withCurrent(value);
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

    }

}