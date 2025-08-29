using System;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace DeathRecap.Events;

public record CombatEvent {
    public required EventSnapshot Snapshot { get; init; }

    public record EventSnapshot {
        public required DateTime Time { get; init; }
        public required uint CurrentHp { get; init; }
        public required uint MaxHp { get; init; }
        public List<StatusEffectSnapshot>? StatusEffects { get; init; }
        public uint BarrierPercent { get; init; }
    }

    public record struct StatusEffectSnapshot {
        public required uint Id;
        public required uint StackCount;
    }

    public record StatusEffect : CombatEvent {
        public required uint Id { get; init; }
        public required uint StackCount { get; init; }
        public required string? Source { get; init; }
        public required uint? Icon { get; init; }
        public required float Duration { get; init; }
        public required string? Status { get; init; }
        public required string? Description { get; init; }
        public required StatusCategory Category { get; init; }
    }

    public record HoT : CombatEvent {
        public required uint Amount { get; init; }
    }

    public record DoT : CombatEvent {
        public required uint Amount { get; init; }
    }

    public record DamageTaken : CombatEvent {
        public required string? Source { get; init; }
        public required uint Amount { get; init; }
        public required string Action { get; init; }
        public bool Crit { get; init; }
        public bool DirectHit { get; init; }
        public required DamageType DamageType { get; init; }
        public required ActionType DisplayType { get; init; }
        public bool Parried { get; init; }
        public bool Blocked { get; init; }
        public required uint? Icon { get; init; }
    }

    public record Healed : CombatEvent {
        public required string? Source { get; init; }
        public required uint Amount { get; init; }
        public required string Action { get; init; }
        public bool Crit { get; init; }
        public required uint? Icon { get; init; }
    }

    public record MedKit : CombatEvent {
        public required uint Amount { get; init; }
    }

    public record FallDamage : CombatEvent {
        public required uint Amount { get; init; }
    }
}
