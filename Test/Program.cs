using OpenRGB.NET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            OpenRGBClient openRgb = new OpenRGBClient(port: 1338, name: "OpenRGB.NET Test Application");
            openRgb.Connect();
            var controllerCount = openRgb.GetControllerCount();
            var devices = new List<OpenRGBDevice>();

            for (int i = 0; i < controllerCount; i++)
                devices.Add(openRgb.GetControllerData(i));

            for(int i = 0; i < devices.Count; i++)
            {
                var data = devices[i];

                var list = new OpenRGBColor[data.leds.Length];
                Color clr = Color.Lime;
                for (int j = 0; j < data.leds.Length; j++)
                {
                    list[j] = new OpenRGBColor(clr.R, clr.G, clr.B);
                    clr = ColorHelper.ChangeHue(clr, (360.0/3.0) / data.leds.Length);
                }
                openRgb.UpdateLeds(i, list);
            }

            Thread.Sleep(1000);
        }
    }
}
