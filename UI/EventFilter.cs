using System;

namespace DeathRecap.UI;

[Flags]
public enum EventFilter {
    Damage = 0b1,
    Healing = 0b10,
    Debuffs = 0b100,
    Buffs = 0b1000,
    Default = Damage | Healing | Debuffs | Buffs
}
