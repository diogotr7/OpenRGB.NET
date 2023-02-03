// See https://aka.ms/new-console-template for more information

using OpenRGB.NET;
using OpenRGB.NET.Utils;

using var client = new OpenRgbClient();
client.Connect();
Console.WriteLine("Connected to OpenRGB");

var devices = client.GetAllControllerData();

Console.WriteLine("Found devices:");
foreach (var device in devices)
    Console.WriteLine(device.Name);

//set everything to red
foreach(var device in devices)
{
    var colors = new Color[device.Colors.Length];
    for (var i = 0; i < colors.Length; i++)
        colors[i] = new Color(255,0,0);

    client.UpdateLeds(device.Index, colors);
}
Console.WriteLine("Press any key to continue");

Console.ReadKey();

//animate hue
Console.WriteLine("Starting animation");
var run = true;
Task.Run(() =>
{
    var hue = 0;

    while (run)
    {
        foreach (var device in devices)
        {
            var colors = new Color[device.Colors.Length];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = ColorUtils.FromHsv(hue, 1, 1);

            client.UpdateLeds(device.Index, colors);
        }

        hue += 1;
        if (hue > 360)
            hue = 0;

        Thread.Sleep(10);
    }
});

Console.WriteLine("Press any key to exit");
Console.ReadKey();
run = false;
