using System;
using System.Runtime.InteropServices;

namespace BrightyUI.Services; 

public partial class DxvaMonitorService {

    private const string DXVA2 = "dxva2.dll";

    /// <summary>
    /// Retrieves the physical monitors associated with an HMONITOR monitor handle.
    /// </summary>
    /// <param name="monitor">A monitor handle. Monitor handles are returned by several Multiple Display Monitor functions, including <c>EnumDisplayMonitors</c> and <c>MonitorFromWindow</c>, which are part of the graphics device interface (GDI).</param>
    /// <param name="physicalMonitorCount">Number of elements in pPhysicalMonitorArray. To get the required size of the array, call <see cref="GetNumberOfPhysicalMonitorsFromHMONITOR"/>.</param>
    /// <param name="physicalMonitors">Pointer to an array of <see cref="PhysicalMonitor"/> structures. The caller must allocate the array.</param>
    /// <returns>If the function succeeds, the return value is TRUE. If the function fails, the return value is FALSE. To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.</returns>
    /// <remarks><para>A single <c>HMONITOR</c> handle can be associated with more than one physical monitor. This function returns a handle and a text description for each physical monitor.</para>
    /// <para>When you are done using the monitor handles, close them by passing the <c>pPhysicalMonitorArray</c> array to the <see cref="DestroyPhysicalMonitors"/> function.</para></remarks>
    [DllImport(DXVA2)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr monitor, uint physicalMonitorCount, [Out] PhysicalMonitor[] physicalMonitors);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr MonitorFromPoint(Point point, MonitorOptions flags);

    /// <summary>
    /// Retrieves the number of physical monitors associated with an <c>HMONITOR</c> monitor handle. Call this function before calling <see cref="GetPhysicalMonitorsFromHMONITOR"/>.
    /// </summary>
    /// <param name="monitor">A monitor handle. Monitor handles are returned by several Multiple Display Monitor functions, including <c>EnumDisplayMonitors</c> and <c>MonitorFromWindow</c>, which are part of the graphics device interface (GDI).</param>
    /// <param name="physicalMonitorCount">Receives the number of physical monitors associated with the monitor handle.</param>
    /// <returns>If the function succeeds, the return value is TRUE. If the function fails, the return value is FALSE. To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.</returns>
    [DllImport(DXVA2)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr monitor, out uint physicalMonitorCount);

    /// <summary>
    /// Closes an array of physical monitor handles. Call this function to close an array of monitor handles obtained from the <see cref="GetPhysicalMonitorsFromHMONITOR"/> or <c>GetPhysicalMonitorsFromIDirect3DDevice9</c> function.
    /// </summary>
    /// <param name="physicalMonitorCount">Number of elements in the <c>pPhysicalMonitorArray</c> array.</param>
    /// <param name="physicalMonitors">Pointer to an array of <see cref="PhysicalMonitor"/> structures.</param>
    /// <returns>If the function succeeds, the return value is TRUE. If the function fails, the return value is FALSE. To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.</returns>
    [DllImport(DXVA2)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyPhysicalMonitors(uint physicalMonitorCount, ref PhysicalMonitor[] physicalMonitors);

    /// <summary>
    /// Retrieves a monitor's minimum, maximum, and current brightness settings.
    /// </summary>
    /// <param name="physicalMonitorHandle">Handle to a physical monitor. To get the monitor handle, call <see cref="GetPhysicalMonitorsFromHMONITOR"/> or <c>GetPhysicalMonitorsFromIDirect3DDevice9</c>.</param>
    /// <param name="minimumBrightness">Receives the monitor's minimum brightness.</param>
    /// <param name="currentBrightness">Receives the monitor's current brightness.</param>
    /// <param name="maximumBrightness">Receives the monitor's maximum brightness.</param>
    /// <returns>If the function succeeds, the return value is TRUE. If the function fails, the return value is FALSE. To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.</returns>
    /// <remarks><para>If this function is supported, the <c>GetMonitorCapabilities</c> function returns the <c>MC_CAPS_BRIGHTNESS</c> flag.</para>
    /// <para>This function takes about 40 milliseconds to return.</para>
    /// <para>The brightness setting is a continuous monitor setting. For more information, see <a href="https://docs.microsoft.com/en-us/windows/desktop/Monitor/using-the-high-level-monitor-configuration-functions">Using the High-Level Monitor Configuration Functions</a>.</para></remarks>
    [DllImport(DXVA2)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorBrightness(IntPtr physicalMonitorHandle, ref uint minimumBrightness, ref uint currentBrightness, ref uint maximumBrightness);

    /// <summary>
    /// Sets a monitor's brightness value. Increasing the brightness value makes the display on the monitor brighter, and decreasing it makes the display dimmer.
    /// </summary>
    /// <param name="physicalMonitorHandle">Handle to a physical monitor. To get the monitor handle, call <see cref="GetPhysicalMonitorsFromHMONITOR"/> or <c>GetPhysicalMonitorsFromIDirect3DDevice9</c>.</param>
    /// <param name="newBrightness">Brightness value. To get the monitor's minimum and maximum brightness values, call <see cref="GetMonitorBrightness"/>.</param>
    /// <returns>If the function succeeds, the return value is TRUE. If the function fails, the return value is FALSE. To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.</returns>
    /// <remarks><para>If this function is supported, the <c>GetMonitorCapabilities</c> function returns the <c>MC_CAPS_BRIGHTNESS</c> flag.</para>
    /// <para>This function takes about 50 milliseconds to return.</para>
    /// <para>The brightness setting is a continuous monitor setting. For more information, see <a href="https://docs.microsoft.com/en-us/windows/desktop/Monitor/using-the-high-level-monitor-configuration-functions">Using the High-Level Monitor Configuration Functions</a>.</para></remarks>
    [DllImport(DXVA2)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetMonitorBrightness(IntPtr physicalMonitorHandle, uint newBrightness);

    /// <summary>
    /// Saves the current monitor settings to the display's nonvolatile storage.
    /// </summary>
    /// <param name="physicalMonitorHandle">Handle to a physical monitor. To get the monitor handle, call <see cref="GetPhysicalMonitorsFromHMONITOR"/> or <c>GetPhysicalMonitorsFromIDirect3DDevice9</c>.</param>
    /// <returns>If the function succeeds, the return value is TRUE. If the function fails, the return value is FALSE. To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.</returns>
    /// <remarks><para>This function takes about 200 milliseconds to return.</para>
    /// <para>This high-level function is identical to the low-level function <c>SaveCurrentSettings</c>.</para></remarks>
    [DllImport(DXVA2)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SaveCurrentMonitorSettings(IntPtr physicalMonitorHandle);

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

}