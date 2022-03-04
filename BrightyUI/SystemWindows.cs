using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

#nullable enable

namespace BrightyUI; 

/// <summary>
/// https://stackoverflow.com/a/11552906/979493
/// </summary>
public static class SystemWindows {

    private const uint SWP_NOSIZE     = 0x0001;
    private const uint SWP_NOMOVE     = 0x0002;
    private const uint SWP_SHOWWINDOW = 0x0040;

    /// <summary>
    /// Activate a window from anywhere by attaching to the foreground window
    /// </summary>
    public static void globalActivate(this Window w) {
        //Get the thread ID for this WPF window
        WindowInteropHelper selfWindowInteropHelper = new(w);
        uint                selfWindowThreadId      = GetWindowThreadProcessId(selfWindowInteropHelper.Handle, IntPtr.Zero);

        //Get the thread ID for the foreground window
        IntPtr foregroundWindow         = GetForegroundWindow();
        uint   foregroundWindowThreadId = GetWindowThreadProcessId(foregroundWindow, IntPtr.Zero);

        //Attach the input processing mechanism of the foreground window's thread to that of this window's thread
        AttachThreadInput(foregroundWindowThreadId, selfWindowThreadId, true);

        //Set this window to be on top of the z-order, displaying it but neither moving nor resizing it
        SetWindowPos(selfWindowInteropHelper.Handle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_SHOWWINDOW);

        //Detach the foreground window's thread from the current window's thread
        AttachThreadInput(foregroundWindowThreadId, selfWindowThreadId, false);

        //Show and activate the window
        if (w.WindowState == WindowState.Minimized) {
            w.WindowState = WindowState.Normal;
        }

        w.Show();
        w.Activate();
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr processId);

    [DllImport("user32.dll")]
    private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

}