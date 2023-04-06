#if DEBUG
using System;
using System.Collections.Generic;
using DeathRecap.Events;
using DeathRecap.Game;

namespace DeathRecap;

internal static class DummyData {
    internal static void AddDummyData(DeathRecapPlugin plugin) {
        plugin.DeathsPerPlayer.AddEntry(0u, new Death {
            PlayerId = 0,
            PlayerName = "Testing",
            TimeOfDeath = DateTime.Parse("2022-09-30T18:10:37.0213944+02:00"),
            Events = new List<CombatEvent> {
                new CombatEvent.Healed {
                    Source = "Astrologian",
                    Amount = 18004,
                    Action = "Essential Dignity",
                    Icon = 3141,
                    Snapshot = new CombatEvent.EventSnapshot {
                        Time = DateTime.Parse("2022-09-30T18:09:31.4717205+02:00"),
                        CurrentHp = 31655,
                        MaxHp = 64167,
                        StatusEffects = new List<uint> {
                            360,
                            362,
                            2604,
                            48,
                            50,
                            2911
                        }
                    }
                },
                new CombatEvent.StatusEffect {
                    Category = StatusCategory.Beneficial,
                    Id = 2618,
                    Source = "Sage",
                    Icon = 12964,
                    Duration = 15.0f,
                    Status = "Kerachole",
                    Description = "Damage taken is reduced.",
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:09:33.9858002+02:00"), CurrentHp = 50300, MaxHp = 64167 }
                },
                new CombatEvent.StatusEffect {
                    Category = StatusCategory.Beneficial,
                    Id = 2938,
                    Source = "Sage",
                    Icon = 12970,
                    Duration = 15.0f,
                    Status = "Kerakeia",
                    Description = "Regenerating HP over time.",
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:09:33.985809+02:00"), CurrentHp = 50300, MaxHp = 64167 }
                },
                new CombatEvent.HoT {
                    Amount = 2502,
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:09:34.9231187+02:00"), CurrentHp = 50300, MaxHp = 64167 }
                },
                new CombatEvent.HoT {
                    Amount = 2650,
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:09:37.9438292+02:00"), CurrentHp = 53443, MaxHp = 64167 }
                },
                new CombatEvent.HoT {
                    Amount = 2525,
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:09:40.9158899+02:00"), CurrentHp = 56734, MaxHp = 64167 }
                },
                new CombatEvent.StatusEffect {
                    Category = StatusCategory.Beneficial,
                    Id = 1204,
                    Source = "Sage",
                    Icon = 13909,
                    Duration = 21.0f,
                    Status = "Lucid Dreaming",
                    Description = "Restoring MP over time.",
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:09:41.3950098+02:00"), CurrentHp = 59259, MaxHp = 64167 }
                },
                new CombatEvent.HoT {
                    Amount = 2564,
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:09:43.9296323+02:00"), CurrentHp = 59900, MaxHp = 64167 }
                },
                new CombatEvent.HoT {
                    Amount = 2478,
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:09:46.9368415+02:00"), CurrentHp = 63105, MaxHp = 64167 }
                },
                new CombatEvent.StatusEffect {
                    Category = StatusCategory.Beneficial,
                    Id = 2599,
                    Source = "Reaper",
                    Icon = 12936,
                    Duration = 20.0f,
                    Status = "Arcane Circle",
                    Description = "Damage dealt is increased.",
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:09:51.2637863+02:00"), CurrentHp = 64167, MaxHp = 64167 }
                },
                new CombatEvent.StatusEffect {
                    Category = StatusCategory.Beneficial,
                    Id = 2600,
                    Source = "Reaper",
                    Icon = 12937,
                    Duration = 5.0f,
                    Status = "Circle of Sacrifice",
                    Description = "Grants Immortal Sacrifice to the reaper who applied this effect after successfully landing a weaponskill or spell.",
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:09:51.2637952+02:00"), CurrentHp = 64167, MaxHp = 64167 }
                },
                new CombatEvent.StatusEffect {
                    Category = StatusCategory.Beneficial,
                    Id = 1890,
                    Source = "Astrologian",
                    Icon = 13251,
                    Duration = 10.0f,
                    Status = "Horoscope",
                    Description = "Primed to receive the healing effects of Horoscope.",
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:09:51.8537222+02:00"), CurrentHp = 64167, MaxHp = 64167 }
                },
                new CombatEvent.StatusEffect {
                    Category = StatusCategory.Beneficial,
                    Id = 1951,
                    Source = "Machinist",
                    Icon = 13021,
                    Duration = 15.0f,
                    Status = "Tactician",
                    Description = "Damage taken is reduced.",
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:09:52.305156+02:00"), CurrentHp = 64167, MaxHp = 64167 }
                },
                new CombatEvent.Healed {
                    Source = "Sage",
                    Amount = 3884,
                    Action = "Eukrasian Prognosis",
                    Icon = 3660,
                    Snapshot = new CombatEvent.EventSnapshot {
                        Time = DateTime.Parse("2022-09-30T18:09:52.5276339+02:00"),
                        CurrentHp = 64167,
                        MaxHp = 64167,
                        StatusEffects = new List<uint> {
                            360,
                            362,
                            2604,
                            48,
                            1890,
                            2911,
                            2599,
                            1204,
                            2600,
                            2606,
                            1951
                        }
                    }
                },
                new CombatEvent.DamageTaken {
                    Source = "Proto-Carbuncle",
                    Amount = 21445,
                    Action = "Sonic Howl",
                    DamageType = (DamageType)5,
                    DisplayType = (ActionEffectDisplayType)1,
                    Icon = 405,
                    Snapshot = new CombatEvent.EventSnapshot {
                        Time = DateTime.Parse("2022-09-30T18:09:53.1037732+02:00"),
                        CurrentHp = 64167,
                        MaxHp = 64167,
                        StatusEffects = new List<uint> {
                            360,
                            362,
                            2604,
                            48,
                            1890,
                            2599,
                            1204,
                            2600,
                            1951,
                            2609,
                            1195,
                            1193
                        },
                        BarrierPercent = 19
                    }
                },
                new CombatEvent.StatusEffect {
                    Category = StatusCategory.Beneficial,
                    Id = 2609,
                    Source = "Sage",
                    Icon = 12954,
                    Duration = 29.110992f,
                    Status = "Eukrasian Prognosis",
                    Description = "A magicked barrier is nullifying damage.",
                    Snapshot = new CombatEvent.EventSnapshot {
                        Time = DateTime.Parse("2022-09-30T18:09:53.5204295+02:00"), CurrentHp = 64167, MaxHp = 64167, BarrierPercent = 19
                    }
                },
                new CombatEvent.Healed {
                    Source = "Astrologian",
                    Amount = 5144,
                    Action = "Horoscope",
                    Icon = 3551,
                    Snapshot = new CombatEvent.EventSnapshot {
                        Time = DateTime.Parse("2022-09-30T18:09:58.3747536+02:00"),
                        CurrentHp = 44004,
                        MaxHp = 64167,
                        StatusEffects = new List<uint> {
                            360,
                            362,
                            2604,
                            48,
                            2611,
                            2599,
                            1204,
                            1951
                        }
                    }
                },
                new CombatEvent.StatusEffect {
                    Category = StatusCategory.Beneficial,
                    Id = 1839,
                    Source = "Gunbreaker",
                    Icon = 13609,
                    Duration = 15.0f,
                    Status = "Heart of Light",
                    Description = "Magic damage taken is reduced.",
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:09:58.4440242+02:00"), CurrentHp = 44004, MaxHp = 64167 }
                },
                new CombatEvent.StatusEffect {
                    Category = StatusCategory.Beneficial,
                    Id = 2611,
                    Source = "Sage",
                    Icon = 12957,
                    Duration = 30.0f,
                    Status = "Zoe",
                    Description = "Healing magic potency of next spell cast is increased.",
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:09:58.7843269+02:00"), CurrentHp = 44004, MaxHp = 64167 }
                },
                new CombatEvent.DamageTaken {
                    Source = "Proto-Carbuncle",
                    Amount = 42589,
                    Action = "Ruby Glow",
                    DamageType = (DamageType)5,
                    DisplayType = (ActionEffectDisplayType)1,
                    Icon = 405,
                    Snapshot = new CombatEvent.EventSnapshot {
                        Time = DateTime.Parse("2022-09-30T18:10:00.2567368+02:00"),
                        CurrentHp = 49789,
                        MaxHp = 64167,
                        StatusEffects = new List<uint> {
                            360,
                            362,
                            2604,
                            48,
                            1839,
                            2611,
                            2599,
                            1204,
                            1951
                        }
                    }
                },
                new CombatEvent.Healed {
                    Source = "Sage",
                    Amount = 21903,
                    Action = "Pneuma",
                    Icon = 405,
                    Snapshot = new CombatEvent.EventSnapshot {
                        Time = DateTime.Parse("2022-09-30T18:10:00.4860326+02:00"),
                        CurrentHp = 49789,
                        MaxHp = 64167,
                        StatusEffects = new List<uint> {
                            360,
                            362,
                            2604,
                            48,
                            1839,
                            2611,
                            2599,
                            1204,
                            1951
                        }
                    }
                },
                new CombatEvent.Healed {
                    Source = "Astrologian",
                    Amount = 5419,
                    Action = "Celestial Opposition",
                    Icon = 3142,
                    Snapshot = new CombatEvent.EventSnapshot {
                        Time = DateTime.Parse("2022-09-30T18:10:03.1179048+02:00"),
                        CurrentHp = 22219,
                        MaxHp = 64167,
                        StatusEffects = new List<uint> {
                            360,
                            362,
                            2604,
                            48,
                            1839,
                            2599,
                            1951
                        }
                    }
                },
                new CombatEvent.StatusEffect {
                    Category = StatusCategory.Beneficial,
                    Id = 1879,
                    Source = "Astrologian",
                    Icon = 13246,
                    Duration = 15.0f,
                    Status = "Opposition",
                    Description = "Regenerating HP over time.",
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:10:04.5000781+02:00"), CurrentHp = 27638, MaxHp = 64167 }
                },
                new CombatEvent.HoT {
                    Amount = 2738,
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:10:04.9511633+02:00"), CurrentHp = 27638, MaxHp = 64167 }
                },
                new CombatEvent.StatusEffect {
                    Category = StatusCategory.Beneficial,
                    Id = 2620,
                    Source = "Sage",
                    Icon = 12966,
                    Duration = 15.0f,
                    Status = "Physis II",
                    Description = "Regenerating HP over time.",
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:10:06.6040586+02:00"), CurrentHp = 31017, MaxHp = 64167 }
                },
                new CombatEvent.StatusEffect {
                    Category = StatusCategory.Beneficial,
                    Id = 2621,
                    Source = "Sage",
                    Icon = 12967,
                    Duration = 10.0f,
                    Status = "Autophysis",
                    Description = "HP recovery via healing actions is increased.",
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:10:06.6040674+02:00"), CurrentHp = 31017, MaxHp = 64167 }
                },
                new CombatEvent.HoT {
                    Amount = 5958,
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:10:07.9374975+02:00"), CurrentHp = 31017, MaxHp = 64167 }
                },
                new CombatEvent.StatusEffect {
                    Category = StatusCategory.Beneficial,
                    Id = 2618,
                    Source = "Sage",
                    Icon = 12964,
                    Duration = 15.0f,
                    Status = "Kerachole",
                    Description = "Damage taken is reduced.",
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:10:09.1391312+02:00"), CurrentHp = 37616, MaxHp = 64167 }
                },
                new CombatEvent.StatusEffect {
                    Category = StatusCategory.Beneficial,
                    Id = 2938,
                    Source = "Sage",
                    Icon = 12970,
                    Duration = 15.0f,
                    Status = "Kerakeia",
                    Description = "Regenerating HP over time.",
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:10:09.1391411+02:00"), CurrentHp = 37616, MaxHp = 64167 }
                },
                new CombatEvent.HoT {
                    Amount = 9009,
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:10:10.9106977+02:00"), CurrentHp = 37616, MaxHp = 64167 }
                },
                new CombatEvent.HoT {
                    Amount = 8701,
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:10:13.9443522+02:00"), CurrentHp = 47266, MaxHp = 64167 }
                },
                new CombatEvent.HoT {
                    Amount = 8502,
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:10:16.9444801+02:00"), CurrentHp = 56608, MaxHp = 64167 }
                },
                new CombatEvent.DamageTaken {
                    Source = "Proto-Carbuncle",
                    Amount = 41434,
                    Action = "Venom Pool",
                    DamageType = (DamageType)5,
                    DisplayType = (ActionEffectDisplayType)1,
                    Icon = 405,
                    Snapshot = new CombatEvent.EventSnapshot {
                        Time = DateTime.Parse("2022-09-30T18:10:17.3821939+02:00"),
                        CurrentHp = 64167,
                        MaxHp = 64167,
                        StatusEffects = new List<uint> {
                            360,
                            362,
                            2604,
                            48,
                            2620,
                            1879,
                            2618,
                            2938
                        }
                    }
                },
                new CombatEvent.StatusEffect {
                    Category = StatusCategory.Detrimental,
                    Id = 2941,
                    Source = "Proto-Carbuncle",
                    Icon = 15057,
                    Duration = 2.376f,
                    Status = "Magic Vulnerability Up",
                    Description = "Magic damage taken is increased.",
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:10:18.0280336+02:00"), CurrentHp = 22733, MaxHp = 64167 }
                },
                new CombatEvent.HoT {
                    Amount = 6963,
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:10:19.9310721+02:00"), CurrentHp = 23374, MaxHp = 64167 }
                },
                new CombatEvent.Healed {
                    Source = "Earthly Star",
                    Amount = 17620,
                    Action = "Stellar Explosion",
                    Icon = 405,
                    Snapshot = new CombatEvent.EventSnapshot {
                        Time = DateTime.Parse("2022-09-30T18:10:20.2713138+02:00"),
                        CurrentHp = 30337,
                        MaxHp = 64167,
                        StatusEffects = new List<uint> {
                            360,
                            362,
                            2604,
                            48,
                            2941,
                            2620,
                            2598,
                            2618,
                            2938
                        }
                    }
                },
                new CombatEvent.Healed {
                    Source = "Sage",
                    Amount = 2476,
                    Action = "Eukrasian Prognosis",
                    Icon = 3660,
                    Snapshot = new CombatEvent.EventSnapshot {
                        Time = DateTime.Parse("2022-09-30T18:10:22.0075268+02:00"),
                        CurrentHp = 48598,
                        MaxHp = 64167,
                        StatusEffects = new List<uint> {
                            360,
                            362,
                            2604,
                            48,
                            2606,
                            2598,
                            2618,
                            2938
                        }
                    }
                },
                new CombatEvent.StatusEffect {
                    Category = StatusCategory.Beneficial,
                    Id = 2609,
                    Source = "Sage",
                    Icon = 12954,
                    Duration = 29.111992f,
                    Status = "Eukrasian Prognosis",
                    Description = "A magicked barrier is nullifying damage.",
                    Snapshot = new CombatEvent.EventSnapshot {
                        Time = DateTime.Parse("2022-09-30T18:10:22.9448611+02:00"), CurrentHp = 51074, MaxHp = 64167, BarrierPercent = 12
                    }
                },
                new CombatEvent.HoT {
                    Amount = 3722,
                    Snapshot = new CombatEvent.EventSnapshot {
                        Time = DateTime.Parse("2022-09-30T18:10:22.9448717+02:00"), CurrentHp = 51074, MaxHp = 64167, BarrierPercent = 12
                    }
                },
                new CombatEvent.Healed {
                    Source = "Sage",
                    Amount = 7255,
                    Action = "Eukrasian Diagnosis",
                    Icon = 3659,
                    Snapshot = new CombatEvent.EventSnapshot {
                        Time = DateTime.Parse("2022-09-30T18:10:25.4941126+02:00"),
                        CurrentHp = 55437,
                        MaxHp = 64167,
                        StatusEffects = new List<uint> {
                            360,
                            362,
                            2604,
                            48,
                            3082,
                            2609,
                            2598,
                            2606
                        },
                        BarrierPercent = 12
                    }
                },
                new CombatEvent.DoT {
                    Amount = 12794,
                    Snapshot = new CombatEvent.EventSnapshot {
                        Time = DateTime.Parse("2022-09-30T18:10:25.930989+02:00"), CurrentHp = 55437, MaxHp = 64167, BarrierPercent = 20
                    }
                },
                new CombatEvent.HoT {
                    Amount = 858,
                    Snapshot = new CombatEvent.EventSnapshot {
                        Time = DateTime.Parse("2022-09-30T18:10:25.9309947+02:00"), CurrentHp = 55437, MaxHp = 64167, BarrierPercent = 20
                    }
                },
                new CombatEvent.StatusEffect {
                    Category = StatusCategory.Beneficial,
                    Id = 160,
                    Source = "Sage",
                    Icon = 10452,
                    Duration = 6.0f,
                    Status = "Surecast",
                    Description = "Spells cannot be interrupted by taking damage.",
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:10:28.7365375+02:00"), CurrentHp = 51397, MaxHp = 64167 }
                },
                new CombatEvent.DoT {
                    Amount = 25415,
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:10:28.9309607+02:00"), CurrentHp = 51397, MaxHp = 64167 }
                },
                new CombatEvent.HoT {
                    Amount = 859,
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:10:28.930965+02:00"), CurrentHp = 51397, MaxHp = 64167 }
                },
                new CombatEvent.StatusEffect {
                    Category = StatusCategory.Beneficial,
                    Id = 2643,
                    Source = "Sage",
                    Icon = 17355,
                    Duration = 15.0f,
                    Status = "Panhaimatinon",
                    Description =
                        "Stacks are consumed to restore the Panhaima barrier each time it is absorbed. Grants a healing effect when duration expires, its potency based on the number of remaining stacks.",
                    Snapshot = new CombatEvent.EventSnapshot {
                        Time = DateTime.Parse("2022-09-30T18:10:29.6255502+02:00"), CurrentHp = 26841, MaxHp = 64167, BarrierPercent = 7
                    }
                },
                new CombatEvent.StatusEffect {
                    Category = StatusCategory.Beneficial,
                    Id = 2613,
                    Source = "Sage",
                    Icon = 12959,
                    Duration = 15.0f,
                    Status = "Panhaima",
                    Description = "A magicked barrier is nullifying damage.",
                    Snapshot = new CombatEvent.EventSnapshot {
                        Time = DateTime.Parse("2022-09-30T18:10:29.6255603+02:00"), CurrentHp = 26841, MaxHp = 64167, BarrierPercent = 7
                    }
                },
                new CombatEvent.Healed {
                    Source = "Sage",
                    Amount = 7323,
                    Action = "Eukrasian Diagnosis",
                    Icon = 3659,
                    Snapshot = new CombatEvent.EventSnapshot {
                        Time = DateTime.Parse("2022-09-30T18:10:30.6188893+02:00"),
                        CurrentHp = 27482,
                        MaxHp = 64167,
                        StatusEffects = new List<uint> {
                            360,
                            362,
                            2604,
                            48,
                            3082,
                            160,
                            2598,
                            2643,
                            2613,
                            2606
                        },
                        BarrierPercent = 7
                    }
                },
                new CombatEvent.StatusEffect {
                    Category = StatusCategory.Beneficial,
                    Id = 2607,
                    Source = "Sage",
                    Icon = 12954,
                    Duration = 29.377995f,
                    Status = "Eukrasian Diagnosis",
                    Description = "A magicked barrier is nullifying damage.",
                    Snapshot = new CombatEvent.EventSnapshot {
                        Time = DateTime.Parse("2022-09-30T18:10:31.2851132+02:00"), CurrentHp = 34805, MaxHp = 64167, BarrierPercent = 28
                    }
                },
                new CombatEvent.StatusEffect {
                    Category = StatusCategory.Beneficial,
                    Id = 2683,
                    Source = "Gunbreaker",
                    Icon = 13615,
                    Duration = 8.0f,
                    Status = "Heart of Corundum",
                    Description = "Damage taken is reduced.",
                    Snapshot = new CombatEvent.EventSnapshot {
                        Time = DateTime.Parse("2022-09-30T18:10:31.5564633+02:00"), CurrentHp = 34805, MaxHp = 64167, BarrierPercent = 28
                    }
                },
                new CombatEvent.StatusEffect {
                    Category = StatusCategory.Beneficial,
                    Id = 2684,
                    Source = "Gunbreaker",
                    Icon = 13616,
                    Duration = 4.0f,
                    Status = "Clarity of Corundum",
                    Description = "Damage taken is reduced.",
                    Snapshot = new CombatEvent.EventSnapshot {
                        Time = DateTime.Parse("2022-09-30T18:10:31.5564732+02:00"), CurrentHp = 34805, MaxHp = 64167, BarrierPercent = 28
                    }
                },
                new CombatEvent.StatusEffect {
                    Category = StatusCategory.Beneficial,
                    Id = 2685,
                    Source = "Gunbreaker",
                    Icon = 13617,
                    Duration = 20.0f,
                    Status = "Catharsis of Corundum",
                    Description =
                        "HP will be restored automatically upon falling below a certain level or expiration of effect duration.",
                    Snapshot = new CombatEvent.EventSnapshot {
                        Time = DateTime.Parse("2022-09-30T18:10:31.5564771+02:00"), CurrentHp = 34805, MaxHp = 64167, BarrierPercent = 28
                    }
                },
                new CombatEvent.HoT {
                    Amount = 6167,
                    Snapshot = new CombatEvent.EventSnapshot {
                        Time = DateTime.Parse("2022-09-30T18:10:31.9446469+02:00"), CurrentHp = 34805, MaxHp = 64167, BarrierPercent = 28
                    }
                },
                new CombatEvent.HoT {
                    Amount = 936,
                    Snapshot = new CombatEvent.EventSnapshot {
                        Time = DateTime.Parse("2022-09-30T18:10:31.9446514+02:00"), CurrentHp = 34805, MaxHp = 64167, BarrierPercent = 28
                    }
                },
                new CombatEvent.DoT {
                    Amount = 26320,
                    Snapshot = new CombatEvent.EventSnapshot {
                        Time = DateTime.Parse("2022-09-30T18:10:31.9446675+02:00"), CurrentHp = 34805, MaxHp = 64167, BarrierPercent = 28
                    }
                },
                new CombatEvent.DamageTaken {
                    Source = "Proto-Carbuncle",
                    Amount = 36472,
                    Action = "Double Rush",
                    DamageType = (DamageType)3,
                    DisplayType = (ActionEffectDisplayType)1,
                    Icon = 405,
                    Snapshot = new CombatEvent.EventSnapshot {
                        Time = DateTime.Parse("2022-09-30T18:10:32.4240217+02:00"),
                        CurrentHp = 55894,
                        MaxHp = 64167,
                        StatusEffects = new List<uint> {
                            360,
                            362,
                            2604,
                            48,
                            3082,
                            160,
                            2598,
                            2643,
                            2613,
                            2683,
                            2684
                        },
                        BarrierPercent = 7
                    }
                },
                new CombatEvent.StatusEffect {
                    Category = StatusCategory.Beneficial,
                    Id = 2613,
                    Source = "Sage",
                    Icon = 12959,
                    Duration = 15.0f,
                    Status = "Panhaima",
                    Description = "A magicked barrier is nullifying damage.",
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:10:33.4031393+02:00"), CurrentHp = 20063, MaxHp = 64167 }
                },
                new CombatEvent.StatusEffect {
                    Category = StatusCategory.Beneficial,
                    Id = 2643,
                    Source = "Sage",
                    Icon = 17355,
                    Duration = 11.267013f,
                    Status = "Panhaimatinon",
                    Description =
                        "Stacks are consumed to restore the Panhaima barrier each time it is absorbed. Grants a healing effect when duration expires, its potency based on the number of remaining stacks.",
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:10:33.4031492+02:00"), CurrentHp = 20063, MaxHp = 64167 }
                },
                new CombatEvent.DoT {
                    Amount = 20640,
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:10:34.9313721+02:00"), CurrentHp = 0, MaxHp = 64167 }
                },
                new CombatEvent.HoT {
                    Amount = 2469,
                    Snapshot = new CombatEvent.EventSnapshot { Time = DateTime.Parse("2022-09-30T18:10:34.9313779+02:00"), CurrentHp = 0, MaxHp = 64167 }
                }
            }
        });
        plugin.DeathsPerPlayer.AddEntry(0u,
            new Death {
                PlayerId = 275579810,
                PlayerName = "Scholar",
                TimeOfDeath = DateTime.Parse("2022-09-29T19:55:43.9918+02:00"),
                Events = new List<CombatEvent> {
                    new CombatEvent.StatusEffect {
                        Category = StatusCategory.Beneficial,
                        Id = 297,
                        Source = "Scholar",
                        Icon = 12801,
                        Duration = 29.0f,
                        Status = "Galvanize",
                        Description = "A magicked barrier is nullifying damage.",
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:54:41.4765639+02:00"), CurrentHp = 56605, MaxHp = 61202, BarrierPercent = 41
                        }
                    },
                    new CombatEvent.HoT {
                        Amount = 3086,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:54:41.5876295+02:00"), CurrentHp = 56605, MaxHp = 61202, BarrierPercent = 41
                        }
                    },
                    new CombatEvent.HoT {
                        Amount = 2986,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:54:42.2198938+02:00"), CurrentHp = 59691, MaxHp = 61202, BarrierPercent = 41
                        }
                    },
                    new CombatEvent.HoT {
                        Amount = 0,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:54:42.2199431+02:00"), CurrentHp = 59691, MaxHp = 61202, BarrierPercent = 41
                        }
                    },
                    new CombatEvent.HoT {
                        Amount = 6009,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:54:42.2199472+02:00"), CurrentHp = 59691, MaxHp = 61202, BarrierPercent = 41
                        }
                    },
                    new CombatEvent.HoT {
                        Amount = 4950,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:54:44.5526758+02:00"), CurrentHp = 61202, MaxHp = 61202, BarrierPercent = 19
                        }
                    },
                    new CombatEvent.HoT {
                        Amount = 1933,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:54:45.2197323+02:00"), CurrentHp = 61202, MaxHp = 61202, BarrierPercent = 19
                        }
                    },
                    new CombatEvent.HoT {
                        Amount = 3190,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:54:47.6016013+02:00"), CurrentHp = 61202, MaxHp = 61202, BarrierPercent = 19
                        }
                    },
                    new CombatEvent.HoT {
                        Amount = 2898,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:54:50.5530659+02:00"), CurrentHp = 61202, MaxHp = 61202, BarrierPercent = 19
                        }
                    },
                    new CombatEvent.HoT {
                        Amount = 2928,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:54:53.5877742+02:00"), CurrentHp = 61202, MaxHp = 61202, BarrierPercent = 19
                        }
                    },
                    new CombatEvent.HoT {
                        Amount = 2809,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:54:56.5600275+02:00"), CurrentHp = 61202, MaxHp = 61202, BarrierPercent = 19
                        }
                    },
                    new CombatEvent.Healed {
                        Source = "White Mage",
                        Amount = 10861,
                        Action = "Medica II",
                        Icon = 409,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:54:57.3379329+02:00"),
                            CurrentHp = 61202,
                            MaxHp = 61202,
                            StatusEffects = new List<uint> {
                                360,
                                364,
                                48,
                                1204,
                                1912,
                                297
                            },
                            BarrierPercent = 19
                        }
                    },
                    new CombatEvent.StatusEffect {
                        Category = StatusCategory.Beneficial,
                        Id = 150,
                        Source = "White Mage",
                        Icon = 10413,
                        Duration = 15.0f,
                        Status = "Medica II",
                        Description = "Regenerating HP over time.",
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:54:58.8863873+02:00"), CurrentHp = 61202, MaxHp = 61202, BarrierPercent = 19
                        }
                    },
                    new CombatEvent.HoT {
                        Amount = 4181,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:00.1998629+02:00"), CurrentHp = 61202, MaxHp = 61202, BarrierPercent = 19
                        }
                    },
                    new CombatEvent.Healed {
                        Source = "White Mage",
                        Amount = 6509,
                        Action = "Medica II",
                        Icon = 409,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:02.2206922+02:00"),
                            CurrentHp = 61202,
                            MaxHp = 61202,
                            StatusEffects = new List<uint> {
                                360,
                                364,
                                48,
                                150,
                                297
                            },
                            BarrierPercent = 19
                        }
                    },
                    new CombatEvent.HoT {
                        Amount = 6926,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:03.2550274+02:00"), CurrentHp = 61202, MaxHp = 61202, BarrierPercent = 19
                        }
                    },
                    new CombatEvent.StatusEffect {
                        Category = StatusCategory.Beneficial,
                        Id = 150,
                        Source = "White Mage",
                        Icon = 10413,
                        Duration = 15.0f,
                        Status = "Medica II",
                        Description = "Regenerating HP over time.",
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:03.9976567+02:00"), CurrentHp = 61202, MaxHp = 61202, BarrierPercent = 19
                        }
                    },
                    new CombatEvent.HoT {
                        Amount = 5943,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:06.1990313+02:00"), CurrentHp = 61202, MaxHp = 61202, BarrierPercent = 19
                        }
                    },
                    new CombatEvent.HoT {
                        Amount = 3838,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:09.2762546+02:00"), CurrentHp = 61202, MaxHp = 61202, BarrierPercent = 19
                        }
                    },
                    new CombatEvent.Healed {
                        Source = "Scholar",
                        Amount = 7643,
                        Action = "Succor",
                        Icon = 2802,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:11.6235309+02:00"),
                            CurrentHp = 61202,
                            MaxHp = 61202,
                            StatusEffects = new List<uint> {
                                360,
                                364,
                                48,
                                150,
                                304
                            },
                            BarrierPercent = 0
                        }
                    },
                    new CombatEvent.HoT {
                        Amount = 4021,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:12.2269096+02:00"), CurrentHp = 61202, MaxHp = 61202, BarrierPercent = 19
                        }
                    },
                    new CombatEvent.StatusEffect {
                        Category = StatusCategory.Beneficial,
                        Id = 297,
                        Source = "Scholar",
                        Icon = 12801,
                        Duration = 28.928993f,
                        Status = "Galvanize",
                        Description = "A magicked barrier is nullifying damage.",
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:12.7409602+02:00"), CurrentHp = 61202, MaxHp = 61202, BarrierPercent = 19
                        }
                    },
                    new CombatEvent.HoT {
                        Amount = 3909,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:15.1716393+02:00"), CurrentHp = 61202, MaxHp = 61202, BarrierPercent = 19
                        }
                    },
                    new CombatEvent.HoT {
                        Amount = 6532,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:18.2031049+02:00"), CurrentHp = 61202, MaxHp = 61202, BarrierPercent = 19
                        }
                    },
                    new CombatEvent.Healed {
                        Source = "Scholar",
                        Amount = 7578,
                        Action = "Succor",
                        Icon = 2802,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:19.7206667+02:00"),
                            CurrentHp = 61202,
                            MaxHp = 61202,
                            StatusEffects = new List<uint> {
                                360,
                                364,
                                48,
                                304,
                                297
                            },
                            BarrierPercent = 19
                        }
                    },
                    new CombatEvent.StatusEffect {
                        Category = StatusCategory.Beneficial,
                        Id = 315,
                        Source = "Selene",
                        Icon = 12826,
                        Duration = 21.0f,
                        Status = "Whispering Dawn",
                        Description = "Regenerating HP over time.",
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:21.8871376+02:00"), CurrentHp = 61202, MaxHp = 61202, BarrierPercent = 19
                        }
                    },
                    new CombatEvent.DamageTaken {
                        Source = "Hephaistos",
                        Amount = 45020,
                        Action = "Forcible Fire II",
                        DirectHit = true,
                        DamageType = (DamageType)5,
                        DisplayType = (ActionEffectDisplayType)1,
                        Icon = 405,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:21.9571395+02:00"),
                            CurrentHp = 61202,
                            MaxHp = 61202,
                            StatusEffects = new List<uint> {
                                360,
                                364,
                                48,
                                315,
                                304,
                                297
                            },
                            BarrierPercent = 19
                        }
                    },
                    new CombatEvent.StatusEffect {
                        Category = StatusCategory.Beneficial,
                        Id = 1219,
                        Source = "White Mage",
                        Icon = 12637,
                        Duration = 10.0f,
                        Status = "Confession",
                        Description = "Sins are confessed. Ready for Plenary Indulgence.",
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:22.678862+02:00"), CurrentHp = 61202, MaxHp = 61202, BarrierPercent = 19
                        }
                    },
                    new CombatEvent.StatusEffect {
                        Category = StatusCategory.Detrimental,
                        Id = 2941,
                        Source = "Hephaistos",
                        Icon = 15057,
                        Duration = 1.2430001f,
                        Status = "Magic Vulnerability Up",
                        Description = "Magic damage taken is increased.",
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:22.7416297+02:00"), CurrentHp = 16182, MaxHp = 61202, BarrierPercent = 0
                        }
                    },
                    new CombatEvent.Healed {
                        Source = "White Mage",
                        Amount = 16316,
                        Action = "Afflatus Rapture",
                        Icon = 2643,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:23.0054561+02:00"),
                            CurrentHp = 16182,
                            MaxHp = 61202,
                            StatusEffects = new List<uint> {
                                360,
                                364,
                                48,
                                315,
                                304,
                                2941,
                                1219
                            },
                            BarrierPercent = 0
                        }
                    },
                    new CombatEvent.Healed {
                        Source = "Scholar",
                        Amount = 4969,
                        Action = "Succor",
                        Icon = 2802,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:23.7000311+02:00"),
                            CurrentHp = 16182,
                            MaxHp = 61202,
                            StatusEffects = new List<uint> {
                                360,
                                364,
                                48,
                                315,
                                304,
                                2941,
                                1219
                            },
                            BarrierPercent = 0
                        }
                    },
                    new CombatEvent.Healed {
                        Source = "White Mage",
                        Amount = 10274,
                        Action = "Assize",
                        Icon = 2634,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:23.7626569+02:00"),
                            CurrentHp = 16182,
                            MaxHp = 61202,
                            StatusEffects = new List<uint> {
                                360,
                                364,
                                48,
                                315,
                                304,
                                297,
                                2941,
                                1219
                            },
                            BarrierPercent = 12
                        }
                    },
                    new CombatEvent.HoT {
                        Amount = 5066,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:23.9914224+02:00"), CurrentHp = 16182, MaxHp = 61202, BarrierPercent = 12
                        }
                    },
                    new CombatEvent.HoT {
                        Amount = 2766,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:24.2070599+02:00"), CurrentHp = 37564, MaxHp = 61202, BarrierPercent = 12
                        }
                    },
                    new CombatEvent.Healed {
                        Source = "Scholar",
                        Amount = 15646,
                        Action = "Indomitability",
                        Icon = 2806,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:24.4117998+02:00"),
                            CurrentHp = 40330,
                            MaxHp = 61202,
                            StatusEffects = new List<uint> {
                                360,
                                364,
                                48,
                                315,
                                304,
                                297,
                                1219
                            },
                            BarrierPercent = 12
                        }
                    },
                    new CombatEvent.StatusEffect {
                        Category = StatusCategory.Beneficial,
                        Id = 297,
                        Source = "Scholar",
                        Icon = 12801,
                        Duration = 28.932991f,
                        Status = "Galvanize",
                        Description = "A magicked barrier is nullifying damage.",
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:24.8247652+02:00"), CurrentHp = 45911, MaxHp = 61202, BarrierPercent = 12
                        }
                    },
                    new CombatEvent.HoT {
                        Amount = 1872,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:27.2344709+02:00"), CurrentHp = 61202, MaxHp = 61202, BarrierPercent = 12
                        }
                    },
                    new CombatEvent.DamageTaken {
                        Source = "Hephaistos",
                        Amount = 37545,
                        Action = "Forcible Difreeze",
                        DirectHit = true,
                        DamageType = (DamageType)5,
                        DisplayType = (ActionEffectDisplayType)1,
                        Icon = 405,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:28.0611752+02:00"),
                            CurrentHp = 61202,
                            MaxHp = 61202,
                            StatusEffects = new List<uint> {
                                360,
                                364,
                                48,
                                315,
                                304,
                                297,
                                1219
                            },
                            BarrierPercent = 12
                        }
                    },
                    new CombatEvent.StatusEffect {
                        Category = StatusCategory.Detrimental,
                        Id = 2941,
                        Source = "Hephaistos",
                        Icon = 15057,
                        Duration = 1.3339999f,
                        Status = "Magic Vulnerability Up",
                        Description = "Magic damage taken is increased.",
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:28.7003957+02:00"), CurrentHp = 23657, MaxHp = 61202, BarrierPercent = 0
                        }
                    },
                    new CombatEvent.Healed {
                        Source = "White Mage",
                        Amount = 10286,
                        Action = "Afflatus Rapture",
                        Icon = 2643,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:30.1169648+02:00"),
                            CurrentHp = 23657,
                            MaxHp = 61202,
                            StatusEffects = new List<uint> {
                                360,
                                364,
                                48,
                                315,
                                304,
                                1219
                            },
                            BarrierPercent = 0
                        }
                    },
                    new CombatEvent.HoT {
                        Amount = 3022,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:30.1860675+02:00"), CurrentHp = 23657, MaxHp = 61202, BarrierPercent = 0
                        }
                    },
                    new CombatEvent.HoT {
                        Amount = 5018,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:31.3667374+02:00"), CurrentHp = 27291, MaxHp = 61202, BarrierPercent = 0
                        }
                    },
                    new CombatEvent.Healed {
                        Source = "Warrior",
                        Amount = 5648,
                        Action = "Shake It Off",
                        Icon = 2563,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:31.6656228+02:00"),
                            CurrentHp = 42595,
                            MaxHp = 61202,
                            StatusEffects = new List<uint> {
                                360,
                                364,
                                48,
                                315,
                                304,
                                1219
                            },
                            BarrierPercent = 0
                        }
                    },
                    new CombatEvent.StatusEffect {
                        Category = StatusCategory.Beneficial,
                        Id = 1457,
                        Source = "Warrior",
                        Icon = 12557,
                        Duration = 13.708003f,
                        Status = "Shake It Off",
                        Description = "A highly effective defensive maneuver is nullifying damage.",
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:32.9776557+02:00"), CurrentHp = 48243, MaxHp = 61202, BarrierPercent = 14
                        }
                    },
                    new CombatEvent.HoT {
                        Amount = 1826,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:33.234978+02:00"), CurrentHp = 48243, MaxHp = 61202, BarrierPercent = 14
                        }
                    },
                    new CombatEvent.Healed {
                        Source = "Scholar",
                        Amount = 4868,
                        Action = "Succor",
                        Icon = 2802,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:33.3403819+02:00"),
                            CurrentHp = 50069,
                            MaxHp = 61202,
                            StatusEffects = new List<uint> {
                                360,
                                364,
                                48,
                                315,
                                304,
                                1457
                            },
                            BarrierPercent = 14
                        }
                    },
                    new CombatEvent.StatusEffect {
                        Category = StatusCategory.Beneficial,
                        Id = 297,
                        Source = "Scholar",
                        Icon = 12801,
                        Duration = 28.931992f,
                        Status = "Galvanize",
                        Description = "A magicked barrier is nullifying damage.",
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:34.4577388+02:00"), CurrentHp = 55549, MaxHp = 61202, BarrierPercent = 27
                        }
                    },
                    new CombatEvent.Healed {
                        Source = "Seraph",
                        Amount = 5585,
                        Action = "Consolation",
                        Icon = 2846,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:34.7766812+02:00"),
                            CurrentHp = 55549,
                            MaxHp = 61202,
                            StatusEffects = new List<uint> {
                                360,
                                364,
                                48,
                                315,
                                304,
                                1457,
                                297
                            },
                            BarrierPercent = 27
                        }
                    },
                    new CombatEvent.Healed {
                        Source = "White Mage",
                        Amount = 10244,
                        Action = "Afflatus Rapture",
                        Icon = 2643,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:35.3260379+02:00"),
                            CurrentHp = 55549,
                            MaxHp = 61202,
                            StatusEffects = new List<uint> {
                                360,
                                364,
                                48,
                                315,
                                304,
                                1457,
                                297,
                                1917
                            },
                            BarrierPercent = 36
                        }
                    },
                    new CombatEvent.HoT {
                        Amount = 1839,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:36.1932848+02:00"), CurrentHp = 55549, MaxHp = 61202, BarrierPercent = 36
                        }
                    },
                    new CombatEvent.StatusEffect {
                        Category = StatusCategory.Beneficial,
                        Id = 1917,
                        Source = "Seraph",
                        Icon = 12848,
                        Duration = 28.444992f,
                        Status = "Seraphic Veil",
                        Description = "A holy barrier is nullifying damage.",
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:36.3739846+02:00"), CurrentHp = 61202, MaxHp = 61202, BarrierPercent = 36
                        }
                    },
                    new CombatEvent.HoT {
                        Amount = 1723,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:39.2069314+02:00"), CurrentHp = 61202, MaxHp = 61202, BarrierPercent = 36
                        }
                    },
                    new CombatEvent.StatusEffect {
                        Category = StatusCategory.Beneficial,
                        Id = 1204,
                        Source = "Scholar",
                        Icon = 13909,
                        Duration = 21.0f,
                        Status = "Lucid Dreaming",
                        Description = "Restoring MP over time.",
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:39.4847986+02:00"), CurrentHp = 61202, MaxHp = 61202, BarrierPercent = 36
                        }
                    },
                    new CombatEvent.DamageTaken {
                        Source = "Hephaistos",
                        Amount = 35697,
                        Action = "Forcible Trifire",
                        DirectHit = true,
                        DamageType = (DamageType)5,
                        DisplayType = (ActionEffectDisplayType)1,
                        Icon = 405,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:40.873757+02:00"),
                            CurrentHp = 61202,
                            MaxHp = 61202,
                            StatusEffects = new List<uint> {
                                360,
                                364,
                                48,
                                315,
                                304,
                                1457,
                                297,
                                1917,
                                1204
                            },
                            BarrierPercent = 36
                        }
                    },
                    new CombatEvent.DamageTaken {
                        Source = "Hephaistos",
                        Amount = 9999999,
                        Action = "Forcible Failure",
                        DirectHit = true,
                        DamageType = (DamageType)0,
                        DisplayType = (ActionEffectDisplayType)1,
                        Icon = 405,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:41.0267921+02:00"),
                            CurrentHp = 61202,
                            MaxHp = 61202,
                            StatusEffects = new List<uint> {
                                360,
                                364,
                                48,
                                315,
                                304,
                                1457,
                                297,
                                1917,
                                1204,
                                2941
                            },
                            BarrierPercent = 36
                        }
                    },
                    new CombatEvent.DamageTaken {
                        Source = "Hephaistos",
                        Amount = 9999999,
                        Action = "Forcible Failure",
                        DirectHit = true,
                        DamageType = (DamageType)0,
                        DisplayType = (ActionEffectDisplayType)1,
                        Icon = 405,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:41.0268404+02:00"),
                            CurrentHp = 61202,
                            MaxHp = 61202,
                            StatusEffects = new List<uint> {
                                360,
                                364,
                                48,
                                315,
                                304,
                                1457,
                                297,
                                1917,
                                1204,
                                2941
                            },
                            BarrierPercent = 36
                        }
                    },
                    new CombatEvent.StatusEffect {
                        Category = StatusCategory.Detrimental,
                        Id = 2941,
                        Source = "Hephaistos",
                        Icon = 15057,
                        Duration = 1.3320001f,
                        Status = "Magic Vulnerability Up",
                        Description = "Magic damage taken is increased.",
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:41.5219014+02:00"), CurrentHp = 25505, MaxHp = 61202, BarrierPercent = 0
                        }
                    },
                    new CombatEvent.Healed {
                        Source = "Seraph",
                        Amount = 5475,
                        Action = "Consolation",
                        Icon = 2846,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:41.645083+02:00"),
                            CurrentHp = 25505,
                            MaxHp = 61202,
                            StatusEffects = new List<uint> {
                                360,
                                364,
                                48,
                                315,
                                304,
                                1204,
                                2941
                            },
                            BarrierPercent = 0
                        }
                    },
                    new CombatEvent.Healed {
                        Source = "Liturgic Bell",
                        Amount = 9553,
                        Action = "Liturgy of the Bell",
                        Icon = 405,
                        Snapshot = new CombatEvent.EventSnapshot {
                            Time = DateTime.Parse("2022-09-29T19:55:41.6451593+02:00"),
                            CurrentHp = 25505,
                            MaxHp = 61202,
                            StatusEffects = new List<uint> {
                                360,
                                364,
                                48,
                                315,
                                304,
                                1204,
                                2941
                            },
                            BarrierPercent = 0
                        }
                    }
                }
            });

        plugin.NotificationHandler.DisplayDeath(plugin.DeathsPerPlayer[0][0]);
        plugin.Window.IsOpen = true;
    }
}
#endif
