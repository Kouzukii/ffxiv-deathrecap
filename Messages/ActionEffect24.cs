using System.Runtime.InteropServices;

namespace DeathRecap.Messages;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ActionEffect24 {
    public ActionEffectHeader Header;

    public uint Unknown1;

    public ushort Unknown2;

    public unsafe fixed byte Effects[1536];

    public ushort Unknown3;

    public uint Unknown4;

    public unsafe fixed ulong TargetIds[24];
}
