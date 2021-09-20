☼ Brighty
=======

This is a program for Windows 10 that lets you quickly and easily change the brightness of your computer screen, without navigating through OSD menus or holding buttons for a long time.

<p align="center">
    <img src="https://i.imgur.com/cOLrkPD.png" alt="Brighty" />
</p>

## Installation
1. Install [.NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework) (included by default in Windows 10 version 1903 and later).
1. Go to the [latest Release](https://github.com/Aldaviva/Brighty/releases/latest) of this repository.
1. Under Assets, download the `Brightness.exe` file.

## Usage
1. Run the `Brightness.exe` program you downloaded earlier.
1. Type in a percentage for the desired brightness of the screen as an integer between `0` and `100` inclusive.
1. Press `Enter`, and your monitor's brightness should change.
1. Press `Esc` to close the program, or enter a new percentage to change it again.

## Notes
- Your monitor must support DDC/CI to be able to control it from your computer.
- This program uses the [Windows Monitor Configuration](https://docs.microsoft.com/en-us/windows/win32/monitor/monitor-configuration?redirectedfrom=MSDN) API.
- If you have multiple monitors connected to your computer, this program will change the brightness on all of them at once. It doesn't support setting the brightness of individual monitors, nor can it maintain a custom relationship between the brightness of multiple monitors (for example, if your left monitor is dimmer than your right monitor, and its brightness always needs to be set 5% higher in order for them to match each other). In this case, you should probably just fork this repository and put your custom logic in `DirectXVideoAccelerationMonitorService.set_brightness(uint)`.
- This program should run fine on Windows 7 as well, but the icon in the UI currently comes from the Segoe MDL2 Assets font that is only available in Windows 10, so the icon will look wrong in Windows 7.

## Building
- Visual Studio 2019 Community or later
- .NET Framework 4.8 Targeting Pack