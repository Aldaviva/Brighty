#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LaunchySharp;

namespace Brighty {

    public class BrightyPlugin: IPlugin, IDisposable {

        private const string COMMAND_TEXT = "Brightness";

        private readonly MonitorService monitorService = new MonitorServiceImpl();

        private IPluginHost? pluginHost;
        private ICatItemFactory? catalogItemFactory;
        private string? pluginPath;

        public void init(IPluginHost _pluginHost) {
            if (_pluginHost != null) {
                pluginHost = _pluginHost;
                catalogItemFactory = _pluginHost.catItemFactory();
            }
        }

        public uint getID() {
            return pluginHost?.hash(getName()) ?? 0;
        }

        public string getName() {
            return "Brighty";
        }

        public void getLabels(List<IInputData> inputDataList) {
            if (inputDataList.Count == 2) {
                string commandText = inputDataList[0].getText();
                if (commandText.Equals(COMMAND_TEXT, StringComparison.CurrentCultureIgnoreCase)) {
                    inputDataList[0].setLabel(getID());
                }
            }
        }

        public void getCatalog(List<ICatItem> catalogItems) {
            if (catalogItemFactory != null) {
                catalogItems.Add(catalogItemFactory.createCatItem("SetBrightness.brighty", COMMAND_TEXT, getID(), getIcon()));
            }
        }

        public void getResults(List<IInputData> inputDataList, List<ICatItem> resultsList) {
            if (catalogItemFactory != null && inputDataList[0].hasLabel(getID())) {
                string argumentText = inputDataList[1].getText();

                string fullPath;
                string shortName;
                if (string.IsNullOrEmpty(argumentText)) {
                    //show current brightness when first tabbing into "Brightness", don't allow changing brightness yet
                    fullPath = string.Empty;
                    shortName = monitorService.brightness + "%";
                } else {
                    //specify new brightness by typing a number after tabbing into "Brightness"
                    fullPath = argumentText;
                    shortName = argumentText + "%";
                }

                resultsList.Add(catalogItemFactory.createCatItem(fullPath, shortName, getID(), getIcon()));
            }
        }

        private string getIcon() {
            return Path.Combine(pluginPath, "icons", "Brighty.ico");
        }

        public void launchItem(List<IInputData> inputDataList, ICatItem item) {
            ICatItem catalogItem = inputDataList.Last().getTopResult();
            try {
                uint desiredBrightness = Convert.ToUInt32(catalogItem.getFullPath());
                Task.Run(() => monitorService.brightness = desiredBrightness);
            } catch (FormatException) {
                // ignore non-integer inputs, like if the user just runs "Brightness" instead of "Brightness 50"
            }
        }

        public bool hasDialog() {
            return false;
        }

        public IntPtr doDialog() {
            return IntPtr.Zero;
        }

        public void endDialog(bool acceptedByUser) { }

        public void launchyShow() { }

        public void launchyHide() { }

        public void setPath(string _pluginPath) {
            pluginPath = _pluginPath;
        }

        public void Dispose() {
            monitorService.Dispose();
        }

    }

}