﻿using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace BrightyUI {

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
            //Get the process ID for this window's thread
            var interopHelper = new WindowInteropHelper(w);
            uint selfWindowThreadId = GetWindowThreadProcessId(interopHelper.Handle, IntPtr.Zero);

            //Get the process ID for the foreground window's thread
            IntPtr foregroundWindow = GetForegroundWindow();
            uint foregroundWindowThreadId = GetWindowThreadProcessId(foregroundWindow, IntPtr.Zero);

            //Attach this window's thread to the current window's thread
            AttachThreadInput(foregroundWindowThreadId, selfWindowThreadId, true);

            //Set the window position
            SetWindowPos(interopHelper.Handle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_SHOWWINDOW);

            //Detach this window's thread from the current window's thread
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

}