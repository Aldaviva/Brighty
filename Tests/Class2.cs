﻿using System;
using System.Runtime.InteropServices;
using Xunit;

namespace Tests {

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct PHYSICAL_MONITOR {

        public IntPtr hPhysicalMonitor;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szPhysicalMonitorDescription;

    }

    public class Class2 {

        [Fact]
        public void initAndGetBrightness() {
            var brightnessController = new BrightnessController(new POINT(0, 0));
            Assert.NotEqual((uint) 0, brightnessController.currentValue);
        }

        [Fact]
        public void initAndSetBrightnessTest() {
            var brightnessController = new BrightnessController(new POINT(0, 0));
            brightnessController.SetBrightness(50);
        }

    }

    public class BrightnessController: IDisposable {

        [DllImport("user32.dll", EntryPoint = "MonitorFromWindow")]
        public static extern IntPtr MonitorFromWindow([In] IntPtr hwnd, uint dwFlags);

        [DllImport("dxva2.dll", EntryPoint = "DestroyPhysicalMonitors")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyPhysicalMonitors(uint dwPhysicalMonitorArraySize, ref PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("dxva2.dll", EntryPoint = "GetNumberOfPhysicalMonitorsFromHMONITOR")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, ref uint pdwNumberOfPhysicalMonitors);

        [DllImport("dxva2.dll", EntryPoint = "GetPhysicalMonitorsFromHMONITOR")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("dxva2.dll", EntryPoint = "GetMonitorBrightness")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMonitorBrightness(IntPtr handle, ref uint minimumBrightness, ref uint currentBrightness, ref uint maxBrightness);

        [DllImport("dxva2.dll", EntryPoint = "SetMonitorBrightness")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetMonitorBrightness(IntPtr handle, uint newBrightness);

        private uint _physicalMonitorsCount = 1;
        private PHYSICAL_MONITOR[] _physicalMonitorArray;

        private IntPtr _firstMonitorHandle;

        private uint _minValue = 0;
        private uint _maxValue = 0;
        private uint _currentValue = 0;


        public BrightnessController(POINT point) {
            uint dwFlags = 0u;
            IntPtr ptr = Class1.MonitorFromPoint(point, 1);
            // if (!GetNumberOfPhysicalMonitorsFromHMONITOR(ptr, ref _physicalMonitorsCount)) {
            //     throw new Exception("Cannot get monitor count!");
            // }

            _physicalMonitorArray = new PHYSICAL_MONITOR[_physicalMonitorsCount];

            if (!GetPhysicalMonitorsFromHMONITOR(ptr, _physicalMonitorsCount, _physicalMonitorArray)) {
                throw new Exception("Cannot get phisical monitor handle!");
            }

            _firstMonitorHandle = _physicalMonitorArray[0].hPhysicalMonitor;

            if (!GetMonitorBrightness(_firstMonitorHandle, ref _minValue, ref _currentValue, ref _maxValue)) {
                throw new Exception("Cannot get monitor brightness!");
            }
        }

        public void SetBrightness(int newValue) {
            newValue = Math.Min(newValue, Math.Max(0, newValue));
            currentValue = (_maxValue - _minValue) * (uint) newValue / 100u + _minValue;
            SetMonitorBrightness(_firstMonitorHandle, currentValue);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (_physicalMonitorsCount > 0) {
                    DestroyPhysicalMonitors(_physicalMonitorsCount, ref _physicalMonitorArray);
                }
            }
        }

        public uint currentValue {
            get => _currentValue;
            set => _currentValue = value;
        }

    }

}