#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LaunchySharp;

namespace Brighty {

    public class BrightyPlugin: IPlugin, IDisposable {

        private readonly MonitorService monitorService = new MonitorServiceImpl();

        private IPluginHost? pluginHost;
        private ICatItemFactory? catalogItemFactory;

        public void init(IPluginHost pluginHost) {
            if (pluginHost != null) {
                this.pluginHost = pluginHost;
                catalogItemFactory = pluginHost.catItemFactory();
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
                string inputText = inputDataList[0].getText();
                if (inputText.Equals("brightness", StringComparison.CurrentCultureIgnoreCase)) {
                    inputDataList[0].setLabel(getID());
                }
            }
        }

        public void getCatalog(List<ICatItem> catalogItems) {
            if (catalogItemFactory != null) {
                catalogItems.Add(catalogItemFactory.createCatItem("SetBrightness.brighty", "Brightness", getID(), getIcon()));
            }
        }

        public void getResults(List<IInputData> inputDataList, List<ICatItem> resultsList) {
            if (catalogItemFactory != null && inputDataList[0].hasLabel(getID())) {
                string inputText = inputDataList[1].getText();
                if (!string.IsNullOrEmpty(inputText)) {
                    resultsList.Add(catalogItemFactory.createCatItem(inputText, inputText + "%", getID(), getIcon()));
                }
            }
        }

        private string getIcon() {
            return Path.Combine(pluginHost?.launchyPaths().getIconsPath(), "Brighty.ico");
        }

        public void launchItem(List<IInputData> inputDataList, ICatItem item) {
            ICatItem catalogItem = inputDataList[inputDataList.Count - 1].getTopResult();
            try {
                uint desiredBrightness = Convert.ToUInt32(catalogItem.getFullPath());
                Task.Run(() => monitorService.setBrightness(desiredBrightness));
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

        public void setPath(string pluginPath) { }

        public void Dispose() {
            monitorService.Dispose();
        }

    }

}