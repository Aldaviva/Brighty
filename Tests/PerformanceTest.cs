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

        // [Benchmark]
        // public void sharpLibMonitorConfigGetBrightness() {
        //     using var sharpLibMonitorConfigMonitorServiceImpl = new SharpLibMonitorConfigMonitorServiceImpl();
        //     uint actual = sharpLibMonitorConfigMonitorServiceImpl.brightness;
        // }

        [Benchmark]
        public void directXVideoAccelerationGetBrightness() {
            using DxvaMonitorService dxva = new();
            uint                     _    = dxva.brightness;
        }

    }

}