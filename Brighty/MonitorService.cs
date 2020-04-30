#nullable enable

using System;
using System.Threading.Tasks;
using SharpLib.MonitorConfig;

namespace Brighty {

    public interface MonitorService: IDisposable {

        void setBrightness(uint brightness);

    }

    public class MonitorServiceImpl: MonitorService {

        private Monitors? _monitors;

        public MonitorServiceImpl() {
            Task.Run(() => monitors); //eagerly scan monitors in the background when initializing
        }

        public void Dispose() {
            _monitors?.Dispose();
        }

        public void setBrightness(uint brightness) {
            monitors.VirtualMonitors.ForEach(virtualMonitor => {
                virtualMonitor.PhysicalMonitors.ForEach(monitor => {
                    if (monitor.SupportsBrightness) {
                        monitor.Brightness = monitor.Brightness.withCurrent(brightness);
                    }
                });
            });
        }

        private Monitors monitors {
            get {
                if (_monitors == null) {
                    _monitors = new Monitors();
                    _monitors.Scan(); //takes a second to run
                }

                return _monitors;
            }
        }

    }

}