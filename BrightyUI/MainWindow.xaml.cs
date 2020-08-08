#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
            percentage  = Convert.ToUInt32(registryKey.GetValue(MRU_REGISTRY_NAME, 100));

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
            brightnessInput.Focus();
            brightnessInput.Select(0, brightnessInput.Text.Length - "%".Length);
        }

        private void OnKeyUp(object sender, KeyEventArgs e) {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault not trying to handle every single key
            switch (e.Key) {
                case Key.Escape:
                    isClosing = true;
                    Close();
                    e.Handled = true;
                    break;

                case Key.Enter:
                    if (!Validation.GetHasError(brightnessInput)) {
                        monitorService.brightness = percentage;
                        registryKey.SetValue(MRU_REGISTRY_NAME, percentage, RegistryValueKind.DWord);
                        e.Handled = true;
                    }

                    break;

                default:
                    break;
            }
        }

        private void OnDeactivated(object sender, EventArgs e) {
            if (!isClosing) {
                isClosing = true;
                Close();
            }
        }

        public void Dispose() {
            monitorService.Dispose();
            registryKey.Dispose();
        }

    }

}