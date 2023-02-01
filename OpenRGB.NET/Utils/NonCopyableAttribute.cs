using System;

namespace OpenRGB.NET.Utils;

#if DEBUG

[AttributeUsage(AttributeTargets.Struct)]
internal class NonCopyableAttribute : Attribute { }

#endif