namespace OpenRGB.NET.Utils;

#if DEBUG
using System;

[AttributeUsage(AttributeTargets.Struct)]
internal class NonCopyableAttribute : Attribute { }

#endif