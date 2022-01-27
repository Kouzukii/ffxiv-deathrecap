using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Lumina.Text;
using Lumina.Text.Payloads;

namespace DeathRecap {
    public static class Extensions {
        public static string DisplayedText(this SeString str) => str.Payloads.Aggregate("", (a, p) => p is TextPayload ? a + p.RawString : a);

        public static unsafe byte? Barrier(this PlayerCharacter player) => *(byte*)(player?.Address + 0x19D9);
    }
}