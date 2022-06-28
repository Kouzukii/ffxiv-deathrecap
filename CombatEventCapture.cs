using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using Dalamud.Logging;
using DeathRecap.Messages;
using Lumina.Excel.GeneratedSheets;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace DeathRecap {
    public class CombatEventCapture : IDisposable {
        private readonly Dictionary<uint, List<CombatEvent>> combatEvents = new();
        private readonly DeathRecapPlugin plugin;

        private delegate void ReceiveAbilityDelegate(int sourceId, IntPtr sourceCharacter, IntPtr pos, IntPtr effectHeader, IntPtr effectArray,
            IntPtr effectTrail);

        private delegate void ReceiveActorControlSelfDelegate(uint entityId, uint id, uint arg0, uint arg1, uint arg2, uint arg3, uint arg4, uint arg5,
            ulong targetId, byte a10);

        private delegate void ActionIntegrityDelegate(uint targetId, IntPtr actionIntegrityData, bool isReplay);

        private readonly Hook<ReceiveAbilityDelegate> receiveAbilityEffectHook;
        private readonly Hook<ReceiveActorControlSelfDelegate> receiveActorControlSelfHook;
        private readonly Hook<ActionIntegrityDelegate> actionIntegrityDelegateHook;

        public CombatEventCapture(DeathRecapPlugin plugin) {
            this.plugin = plugin;

            receiveAbilityEffectHook = new Hook<ReceiveAbilityDelegate>(Service.SigScanner.ScanText("4C 89 44 24 ?? 55 56 57 41 54 41 55 41 56 48 8D 6C 24 ??"),
                ReceiveAbilityEffectDetour);
            receiveActorControlSelfHook =
                new Hook<ReceiveActorControlSelfDelegate>(Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 0F B7 0B 83 E9 64"), ReceiveActorControlSelfDetour);
            actionIntegrityDelegateHook = new Hook<ActionIntegrityDelegate>(
                Service.SigScanner.ScanText("48 8B C4 44 88 40 18 89 48 08"), ActionIntegrityDelegateDetour);
            receiveAbilityEffectHook.Enable();
            receiveActorControlSelfHook.Enable();
            actionIntegrityDelegateHook.Enable();
        }

        private unsafe void ReceiveAbilityEffectDetour(int sourceId, IntPtr sourceCharacter, IntPtr pos, IntPtr effectHeader, IntPtr effectArray,
            IntPtr effectTrail) {
            receiveAbilityEffectHook.Original(sourceId, sourceCharacter, pos, effectHeader, effectArray, effectTrail);

            try {
                var message = (ActionEffectHeader*)effectHeader;
                var targetIds = (ulong*)effectTrail;
                var effectData = (byte*)effectArray;
                uint targets = message->EffectCount;

                if (targets == 0)
                    return;

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
                            var amount = ((uint)effectData[actionIndex + 7] << 8) + effectData[actionIndex + 6];
                            if ((effectData[actionIndex + 5] & 0x40) == 0x40)
                                amount += (uint)effectData[actionIndex + 4] << 16;

                            var actionType = ConvertActionType(effectData[actionIndex]);
                            action ??= Service.DataManager.Excel.GetSheet<Action>()?.GetRow(actionId);
                            gameObject ??= Service.ObjectTable.SearchById((uint)sourceId);
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
                                            Action = action?.ActionCategory.Row == 1 ? "Auto-attack" : action?.Name?.RawString,
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
            } catch (Exception e) {
                PluginLog.Error(e, "Caught unexpected exception");
            }
        }

        private void ReceiveActorControlSelfDetour(uint entityId, uint type, uint buffId, uint direct, uint damage, uint sourceId, uint arg4, uint arg5,
            ulong targetId, byte a10) {
            receiveActorControlSelfHook.Original(entityId, type, buffId, direct, damage, sourceId, arg4, arg5, targetId, a10);

            try {
                if (!plugin.ConditionEvaluator.ShouldCapture(entityId))
                    return;

                if (Service.ObjectTable.SearchById(entityId) is not PlayerCharacter p)
                    return;

                switch ((ActorControlCategory)type) {
                    case ActorControlCategory.DoT:
                        combatEvents.AddEntry(entityId, new CombatEvent.DoT { Snapshot = p.Snapshot(), Amount = damage });
                        break;
                    case ActorControlCategory.HoT:
                        combatEvents.AddEntry(entityId, new CombatEvent.HoT { Snapshot = p.Snapshot(), Amount = direct });
                        break;
                    case ActorControlCategory.Death: {
                        if (combatEvents.Remove(entityId, out var events)) {
                            var death = new Death {
                                PlayerId = entityId,
                                PlayerName = p.Name.TextValue,
                                TimeOfDeath = DateTime.Now,
                                Events = events
                            };
                            plugin.DeathsPerPlayer.AddEntry(entityId, death);
                            plugin.NotificationHandler.DisplayDeath(death);
                        }

                        break;
                    }
                }
            } catch (Exception e) {
                PluginLog.Error(e, "Caught unexpected exception");
            }
        }

        private unsafe void ActionIntegrityDelegateDetour(uint targetId, IntPtr actionIntegrityData, bool isReplay) {
            actionIntegrityDelegateHook.Original(targetId, actionIntegrityData, isReplay);

            try {
                var message = (AddStatusEffect*)actionIntegrityData;
                if (!plugin.ConditionEvaluator.ShouldCapture(targetId))
                    return;

                if (Service.ObjectTable.SearchById(targetId) is not PlayerCharacter p)
                    return;

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

                    combatEvents.AddEntry(targetId, new CombatEvent.StatusEffect {
                        Snapshot = p.Snapshot(),
                        Id = effectId,
                        Icon = status?.Icon,
                        Status = status?.Name.RawString,
                        Description = status?.Description.DisplayedText(),
                        Source = source,
                        Duration = effect.Duration
                    });
                }
            } catch (Exception e) {
                PluginLog.Error(e, "Caught unexpected exception");
            }
        }

        private static ActionType ConvertActionType(byte ability) {
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

        public void Dispose() {
            receiveAbilityEffectHook.Disable();
            actionIntegrityDelegateHook.Disable();
            receiveActorControlSelfHook.Disable();
        }
    }
}
