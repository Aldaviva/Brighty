using System;

#nullable enable

namespace BrightyUI.Services; 

public partial class DxvaMonitorService: MonitorService {

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

        if (minimumBrightness != DEFAULT_MINIMUM_BRIGHTNESS || maximumBrightness != DEFAULT_MAXIMUM_BRIGHTNESS) {
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

            if (minimumBrightness != DEFAULT_MINIMUM_BRIGHTNESS || maximumBrightness != DEFAULT_MAXIMUM_BRIGHTNESS) {
                value = (uint) ((double) value * (maximumBrightness - minimumBrightness) + minimumBrightness); // min <= value <= max
            }

            foreach (PhysicalMonitor physicalMonitor in monitors) {
                SetMonitorBrightness(physicalMonitor.handle, value);
                SaveCurrentMonitorSettings(physicalMonitor.handle);
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

}