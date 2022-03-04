#nullable enable

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using BrightyUI.Properties;
using BrightyUI.Services;
using Microsoft.Win32;

namespace BrightyUI; 

public partial class MainWindow: IDisposable, INotifyPropertyChanged {

    private const string MRU_REGISTRY_NAME = "mostRecentBrightnessPercentage";

    private uint _percentage;

    public uint percentage {
        get => _percentage;
        set {
            if (value == _percentage) return;
            _percentage = value;
            onPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly MonitorService monitorService = new DxvaMonitorService();
    private readonly RegistryKey    registryKey;

    private int isClosing = Convert.ToInt32(false); //int instead of bool because there are no atomic operations on bool

    public MainWindow() {
        registryKey = Registry.CurrentUser.CreateSubKey(@"Software\Ben Hutchison\Brighty", true);
        percentage  = Convert.ToUInt32(registryKey.GetValue(MRU_REGISTRY_NAME, 0));

        InitializeComponent();

        _ = Task.Run(() => {
            //this library takes about 52 ms to initialize, so let the window appear before it's done and start it early in the background
            uint brightnessFromMonitor = monitorService.brightness;
            Dispatcher.Invoke(() => {
                percentage = brightnessFromMonitor;
                selectNumericBrightnessText();
            });
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
            case Key.Escape:                            // Esc
            case Key.F4 when isCtrlDown:                // Ctrl+F4
            case Key.W when isCtrlDown:                 // Ctrl+W
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
        percentage                = monitorService.brightness; // in case clipping occurred
        selectNumericBrightnessText();
        registryKey.SetValue(MRU_REGISTRY_NAME, percentage, RegistryValueKind.DWord);
    }

    private void OnDeactivated(object sender, EventArgs e) {
        fadeOutAndClose();
    }

    private void selectNumericBrightnessText() {
        brightnessInput.Select(0, brightnessInput.Text.Length - "%".Length);
    }

    private void fadeOutAndClose() {
        bool wasClosing = Convert.ToBoolean(Interlocked.CompareExchange(ref isClosing, 1, 0));
        if (!wasClosing) {
            DoubleAnimation? fadeOutAnimation = new(0, new Duration(TimeSpan.FromMilliseconds(175)));
            fadeOutAnimation.Completed += delegate { Close(); };
            BeginAnimation(OpacityProperty, fadeOutAnimation);
        }
    }

    protected override void OnClosed(EventArgs e) {
        base.OnClosed(e);
        Dispose();
    }

    public void Dispose() {
        monitorService.Dispose();
        registryKey.Dispose();
    }

    [NotifyPropertyChangedInvocator]
    protected virtual void onPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}