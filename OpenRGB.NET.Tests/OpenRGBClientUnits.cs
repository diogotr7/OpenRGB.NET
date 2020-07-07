using System;
using Xunit;
using OpenRGB.NET;
using System.Collections.Generic;
using System.Drawing;

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
                var data = devices[i];

                var list = new OpenRGBColor[data.Leds.Length];
                Color clr = Color.Lime;
                for (int j = 0; j < data.Leds.Length; j++)
                {
                    list[j] = new OpenRGBColor(clr.R, clr.G, clr.B);
                    clr = ColorHelper.ChangeHue(clr, (360.0 / 2.0) / data.Leds.Length);
                }
                client.UpdateLeds(i, list);
            }
            client.Disconnect();
        }
    }
}
