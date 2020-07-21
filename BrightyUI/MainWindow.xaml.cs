#nullable enable

using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace BrightyUI {

    public partial class MainWindow: IDisposable {

        private const string MRU_REGISTRY_NAME = "mostRecentBrightnessPercentage";

        private readonly MonitorService monitorService = new MonitorServiceImpl();
        private readonly RegistryKey registryKey;

        public uint percentage { get; set; }

        public MainWindow() {
            InitializeComponent();

            registryKey = Registry.LocalMachine.CreateSubKey(@"Software\Brighty", true);
            percentage = Convert.ToUInt32(registryKey.GetValue(MRU_REGISTRY_NAME, 100));

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

        private void MainWindow_OnKeyUp(object sender, KeyEventArgs e) {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault not trying to handle every single key
            switch (e.Key) {
                case Key.Escape:
                    e.Handled = true;
                    Close();
                    break;

                case Key.Enter:
                    if (!Validation.GetHasError(brightnessInput)) {
                        e.Handled = true;
                        monitorService.brightness = percentage;
                        registryKey.SetValue(MRU_REGISTRY_NAME, percentage, RegistryValueKind.DWord);
                    }

                    break;

                default:
                    break;
            }
        }

        private void Window_Deactivated(object sender, EventArgs e) {
            Close();
        }

        public void Dispose() {
            monitorService.Dispose();
            registryKey.Dispose();
        }
    }

}