using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Lumina.Text;
using Lumina.Text.Payloads;

namespace DeathRecap {
    public static class Extensions {
        public static string DisplayedText(this SeString str) {
            return str.Payloads.Aggregate("", (a, p) => p is TextPayload ? a + p.RawString : a);
        }

        public static unsafe byte? Barrier(this PlayerCharacter player) {
            return *(byte*)(player?.Address + 0x19D9);
        }

        public static CombatEvent.EventSnapshot Snapshot(this PlayerCharacter player, bool snapEffects = false,
            IReadOnlyCollection<uint>? additionalStatus = null) {
            var statusEffects = snapEffects ? player?.StatusList?.Select(s => s.StatusId).ToList() : null;
            if (additionalStatus != null)
                statusEffects?.AddRange(additionalStatus);
            var snapshot = new CombatEvent.EventSnapshot {
                Time = DateTime.Now,
                CurrentHp = player?.CurrentHp,
                MaxHp = player?.MaxHp,
                StatusEffects = statusEffects,
                BarrierPercent = player?.Barrier()
            };
            return snapshot;
        }

        public static void AddEntry<TKey, TValue>(this Dictionary<TKey, List<TValue>> dict, TKey key, TValue val) where TKey : notnull {
            if (dict.TryGetValue(key, out var list)) {
                list.Add(val);
            } else {
                var objList = new List<TValue>();
                dict.Add(key, objList);
                objList.Add(val);
            }
        }
    }
}
