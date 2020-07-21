#nullable enable

using SharpLib.MonitorConfig;

namespace BrightyUI {

    internal static class MonitorConfigExtensions {

        public static Setting withCurrent(this Setting setting, uint current) {
            return new Setting(setting.Min, current, setting.Max);
        }

    }

}