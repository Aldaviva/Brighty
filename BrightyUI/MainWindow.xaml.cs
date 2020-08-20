#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using KoKo.Property;
using Microsoft.Win32;

namespace BrightyUI {

    public partial class MainWindow: IDisposable {

        private const string MRU_REGISTRY_NAME = "mostRecentBrightnessPercentage";

        public uint percentage { get; set; }
        public Property<bool> isInitialized { get; }

        private readonly MonitorService monitorService = new MonitorServiceImpl();
        private readonly RegistryKey registryKey;

        private bool isClosing;

        public MainWindow() {
            registryKey = Registry.LocalMachine.CreateSubKey(@"Software\Brighty", true);
            percentage = Convert.ToUInt32(registryKey.GetValue(MRU_REGISTRY_NAME, 0));

            isInitialized = new PassthroughProperty<bool>(monitorService.isInitialized) {
                EventSynchronizationContext = SynchronizationContext.Current
            };

            InitializeComponent();

            Task.Run(() => {
                //this library takes a while to initialize, so start it early in the background
                uint _ = monitorService.brightness;
            });
        }

        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);

            // line up with Launchy
            Top -= 2;

            // really become the foreground window, even if mstsc was right behind Launchy
            Activate();
            Focus();

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

        private Task setBrightness() {
            //async to avoid deadlock with MonitorService setting isInitialized=true and trying to update the UI
            return Task.Run(() => {
                monitorService.brightness = percentage;
                registryKey.SetValue(MRU_REGISTRY_NAME, percentage, RegistryValueKind.DWord);
            });
        }

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