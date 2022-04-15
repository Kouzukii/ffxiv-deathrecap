using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Network;
using Dalamud.Logging;
using DeathRecap.Messages;
using Lumina.Excel.GeneratedSheets;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace DeathRecap {
    public class CombatEventCapture {
        private readonly Dictionary<uint, List<CombatEvent>> combatEvents = new();
        private readonly DeathRecapPlugin plugin;

        public CombatEventCapture(DeathRecapPlugin plugin) {
            this.plugin = plugin;
        }

        public unsafe void GameNetworkOnNetworkMessage(IntPtr dataptr, ushort opcode, uint placeholder, uint targetactorid, NetworkMessageDirection direction) {
            try {
                switch ((Opcodes)opcode) {
                    case Opcodes.ActorControl: {
                        if (!plugin.ConditionEvaluator.ShouldCapture(targetactorid))
                            break;
                        if (Service.ObjectTable.SearchById(targetactorid) is PlayerCharacter p) {
                            var actorCtrl = (ActorControl142*)dataptr;
                            if (actorCtrl->Category == ActorControlCategory.HoTDoT) {
                                if (actorCtrl->Param2 == 4)
                                    combatEvents.AddEntry(targetactorid,
                                        new CombatEvent.HoT { Snapshot = p.Snapshot(), Amount = actorCtrl->Param3, Id = actorCtrl->Param1 });
                                else if (actorCtrl->Param2 == 3)
                                    combatEvents.AddEntry(targetactorid,
                                        new CombatEvent.DoT { Snapshot = p.Snapshot(), Amount = actorCtrl->Param3, Id = actorCtrl->Param1 });
                            } else if (actorCtrl->Category == ActorControlCategory.Death) {
                                if (combatEvents.Remove(targetactorid, out var events)) {
                                    var death = new Death {
                                        PlayerId = targetactorid,
                                        PlayerName = p.Name.TextValue,
                                        TimeOfDeath = DateTime.Now,
                                        Events = events
                                    };
                                    plugin.DeathsPerPlayer.AddEntry(targetactorid, death);
                                    plugin.NotificationHandler.DisplayDeath(death);
                                }
                            }
                        }

                        break;
                    }
                    case Opcodes.EffectResult: {
                        if (!plugin.ConditionEvaluator.ShouldCapture(targetactorid))
                            break;
                        if (Service.ObjectTable.SearchById(targetactorid) is PlayerCharacter p) {
                            var message = (AddStatusEffect*)dataptr;
                            var effects = (StatusEffectAddEntry*)message->Effects;
                            var effectCount = Math.Min(message->EffectCount, 4u);
                            for (uint j = 0; j < effectCount; j++) {
                                var effect = effects[j];
                                var effectId = effect.EffectId;
                                if (effectId <= 0)
                                    continue;
                                // TODO: Figure out what negative values mean
                                if (effect.Duration < 0)
                                    continue;
                                var source = Service.ObjectTable.SearchById(effect.SourceActorId)?.Name.TextValue;
                                var status = Service.DataManager.Excel.GetSheet<Status>()?.GetRow(effectId);

                                combatEvents.AddEntry(targetactorid, new CombatEvent.StatusEffect {
                                    Snapshot = p.Snapshot(),
                                    Id = effectId,
                                    Icon = status?.Icon,
                                    Status = status?.Name.RawString,
                                    Description = status?.Description.DisplayedText(),
                                    Source = source,
                                    Duration = effect.Duration
                                });
                            }
                        }

                        break;
                    }
                    case Opcodes.Ability1:
                    case Opcodes.Ability8:
                    case Opcodes.Ability16:
                    case Opcodes.Ability24:
                    case Opcodes.Ability32: {
                        var message = (ActionEffectHeader*)dataptr;
                        ulong* targetIds = null;
                        byte* effectData = null;
                        uint targets = message->EffectCount;
                        switch ((Opcodes)opcode) {
                            case Opcodes.Ability1:
                                targetIds = ((ActionEffect1*)dataptr)->TargetIds;
                                effectData = ((ActionEffect1*)dataptr)->Effects;
                                targets = Math.Min(targets, 1u);
                                break;
                            case Opcodes.Ability8:
                                targetIds = ((ActionEffect8*)dataptr)->TargetIds;
                                effectData = ((ActionEffect8*)dataptr)->Effects;
                                targets = Math.Min(targets, 8u);
                                break;
                            case Opcodes.Ability16:
                                targetIds = ((ActionEffect16*)dataptr)->TargetIds;
                                effectData = ((ActionEffect16*)dataptr)->Effects;
                                targets = Math.Min(targets, 16u);
                                break;
                            case Opcodes.Ability24:
                                targetIds = ((ActionEffect24*)dataptr)->TargetIds;
                                effectData = ((ActionEffect24*)dataptr)->Effects;
                                targets = Math.Min(targets, 24u);
                                break;
                            case Opcodes.Ability32:
                                targetIds = ((ActionEffect32*)dataptr)->TargetIds;
                                effectData = ((ActionEffect32*)dataptr)->Effects;
                                targets = Math.Min(targets, 32u);
                                break;
                        }

                        if (targetIds == null || effectData == null || targets == 0)
                            break;

                        var actionId = message->EffectDisplayType switch {
                            ActionEffectDisplayType.MountName => 218103808 + message->ActionId,
                            ActionEffectDisplayType.ShowItemName => 33554432 + message->ActionId,
                            _ => message->ActionAnimationId
                        };
                        Action? action = null;
                        string? source = null;
                        GameObject? gameObject = null;
                        List<uint>? additionalStatus = null;

                        for (var i = 0; i < targets; i++) {
                            var actionTargetId = (uint)(targetIds[i] & uint.MaxValue);
                            if (!plugin.ConditionEvaluator.ShouldCapture(actionTargetId))
                                continue;
                            if (Service.ObjectTable.SearchById(actionTargetId) is PlayerCharacter p)
                                for (var j = 0; j < 8; j++) {
                                    var actionIndex = i * 64 + j * 8;
                                    if (effectData[actionIndex] == 0)
                                        continue;
                                    var amount = (effectData[actionIndex + 7] << 8) + effectData[actionIndex + 6];
                                    if ((effectData[actionIndex + 5] & 0x40) == 0x40)
                                        amount += effectData[actionIndex + 4] << 16;

                                    var actionType = ConvertActionType(effectData[actionIndex]);
                                    action ??= Service.DataManager.Excel.GetSheet<Action>()?.GetRow(actionId);
                                    gameObject ??= Service.ObjectTable.SearchById(targetactorid);
                                    source ??= gameObject?.Name.TextValue;

                                    switch (actionType) {
                                        case ActionType.Ability:
                                        case ActionType.AbilityBlocked:
                                        case ActionType.AbilityParried:
                                            combatEvents.AddEntry(actionTargetId,
                                                new CombatEvent.DamageTaken {
                                                    Snapshot =
                                                        p.Snapshot(true,
                                                            additionalStatus ??= gameObject is BattleChara b
                                                                ? b.StatusList.Select(s => s.StatusId).Where(s => s is 1203 or 1195 or 1193).ToList()
                                                                : new List<uint>()),
                                                    Source = source,
                                                    Amount = amount,
                                                    Action = action?.Name?.RawString,
                                                    Icon = action?.Icon,
                                                    Crit = (effectData[actionIndex + 1] & 1) == 1,
                                                    DirectHit = (effectData[actionIndex + 1] & 2) == 2,
                                                    DamageType = (DamageType)(effectData[actionIndex + 2] & 0xF),
                                                    Parried = actionType == ActionType.AbilityParried,
                                                    Blocked = actionType == ActionType.AbilityBlocked,
                                                    DisplayType = message->EffectDisplayType
                                                });
                                            break;
                                        case ActionType.Healing:
                                            combatEvents.AddEntry(actionTargetId,
                                                new CombatEvent.Healed {
                                                    Snapshot = p.Snapshot(true),
                                                    Source = source,
                                                    Amount = amount,
                                                    Action = action?.Name?.RawString,
                                                    Icon = action?.Icon,
                                                    Crit = (effectData[actionIndex + 2] & 1) == 1,
                                                    DirectHit = (effectData[actionIndex + 2] & 2) == 2
                                                });
                                            break;
                                    }
                                }
                        }

                        break;
                    }
                }
            } catch (Exception e) {
                PluginLog.Error(e, "Failed to handle network packet");
            }
        }

        private ActionType ConvertActionType(byte ability) {
            return ability switch {
                1 => ActionType.Ability,
                2 => ActionType.Ability,
                3 => ActionType.Ability,
                4 => ActionType.Healing,
                5 => ActionType.AbilityBlocked,
                6 => ActionType.AbilityParried,
                10 => ActionType.PowerDrain,
                11 => ActionType.PowerHealing,
                13 => ActionType.TpHeal,
                14 => ActionType.Buff,
                15 => ActionType.Buff,
                24 => ActionType.Threat,
                25 => ActionType.Threat,
                52 => ActionType.Buff,
                _ => ActionType.Other
            };
        }

        public void CleanCombatEvents() {
            try {
                var entriesToRemove = new List<uint>();
                foreach (var (id, events) in combatEvents) {
                    if (events.Count == 0 || (DateTime.Now - events.Last().Snapshot.Time).TotalSeconds > plugin.Configuration.KeepCombatEventsForSeconds) {
                        entriesToRemove.Add(id);
                        continue;
                    }

                    var cutOffTime = DateTime.Now - TimeSpan.FromSeconds(plugin.Configuration.KeepCombatEventsForSeconds);
                    for (var i = 0; i < events.Count; i++)
                        if (events[i].Snapshot.Time > cutOffTime) {
                            events.RemoveRange(0, i);
                            break;
                        }
                }

                foreach (var entry in entriesToRemove)
                    combatEvents.Remove(entry);

                entriesToRemove.Clear();

                foreach (var (id, death) in plugin.DeathsPerPlayer) {
                    if (death.Count == 0 || (DateTime.Now - death.Last().TimeOfDeath).TotalMinutes > plugin.Configuration.KeepDeathsForMinutes) {
                        entriesToRemove.Add(id);
                        continue;
                    }

                    var cutOffTime = DateTime.Now - TimeSpan.FromMinutes(plugin.Configuration.KeepDeathsForMinutes);
                    for (var i = 0; i < death.Count; i++)
                        if (death[i].TimeOfDeath > cutOffTime) {
                            death.RemoveRange(0, i);
                            break;
                        }
                }

                foreach (var entry in entriesToRemove)
                    plugin.DeathsPerPlayer.Remove(entry);
            } catch (Exception e) {
                PluginLog.LogError(e, "Error while clearing events");
            }
        }
    }
}
