using System.Runtime.InteropServices;

namespace DeathRecap.Messages {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AddStatusEffect {
        public uint RelatedActionSequence;

        public uint ActorId;

        public uint CurrentHp;

        public uint MaxHp;

        public ushort CurrentMp;

        public ushort Unknown1;

        public byte DamageShield;

        public byte EffectCount;

        public ushort Unknown2;

        public unsafe fixed byte Effects[64];
    }
}