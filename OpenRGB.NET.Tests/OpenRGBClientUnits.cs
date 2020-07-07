using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace OpenRGB.NET.Test
{
    public class OpenRGBClientUnits : IDisposable
    {
        private readonly ITestOutputHelper Output;

        /// <summary>
        /// Will be exectuted before every test
        /// </summary>
        /// <param name="output"></param>
        public OpenRGBClientUnits(ITestOutputHelper output)
        {
            DoDryRun(5); // Do some dry runs first for more accurate stopwatch mesurement.
            Output = output;
        }

        /// <summary>
        /// Will be executed after every test
        /// </summary>
        public void Dispose() { }

        [Fact]
        public void ClientConnectToServer()
        {
            Stopwatch sw = Stopwatch.StartNew();
            OpenRGBClient client = new OpenRGBClient(name: "OpenRGB.NET Test: ClientConnectToServer");
            client.Connect();
            client.Dispose();
            sw.Stop();
            Output.WriteLine($"Time elapsed: {(double)sw.ElapsedTicks / Stopwatch.Frequency * 1000} ms.");
        }

        [Fact]
        public void ListController()
        {
            Stopwatch sw = Stopwatch.StartNew();
            OpenRGBClient client = new OpenRGBClient(name: "OpenRGB.NET Test: ListController");

            int nbController = client.GetControllerCount();
            for (int i = 0; i < nbController; i++)
            {
                OpenRGBDevice controller = client.GetControllerData(i);
                Assert.True(!string.IsNullOrWhiteSpace(controller.Name));
            }

            client.Dispose();
            sw.Stop();
            Output.WriteLine($"Time elapsed: {(double)sw.ElapsedTicks / Stopwatch.Frequency * 1000} ms.");
        }


        [Fact]
        public void DisposePatternListController()
        {
            Stopwatch sw = Stopwatch.StartNew();
            using OpenRGBClient client = new OpenRGBClient(name: "OpenRGB.NET Test: DisposePatternListController", autoconnect: true);
            int nbController = client.GetControllerCount();
            for (int i = 0; i < nbController; i++)
            {
                OpenRGBDevice controller = client.GetControllerData(i);
                Assert.True(!string.IsNullOrWhiteSpace(controller.Name));
            }
            sw.Stop();
            Output.WriteLine($"Time elapsed: {(double)sw.ElapsedTicks / Stopwatch.Frequency * 1000} ms.");
        }

        [Fact]
        public void CheckLedChange()
        {
            Stopwatch sw = Stopwatch.StartNew();
            var client = new OpenRGBClient(name: "OpenRGB.NET Test: CheckLedChange");
            var controllerCount = client.GetControllerCount();
            var devices = new OpenRGBDevice[controllerCount];

            for (int i = 0; i < controllerCount; i++)
                devices[i] = client.GetControllerData(i);

            for (int i = 0; i < controllerCount; i++)
            {
                var device = devices[i];

                var originalColors = ColorHelper.GetRainbow(new OpenRGBColor(255, 0, 0), device.Leds.Length);

                client.UpdateLeds(i, originalColors);
                var updatedColors = client.GetControllerData(i).Colors;

                Assert.True(updatedColors.SequenceEqual(originalColors));
            }
            client.Dispose();
            sw.Stop();
            Output.WriteLine($"Time elapsed: {(double)sw.ElapsedTicks / Stopwatch.Frequency * 1000} ms.");
        }

        [Fact]
        public void UpdateZoneTypes()
        {
            //This delay is needed due to a threading bug in OpenRGB
            //https://gitlab.com/CalcProgrammer1/OpenRGB/-/issues/376
            //https://gitlab.com/CalcProgrammer1/OpenRGB/-/issues/350
            Thread.Sleep(150);
            Stopwatch sw = Stopwatch.StartNew();
            var client = new OpenRGBClient(name: "OpenRGB.NET Test: UpdateZoneTypes");
            var controllerCount = client.GetControllerCount();
            var devices = new OpenRGBDevice[controllerCount];

            for (int i = 0; i < controllerCount; i++)
                devices[i] = client.GetControllerData(i);

            for (int i = 0; i < controllerCount; i++)
            {
                var device = devices[i];

                for (int j = 0; j < device.Zones.Length; j++)
                {
                    var zone = device.Zones[j];
                    switch (zone.Type)
                    {
                        case Enums.OpenRGBZoneType.Linear:
                            var colors = ColorHelper.GetRainbow(new OpenRGBColor(255, 0, 0), zone.LedCount);
                            client.UpdateZone(i, j, colors);
                            break;
                        case Enums.OpenRGBZoneType.Single:
                            client.UpdateZone(i, j, new[] { new OpenRGBColor(255, 0, 0) });
                            break;
                        case Enums.OpenRGBZoneType.Matrix:
                            var rainbow = ColorHelper.GetRainbow(new OpenRGBColor(0, 255, 0), zone.MatrixMap.Width);
                            var matrix = new OpenRGBColor[zone.LedCount];
                            for (int k = 0; k < zone.MatrixMap.Width; k++)
                            {
                                for (int l = 0; l < zone.MatrixMap.Height; l++)
                                {
                                    var index = zone.MatrixMap.Matrix[l, k];
                                    if (index != uint.MaxValue)
                                    {
                                        matrix[index] = new OpenRGBColor(rainbow[k]);
                                    }
                                }
                            }
                            client.UpdateZone(i, j, matrix);
                            break;
                    }
                }
            }
            client.Dispose();
            sw.Stop();
            Output.WriteLine($"Time elapsed: {(double)sw.ElapsedTicks / Stopwatch.Frequency * 1000} ms.");
        }

        [Fact]
        public void UseAfterDispose()
        {
            Stopwatch sw = Stopwatch.StartNew();
            var client = new OpenRGBClient();
            client.Dispose();

            Assert.Throws<ObjectDisposedException>(() => client.GetControllerCount());
            Output.WriteLine($"Time elapsed: {(double)sw.ElapsedTicks / Stopwatch.Frequency * 1000} ms.");
        }

        private void DoDryRun(int nbDryRun)
        {
            for (int i = 0; i < nbDryRun; i++)
            {
                using OpenRGBClient client = new OpenRGBClient(name: "OpenRGB.NET Test: DryRun");
                int nbController = client.GetControllerCount();
                for (int j = 0; j < nbController; j++)
                {
                    OpenRGBDevice controller = client.GetControllerData(j);
                    Assert.True(!string.IsNullOrWhiteSpace(controller.Name));
                }
            }
        }
    }
}
