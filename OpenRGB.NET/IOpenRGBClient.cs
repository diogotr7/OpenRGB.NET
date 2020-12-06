﻿using OpenRGB.NET.Enums;
using OpenRGB.NET.Models;

namespace OpenRGB.NET
{
    /// <summary>
    /// Interface for an OpenRGB SDK client.
    /// </summary>
    public interface IOpenRGBClient
    {
        /// <summary>
        /// Represents the connection status of the socket to the server
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Connects manually to the server. Only needs to be called if the constructor was called
        /// with autoconnect set to false.
        /// </summary>
        void Connect();

        /// <summary>
        /// Requests the data for all the controllers detected by the server.
        /// </summary>
        /// <returns>An array with the information for all the devices.</returns>
        Device[] GetAllControllerData();

        /// <summary>
        /// Requests the controller count from the server.
        /// </summary>
        /// <returns>The amount of controllers.</returns>
        int GetControllerCount();

        /// <summary>
        /// Requests the data block for a given controller index.
        /// </summary>
        /// <param name="id">The index of the controller to request the data from.</param>
        /// <returns>The Device containing the decoded data for the controller with the given id.</returns>
        Device GetControllerData(int id);

        /// <summary>
        /// Sets the mode of the specified device to "Custom".
        /// </summary>
        /// <param name="deviceId"></param>
        void SetCustomMode(int deviceId);

        /// <summary>
        /// Sets the specified mode on the specified device.
        /// Any optional parameters not set will be left as received from the server.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="modeId"></param>
        /// <param name="speed"></param>
        /// <param name="direction"></param>
        /// <param name="colors"></param>
        void SetMode(int deviceId, int modeId, uint? speed = null, Direction? direction = null, Color[] colors = null);

        /// <summary>
        /// Updates the LEDs for the give device.
        /// Make sure the array has the correct number of LEDs.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="colors"></param>
        void UpdateLeds(int deviceId, Color[] colors);

        /// <summary>
        /// Updates the LEDs of a given device and zone.
        /// Make sure the array has the correct number of LEDs for the zone.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="zoneId"></param>
        /// <param name="colors"></param>
        void UpdateZone(int deviceId, int zoneId, Color[] colors);
    }
}