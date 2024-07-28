using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

internal readonly record struct ModeOperationArg(uint ModeId, ModeArg Mode) : ISpanWritable
{
    public int Length => sizeof(uint) * 2 + Mode.Length;
    public void WriteTo(ref SpanWriter writer)
    {
        writer.Write(Length);
        writer.Write(ModeId);
        Mode.WriteTo(ref writer);
    }
}
