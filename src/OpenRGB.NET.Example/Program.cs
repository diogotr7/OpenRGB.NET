// See https://aka.ms/new-console-template for more information

using System.Buffers;
using OpenRGB.NET;
using OpenRGB.NET.Utils;

using var client = new OpenRgbClient();
client.Connect();
Console.WriteLine("Connected to OpenRGB");

var devices = client.GetAllControllerData();

Console.WriteLine("Found devices:");
foreach (var device in devices)
    Console.WriteLine(device.Name);

Console.WriteLine("Starting animation");
var cts = new CancellationTokenSource();
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

        Thread.Sleep(33);
    }
});

Console.WriteLine("Press any key to exit");
Console.ReadKey();
cts.Cancel();
updateTask.Wait();
Console.WriteLine("Exited.");