using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

internal readonly struct None : ISpanWritable
{
    public int Length => 0;
    public void WriteTo(ref SpanWriter writer) { }
}

internal readonly struct OpenRgbString(string value) : ISpanWritable
{
    public readonly string Value = value;
    public int Length => Value.Length + 1;
    public void WriteTo(ref SpanWriter writer) => writer.Write(Value);
}

internal readonly struct ProtocolVersionWriter(ProtocolVersion version) : ISpanWritable
{
    public readonly ProtocolVersion Version = version;
    public int Length => sizeof(uint);
    public void WriteTo(ref SpanWriter writer) => writer.Write(Version.Number);
}

internal readonly struct ModeWriter(Mode mode) : ISpanWritable
{
    public readonly Mode Mode = mode;
    
    public int Length
    {
        get
        {
            var size = (
                sizeof(int) * 2 +
                sizeof(uint) * 9 +
                sizeof(ushort) * 2 +
                sizeof(uint) * Mode.Colors.Length +
                Mode.Name.Length + 1);

            if (Mode.ProtocolVersion.SupportsBrightnessAndSaveMode) size += sizeof(uint) * 3;

            return size;
        }
    }
    
    public void WriteTo(ref SpanWriter writer)
    {
        writer.WriteLengthAndString(Mode.Name);
        writer.Write(Mode.Value);
        writer.Write((uint)Mode.Flags);
        writer.Write(Mode.SpeedMin);
        writer.Write(Mode.SpeedMax);

        if (Mode.ProtocolVersion.SupportsBrightnessAndSaveMode)
        {
            writer.Write(Mode.BrightnessMin);
            writer.Write(Mode.BrightnessMax);
        }

        writer.Write(Mode.ColorMin);
        writer.Write(Mode.ColorMax);
        writer.Write(Mode.Speed);

        if (Mode.ProtocolVersion.SupportsBrightnessAndSaveMode)
            writer.Write(Mode.Brightness);

        writer.Write((uint)Mode.Direction);
        writer.Write((uint)Mode.ColorMode);
        writer.Write((ushort)Mode.Colors.Length);

        foreach (var color in Mode.Colors)
            color.WriteTo(ref writer);
    }
}

internal readonly struct ModeOperation(uint dataSize, uint modeId, ModeWriter mode) : ISpanWritable
{
    public readonly uint DataSize = dataSize;
    public readonly uint ModeId = modeId;
    public readonly ModeWriter Mode = mode;
    public int Length => sizeof(uint) * 2 + Mode.Length;
    public void WriteTo(ref SpanWriter writer)
    {
        writer.Write(DataSize);
        writer.Write(ModeId);
        Mode.WriteTo(ref writer);
    }
}

internal readonly unsafe struct Args<T1>(T1 arg1) : ISpanWritable 
    where T1 : unmanaged
{
    public readonly T1 Arg1 = arg1;
    public int Length => sizeof(T1);
    public void WriteTo(ref SpanWriter writer)
    {
        writer.Write(Arg1);
    }
}

internal readonly unsafe struct Args<T1, T2>(T1 arg1, T2 arg2) : ISpanWritable 
    where T1 : unmanaged
    where T2 : unmanaged
{
    public readonly T1 Arg1 = arg1;
    public readonly T2 Arg2 = arg2;
    public int Length =>  sizeof(T1) + sizeof(T2);
    public void WriteTo(ref SpanWriter writer)
    {
        writer.Write(Arg1);
        writer.Write(Arg2);
    }
}

internal readonly unsafe struct Args<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3) : ISpanWritable 
    where T1 : unmanaged
    where T2 : unmanaged
    where T3 : unmanaged
{
    public readonly T1 Arg1 = arg1;
    public readonly T2 Arg2 = arg2;
    public readonly T3 Arg3 = arg3;
    public int Length => sizeof(T1) + sizeof(T2) + sizeof(T3);
    public void WriteTo(ref SpanWriter writer)
    {
        writer.Write(Arg1);
        writer.Write(Arg2);
        writer.Write(Arg3);
    }
}