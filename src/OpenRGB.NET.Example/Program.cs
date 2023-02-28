// See https://aka.ms/new-console-template for more information

using System.Buffers;
using OpenRGB.NET;
using OpenRGB.NET.Utils;

//Moves each element 1 forward, wrapping the last element to the first position
void MoveOne<T>(Span<T> data)
{
    var last = data[^1];
    for (var i = data.Length - 1; i > 0; i--)
        data[i] = data[i - 1];
    data[0] = last;
}

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
var updateTask = Task.Run(() =>
{
    var deviceColors = new Color[devices.Length][];
    for (var index = 0; index < devices.Length; index++)
        deviceColors[index] = ColorUtils.GetSinRainbow(devices[index].Leds.Length).ToArray();

    while (run)
    {
        for (var index = 0; index < devices.Length; index++)
        {
            var colors = deviceColors[index];
            MoveOne<Color>(colors);
            client.UpdateLeds(index, colors);
        }

        Thread.Sleep(10);
    }
});

Console.WriteLine("Press any key to exit");
Console.ReadKey();
run = false;
updateTask.Wait();