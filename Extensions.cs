using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Objects.SubKinds;
using DeathRecap.Events;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using FFXIVClientStructs.FFXIV.Client.System.String;
using Lumina.Text;
using Lumina.Text.Payloads;

namespace DeathRecap;

public static class Extensions {
    public static string DisplayedText(this SeString str) {
        return str.Payloads.Aggregate("", (a, p) => p is TextPayload ? a + p.RawString : a);
    }

    public static unsafe byte Barrier(this IPlayerCharacter player) {
        return ((Character*)player.Address)->CharacterData.ShieldValue;
    }

    public static CombatEvent.EventSnapshot Snapshot(
        this IPlayerCharacter player, bool snapEffects = false,
        IReadOnlyCollection<uint>? additionalStatus = null) {
        var statusEffects = snapEffects
            ? player.StatusList.Select(s => new CombatEvent.StatusEffectSnapshot { Id = s.StatusId, StackCount = s.Param })
                .ToList()
            : null;
        if (additionalStatus != null)
            statusEffects?.AddRange(additionalStatus.Select(s => new CombatEvent.StatusEffectSnapshot { Id = s, StackCount = 0 }));
        var snapshot = new CombatEvent.EventSnapshot {
            Time = DateTime.Now,
            CurrentHp = player.CurrentHp,
            MaxHp = player.MaxHp,
            StatusEffects = statusEffects,
            BarrierPercent = player.Barrier()
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

    public static string Demangle(this string name) {
        if (!name.StartsWith("_rsv_"))
            return name;

        unsafe {
            var demangled = LayoutWorld.Instance()->RsvMap[0][new Utf8String(name)];
            if (demangled.Value != null && Marshal.PtrToStringUTF8((IntPtr)demangled.Value) is { } result) {
                return result;
            }
        }

        Service.PluginLog.Warning($"Unknown name {name}");
        return name;
    }
}
