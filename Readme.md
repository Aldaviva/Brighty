☼ Brighty
=======

This is a plugin for [Launchy](https://www.launchy.net/) that lets you change the brightness of your computer screen.

## Installation
1. [Install .NET Framework 4.5.2 or later](https://dotnet.microsoft.com/download/dotnet-framework).
1. [Install Launchy](https://www.launchy.net/download.php#windows).
1. Download [Launchy# 1.3 or later](https://github.com/randrey/launchysharp/releases) (not the source code files).
    1. Extract the ZIP file into your Launchy installation directory.
    1. Make sure you extract `QtCore4.dll`, even though it already exists. The new version has an important bug fix.
1. Download the [latest release from this repository](https://github.com/Aldaviva/Brighty/releases).
    1. Extract the ZIP file into the `plugins` directory inside the Launchy installation directory.
1. Restart Launchy if it was already running.

## Usage
1. Open the Launchy window (`Alt`+`Space`).
1. Start typing `Brightness`, pressing `Tab` to autocomplete it.
1. Type in a percentage for the desired brightness of the screen, as an integer between `0` and `100`.
1. Press `Enter`, and your monitor's brightness should change.

<p align="center"><code>Alt+Space</code> <code>brig</code> <code>Tab</code> <code>50</code> <code>Enter</code></p>

## Notes
- Your monitor must support DDC/CI to be able to control it from your computer.
- This plugin uses the [Windows Monitor Configuration](https://docs.microsoft.com/en-us/windows/win32/monitor/monitor-configuration?redirectedfrom=MSDN) API, exposed through [SharpLib.MonitorConfig](https://github.com/Slion/SharpLibMonitorConfig).
- If you have multiple monitors connected to your computer, this plugin will change the brightness on all of them at once. It doesn't support setting the brightness of individual monitors, nor can it maintain a custom relationship between the brightness of multiple monitors (for example, if your left monitor is dimmer than your right monitor, and its brightness always needs to be set 5% higher in order for them to match each other). In this case, you should probably just fork this repository and put your custom logic in `MonitorServiceImpl.setBrightness(uint)`.

## Building
- Visual Studio 2019 Community
- .NET Framework 4.5.2 Targeting Pack
- The `Launchy#API.dll` reference comes from [randrey/launchysharp](https://github.com/randrey/launchysharp), which should be installed in the Launchy `plugins` directory. You will need to adjust the path of this reference depending on your Launchy installation directory.