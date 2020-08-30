#nullable enable

using System;
using System.Threading;
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
        private readonly RegistryKey registryKey;

        private bool isClosing;

        public MainWindow() {
            registryKey = Registry.LocalMachine.CreateSubKey(@"Software\Brighty", true);
            percentage = Convert.ToUInt32(registryKey.GetValue(MRU_REGISTRY_NAME, 0));

            InitializeComponent();

            Task.Run(() => {
                //this library takes about 52 ms initialize, so let the window appear before it's done and start it early in the background
                uint _ = monitorService.brightness;
            });
        }

        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);

            // line up with Launchy
            Top -= 3;
            Left -= 1;

            // really become the foreground window, even if mstsc was right behind Launchy
            this.globalActivate();

            brightnessInput.Focus();
            brightnessInput.Select(0, brightnessInput.Text.Length - "%".Length);
        }

        private void OnKeyDown(object sender, KeyEventArgs e) {
            e.Handled = true;

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault not trying to handle every single key
            switch (e.Key) {
                case Key.Escape when !isClosing:
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

        private Task setBrightness() => Task.Run(() => {
            monitorService.brightness = percentage;
            registryKey.SetValue(MRU_REGISTRY_NAME, percentage, RegistryValueKind.DWord);
        });

        private void OnDeactivated(object sender, EventArgs e) {
            if (!isClosing) {
                fadeOutAndClose();
            }
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