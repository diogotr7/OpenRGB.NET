using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace OpenRGB.NET.Test
{
    public class OpenRGBClientUnits
    {
        [Fact]
        public void ClientConnectToServer()
        {
            OpenRGBClient client = new OpenRGBClient(name: "OpenRGB.NET Test: ClientConnectToServer");
            client.Connect();
            client.Disconnect();
        }

        [Fact]
        public void ListController()
        {
            OpenRGBClient client = new OpenRGBClient(name: "OpenRGB.NET Test: ListController");
            client.Connect();
            int nbController = client.GetControllerCount();
            for (int i = 0; i < nbController; i++)
            {
                OpenRGBDevice controller = client.GetControllerData(i);
                Assert.True(!string.IsNullOrWhiteSpace(controller.Name));
            }
            client.Disconnect();
        }

        [Fact]
        public void CheckLedChange()
        {
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
        }
    }
}
