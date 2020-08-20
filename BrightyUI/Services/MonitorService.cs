#nullable enable

using System;
using KoKo.Property;

namespace BrightyUI.Services {

    public interface MonitorService: IDisposable {

        uint brightness { get; set; }

        Property<bool> isInitialized { get; }

    }

}