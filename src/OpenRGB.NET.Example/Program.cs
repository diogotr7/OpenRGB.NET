// See https://aka.ms/new-console-template for more information

using System.Buffers;
using OpenRGB.NET;
using OpenRGB.NET.Utils;

using var client = new OpenRgbClient();
client.Connect();
Console.WriteLine("Connected to OpenRGB");

var plugins = client.GetPlugins();

var devices = client.GetAllControllerData();

var profiles = client.GetProfiles();

Console.WriteLine("Found devices:");
foreach (var device in devices)
    Console.WriteLine(device.Name);

Console.WriteLine("Starting animation");
var cts = new CancellationTokenSource();

const int fps = 60;

var updateTask = Task.Run(() =>
{
    var deviceColors = new Color[devices.Length][];
    for (var index = 0; index < devices.Length; index++)
    {
        var arr = ColorUtils.GetHueRainbow(devices[index].Leds.Length).ToArray();
        deviceColors[index] = arr.Concat(arr).ToArray();
    }
    var colorOffsets = Enumerable.Range(0, devices.Length).Select(x => x).ToArray();
    while (!cts.IsCancellationRequested)
    {
        for (var index = 0; index < devices.Length; index++)
        {
            var colors = deviceColors[index];
            if (colors.Length == 0)
                continue;
            
            var slice = colors.AsSpan().Slice(colorOffsets[index]++ % devices[index].Leds.Length , devices[index].Leds.Length);
            client.UpdateLeds(index, slice);
        }

        Thread.Sleep(1000 / fps);
    }
});

Console.WriteLine("Press any key to exit");
Console.ReadKey();
cts.Cancel();
updateTask.Wait();
Console.WriteLine("Exited.");