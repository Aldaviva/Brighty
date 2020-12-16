﻿#nullable enable

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using BrightyUI.Services;
using Microsoft.Win32;

namespace BrightyUI {

    public partial class MainWindow: IDisposable {

        private const string MRU_REGISTRY_NAME = "mostRecentBrightnessPercentage";

        public uint percentage { get; set; }

        private readonly MonitorService monitorService = new DirectXVideoAccelerationMonitorService();
        private readonly RegistryKey    registryKey;

        private bool isClosing;

        public MainWindow() {
            registryKey = Registry.LocalMachine.CreateSubKey(@"Software\Brighty", true);
            percentage  = Convert.ToUInt32(registryKey.GetValue(MRU_REGISTRY_NAME, 0));

            InitializeComponent();

            Task.Run(() => {
                //this library takes about 52 ms to initialize, so let the window appear before it's done and start it early in the background
                uint _ = monitorService.brightness;
            });
        }

        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);

            Rect workArea = SystemParameters.WorkArea;
            Left -= 1;

            // In WPF, Window.WindowStartupLocation = CenterScreen rounds up, but we want to round down to be consistent with Launchy's window positioning calculations.
            Top = Math.Floor((workArea.Height - Height) / 2 + workArea.Top);

            // really become the foreground window, even if mstsc was right behind Launchy
            this.globalActivate();

            brightnessInput.Focus();
            selectNumericBrightnessText();
        }

        private void OnKeyDown(object sender, KeyEventArgs e) {
            e.Handled = true;
            bool isCtrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault not trying to handle every single key
            switch (e.Key) {
                case Key.Escape when !isClosing:
                case Key.F4 when isCtrlDown:
                case Key.W when isCtrlDown:
                case Key.System when e.SystemKey == Key.F4: // Alt+F4
                    fadeOutAndClose();
                    break;

                case Key.Enter when !Validation.GetHasError(brightnessInput):
                    setBrightness();
                    break;

                default:
                    e.Handled = false;
                    break;
            }
        }

        private void setBrightness() {
            monitorService.brightness = percentage;
            selectNumericBrightnessText();
            registryKey.SetValue(MRU_REGISTRY_NAME, percentage, RegistryValueKind.DWord);
        }

        private void OnDeactivated(object sender, EventArgs e) {
            if (!isClosing) {
                fadeOutAndClose();
            }
        }

        private void selectNumericBrightnessText() {
            brightnessInput.Select(0, brightnessInput.Text.Length - "%".Length);
        }

        private void fadeOutAndClose() {
            isClosing = true;
            var fadeOutAnimation = new DoubleAnimation(0, new Duration(TimeSpan.FromMilliseconds(175)));
            fadeOutAnimation.Completed += delegate { Close(); };
            BeginAnimation(OpacityProperty, fadeOutAnimation);
        }

        public void Dispose() {
            monitorService.Dispose();
            registryKey.Dispose();
        }

    }

}