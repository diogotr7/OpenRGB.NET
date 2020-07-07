using System;
using Xunit;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
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
        public void Dispose(){}

        [Fact]
        public void ClientConnectToServer()
        {
            Stopwatch sw = Stopwatch.StartNew();
            OpenRGBClient client = new OpenRGBClient(name: "OpenRGB.NET Test: ClientConnectToServer");
            client.Connect();
            client.Disconnect();
            sw.Stop();
            Output.WriteLine($"Time elapsed: {(double)sw.ElapsedTicks / Stopwatch.Frequency * 1000} ms.");
        }

        [Fact]
        public void ListController()
        {
            Stopwatch sw = Stopwatch.StartNew();
            OpenRGBClient client = new OpenRGBClient(name: "OpenRGB.NET Test: ListController");
            client.Connect();
            int nbController = client.GetControllerCount();
            for (int i = 0; i < nbController; i++)
            {
                OpenRGBDevice controller = client.GetControllerData(i);
                Assert.True(!string.IsNullOrWhiteSpace(controller.Name));
            }
            client.Disconnect();
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
            OpenRGBClient client = new OpenRGBClient(name: "OpenRGB.NET Test: CheckLedChange");
            client.Connect();
            var controllerCount = client.GetControllerCount();
            var devices = new List<OpenRGBDevice>();

            for (int i = 0; i < controllerCount; i++)
                devices.Add(client.GetControllerData(i));

            for (int i = 0; i < devices.Count; i++)
            {
                var device = devices[i];

                var originalColors = new OpenRGBColor[device.Leds.Length];

                var clr = new OpenRGBColor(0, 0, 255);
                var hueIncrement = 360.0 / device.Leds.Length;

                for (int j = 0; j < device.Leds.Length; j++)
                {
                    originalColors[j] = new OpenRGBColor(clr);
                    clr = clr.ChangeHue(hueIncrement);
                }

                client.UpdateLeds(i, originalColors);
                var updatedColors = client.GetControllerData(i).Colors;

                Assert.True(updatedColors.SequenceEqual(originalColors));
            }

            client.Disconnect();
            sw.Stop();
            Output.WriteLine($"Time elapsed: {(double)sw.ElapsedTicks / Stopwatch.Frequency * 1000} ms.");
        }

        private void DoDryRun(int nbDryRun)
        {
            for (int i = 0; i < nbDryRun; i++)
            {
                using OpenRGBClient client = new OpenRGBClient(name: "OpenRGB.NET Test: DryRun", autoconnect: true);
                int nbController = client.GetControllerCount();
                for (int j = 0; i < nbController; i++)
                {
                    OpenRGBDevice controller = client.GetControllerData(j);
                    Assert.True(!string.IsNullOrWhiteSpace(controller.Name));
                }
            }
        }
    }
}
