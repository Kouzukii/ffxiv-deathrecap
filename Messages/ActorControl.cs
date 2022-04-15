using System.Runtime.InteropServices;

namespace DeathRecap.Messages {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ActorControl142 {
        public ActorControlCategory Category;

        public ushort Unknown1;

        public uint Param1;

        public uint Param2;

        public uint Param3;

        public uint Param4;
    }
}
