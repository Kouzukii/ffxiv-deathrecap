using System.Runtime.InteropServices;

namespace DeathRecap.Game;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct StatusEffectAddEntry {
    public byte EffectIndex;

    public byte Unknown1;

    public ushort EffectId;

    public ushort StackCount;

    public ushort Unknown3;

    public float Duration;

    public uint SourceActorId;
}
