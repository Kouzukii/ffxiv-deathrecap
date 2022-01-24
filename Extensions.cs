using System.Linq;
using Lumina.Text;
using Lumina.Text.Payloads;

namespace DeathRecap {
    public static class Extensions {
        public static string DisplayedText(this SeString str) {
            return str.Payloads.Aggregate("", (a, p) => p is TextPayload ? a + p.RawString : a);
        }
    }
}