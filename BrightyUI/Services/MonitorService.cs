#nullable enable

using System;

namespace BrightyUI.Services; 

public interface MonitorService: IDisposable {

    uint brightness { get; set; }

}