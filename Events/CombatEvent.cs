using System;
using System.Collections.Generic;
using DeathRecap.Game;

namespace DeathRecap.Events;

public record CombatEvent {
    public EventSnapshot Snapshot { get; init; } = null!;

    public record EventSnapshot {
        public DateTime Time { get; init; }
        public uint CurrentHp { get; init; }
        public uint MaxHp { get; init; }
        public List<uint>? StatusEffects { get; init; }
        public uint BarrierPercent { get; init; }
    }

    public record StatusEffect : CombatEvent {
        public uint Id { get; init; }
        public string? Source { get; init; }
        public ushort? Icon { get; init; }
        public float Duration { get; init; }
        public string? Status { get; init; }
        public string? Description { get; init; }
        public StatusCategory Category { get; init; }
    }

    public record HoT : CombatEvent {
        public uint Amount { get; init; }
    }

    public record DoT : CombatEvent {
        public uint Amount { get; init; }
    }

    public record DamageTaken : CombatEvent {
        public string? Source { get; init; }
        public uint Amount { get; init; }
        public string Action { get; init; } = null!;
        public bool Crit { get; init; }
        public bool DirectHit { get; init; }
        public DamageType DamageType { get; init; }
        public ActionEffectDisplayType DisplayType { get; init; }
        public bool Parried { get; init; }
        public bool Blocked { get; init; }
        public ushort? Icon { get; init; }
    }

    public record Healed : CombatEvent {
        public string? Source { get; init; }
        public uint Amount { get; init; }
        public string Action { get; init; } = null!;
        public bool Crit { get; init; }
        public ushort? Icon { get; init; }
    }
}
