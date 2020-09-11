# OpenRGB.NET
[![Nuget](https://img.shields.io/nuget/v/OpenRGB.NET)](https://www.nuget.org/packages/OpenRGB.NET)

OpenRGB.NET is a C# client for the OpenRGB SDK. It is meant to be a simple client, providing a method for each of the functionalities available in the SDK.
Tested on Windows 10 and Arch Linux.

# Installation
You can simply add the nuget package as a reference to your project.

# Usage example
## Setting every led to red
```cs
using var client = new OpenRGBClient(name: "My OpenRGB Client", autoconnect: true, timeout: 1000);

var deviceCount = client.GetControllerCount();
var devices = client.GetAllControllerData();

for (int i = 0; i < devices.Length; i++)
{
    var leds = Enumerable.Range(0, devices[i].Colors.Length)
        .Select(_ => new Color(255, 0, 0))
        .ToArray();
    client.UpdateLeds(i, leds);
}
```
