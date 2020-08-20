using System;
using BenchmarkDotNet.Attributes;
using BrightyUI.Services;
using Xunit.Abstractions;

#nullable enable

namespace Tests {

    public class PerformanceTest: BenchmarkTest {

        private readonly ITestOutputHelper? testOutputHelper;

        public PerformanceTest(ITestOutputHelper? testOutputHelper = null): base(testOutputHelper) {
            this.testOutputHelper = testOutputHelper;
        }

        // private readonly SharpLibMonitorConfigMonitorServiceImpl sharpLibMonitorConfig = new SharpLibMonitorConfigMonitorServiceImpl();
        // private readonly DirectXVideoAccelerationMonitorService dxva = new DirectXVideoAccelerationMonitorService();

        [Benchmark]
        public void sharpLibMonitorConfigGetBrightness() {
            using var sharpLibMonitorConfigMonitorServiceImpl = new SharpLibMonitorConfigMonitorServiceImpl();
            uint actual = sharpLibMonitorConfigMonitorServiceImpl.brightness;
        }

        [Benchmark]
        public void directXVideoAccelerationGetBrightness() {
            using var dxva = new DirectXVideoAccelerationMonitorService();
            uint actual = dxva.brightness;
        }
        
        // public void Dispose() {
        //     sharpLibMonitorConfig.Dispose();
        //     dxva.Dispose();
        // }
    }

}