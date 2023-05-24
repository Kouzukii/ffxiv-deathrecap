using System.Runtime.InteropServices;

namespace DeathRecap.Game;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct ActionEffect {
    public ActionEffectType EffectType;
    public byte Param0;
    public byte Param1;
    public byte Param2;
    public byte Flags1;
    public byte Flags2;
    public ushort Value;
}
