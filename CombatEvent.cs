using System;
using System.Collections.Generic;

namespace DeathRecap {
    internal record CombatEvent {
        public EventSnapshot Snapshot { get; init; }

        internal record EventSnapshot {
            public DateTime Time { get; init; }
            public uint? CurrentHp { get; init; }
            public uint? MaxHp { get; init; }
            public List<uint>? StatusEffects { get; init; }
            public uint? Barrier { get; set; }
        }

        internal record StatusEffect : CombatEvent {
            public uint Id { get; init; }
            public string? Source { get; init; }
            public ushort? Icon { get; init; }
            public float Duration { get; init; }
            public string? Status { get; init; }
        }

        internal record HoT : CombatEvent {
            public uint Id { get; init; }
            public uint Amount { get; init; }
        }

        internal record DoT : CombatEvent {
            public uint Id { get; init; }
            public uint Amount { get; init; }
        }

        internal record Death : CombatEvent {
        }

        internal record DamageTaken : CombatEvent {
            public string? Source { get; init; }
            public int Amount { get; init; }
            public string? Action { get; init; }
            public bool Crit { get; init; }
            public bool DirectHit { get; init; }
            public DamageType DamageType { get; init; }
            public bool Parried { get; init; }
            public bool Blocked { get; init; }
            public ushort? Icon { get; init; }
        }

        public record Healed : CombatEvent {
            public string? Source { get; init; }
            public int Amount { get; init; }
            public string? Action { get; init; }
            public bool Crit { get; init; }
            public bool DirectHit { get; init; }
            public ushort? Icon { get; init; }
        }
    }
}