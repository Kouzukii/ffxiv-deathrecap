using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using DeathRecap.Game;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Common.Math;
using Action = Lumina.Excel.Sheets.Action;
using Status = Lumina.Excel.Sheets.Status;

namespace DeathRecap.Events;

public class CombatEventCapture : IDisposable {
    private readonly Dictionary<ulong, List<CombatEvent>> combatEvents = new();
    private readonly DeathRecapPlugin plugin;

    private unsafe delegate void ProcessPacketActionEffectDelegate(
        uint casterEntityId, Character* casterPtr, Vector3* targetPos, ActionEffectHandler.Header* header, ActionEffectHandler.TargetEffects* effects,
        GameObjectId* targetEntityIds);

    private delegate void ProcessPacketActorControlDelegate(
        uint entityId, ActorControlCategory type, uint param1, uint param2, uint param3, uint param4, uint param5, uint param6, ulong param7, byte isReplay);

    private delegate void ProcessPacketEffectResultDelegate(uint targetId, IntPtr actionIntegrityData, byte isReplay);

    private readonly Hook<ProcessPacketActionEffectDelegate> processPacketActionEffectHook;

    [Signature("E8 ?? ?? ?? ?? 0F B7 0B 83 E9 64", DetourName = nameof(ProcessPacketActorControlDetour))]
    private readonly Hook<ProcessPacketActorControlDelegate> processPacketActorControlHook = null!;

    [Signature("48 8B C4 44 88 40 18 89 48 08", DetourName = nameof(ProcessPacketEffectResultDetour))]
    private readonly Hook<ProcessPacketEffectResultDelegate> processPacketEffectResultHook = null!;

    public unsafe CombatEventCapture(DeathRecapPlugin plugin) {
        this.plugin = plugin;

        Service.GameInteropProvider.InitializeFromAttributes(this);

        processPacketActionEffectHook =
            Service.GameInteropProvider.HookFromSignature<ProcessPacketActionEffectDelegate>(ActionEffectHandler.Addresses.Receive.String,
                ProcessPacketActionEffectDetour);
        processPacketActionEffectHook.Enable();
        processPacketActorControlHook.Enable();
        processPacketEffectResultHook.Enable();
    }

    private unsafe void ProcessPacketActionEffectDetour(
        uint casterEntityId, Character* casterPtr, Vector3* targetPos, ActionEffectHandler.Header* effectHeader, ActionEffectHandler.TargetEffects* effectArray,
        GameObjectId* targetEntityIds) {
        processPacketActionEffectHook.Original(casterEntityId, casterPtr, targetPos, effectHeader, effectArray, targetEntityIds);

        try {
            if (effectHeader->NumTargets == 0)
                return;

            var actionId = effectHeader->ActionType switch {
                ActionType.Mount => 0xD000000 + effectHeader->ActionId,
                ActionType.Item => 0x2000000 + effectHeader->ActionId,
                _ => effectHeader->SpellId
            };
            Action? action = null;
            string? source = null;
            List<uint>? additionalStatus = null;

            var casterObj = Service.ObjectTable.SearchById(casterEntityId);

            for (var i = 0; i < effectHeader->NumTargets; i++) {
                var actionTargetId = (uint)(targetEntityIds[i] & uint.MaxValue);
                //if (!plugin.ConditionEvaluator.ShouldCapture(actionTargetId))
                //    continue;
                if (Service.ObjectTable.SearchById(actionTargetId) is not IPlayerCharacter targetObj)
                    continue;
                for (var j = 0; j < 8; j++) {
                    ref var actionEffect = ref effectArray[i].Effects[j];
                    if (actionEffect.Type == 0)
                        continue;
                    uint amount = actionEffect.Value;
                    if ((actionEffect.Param4 & 0x40) == 0x40)
                        amount += (uint)actionEffect.Param3 << 16;

                    action ??= Service.DataManager.GetExcelSheet<Action>().GetRowOrDefault(actionId);
                    source ??= casterPtr->NameString;

                    IPlayerCharacter p = targetObj;
                    //check for reflected damage and healing onto the caster (lifesteal, thorns)
                    bool isReflected = (actionEffect.Param4 & 0x80) == 0x80;
                    if(isReflected) {
                        if (casterObj is not IPlayerCharacter) {
                            continue;
                        } else {
                            p = casterObj as IPlayerCharacter;
                        }
                    }

                    if (!plugin.ConditionEvaluator.ShouldCapture(p.EntityId))
                        continue;

                    switch ((ActionEffectType)actionEffect.Type) {
                        case ActionEffectType.Miss:
                        case ActionEffectType.Damage:
                        case ActionEffectType.BlockedDamage:
                        case ActionEffectType.ParriedDamage:
                            if (!isReflected && additionalStatus == null) {
                                var statusManager = casterPtr->GetStatusManager();
                                additionalStatus = [];
                                if (statusManager != null) {
                                    foreach (ref var status in statusManager->Status) {
                                        if (status.StatusId is 1203 or 1195 or 1193 or 860 or 1715 or 2115 or 3642)
                                            additionalStatus.Add(status.StatusId);
                                    }
                                }
                            }

                            combatEvents.AddEntry(p.EntityId,
                                new CombatEvent.DamageTaken {
                                    // 1203 = Addle
                                    // 1195 = Feint
                                    // 1193 = Reprisal
                                    //  860 = Dismantled
                                    // 1715 = Malodorous, BLU Bad Breath
                                    // 2115 = Conked, BLU Magic Hammer
                                    // 3642 = Candy Cane, BLU Candy Cane
                                    Snapshot = p.Snapshot(true, additionalStatus),
                                    Source = source,
                                    Amount = amount,
                                    Action = action?.ActionCategory.RowId == 1 ? "Auto-attack" : action?.Name.ExtractText() ?? "",
                                    Icon = action?.Icon,
                                    Crit = (actionEffect.Param0 & 0x20) == 0x20,
                                    DirectHit = (actionEffect.Param0 & 0x40) == 0x40,
                                    DamageType = (DamageType)(actionEffect.Param1 & 0xF),
                                    Parried = actionEffect.Type == (int)ActionEffectType.ParriedDamage,
                                    Blocked = actionEffect.Type == (int)ActionEffectType.BlockedDamage,
                                    DisplayType = effectHeader->ActionType
                                });
                            break;
                        case ActionEffectType.Heal:
                            combatEvents.AddEntry(p.EntityId,
                                new CombatEvent.Healed {
                                    Snapshot = p.Snapshot(true),
                                    Source = source,
                                    Amount = amount,
                                    Action = action?.Name.ExtractText() ?? "",
                                    Icon = action?.Icon,
                                    Crit = (actionEffect.Param1 & 0x20) == 0x20
                                });
                            break;
                    }
                }
            }
        } catch (Exception e) {
            Service.PluginLog.Error(e, "Caught unexpected exception");
        }
    }

    private void ProcessPacketActorControlDetour(
        uint entityId, ActorControlCategory type, uint param1, uint param2, uint param3, uint param4, uint param5, uint param6, ulong param7, byte isReplay) {
        processPacketActorControlHook.Original(entityId, type, param1, param2, param3, param4, param5, param6, param7, isReplay);

        try {
            if (!plugin.ConditionEvaluator.ShouldCapture(entityId))
                return;

            if (Service.ObjectTable.SearchById(entityId) is not IPlayerCharacter p)
                return;

            switch (type) {
                case ActorControlCategory.DoT:
                    if (param1 != 0) {
                        var sourceName = Service.ObjectTable.SearchById(param3)?.Name.TextValue;
                        var status = Service.DataManager.GetExcelSheet<Status>().GetRowOrDefault(param1);
                        combatEvents.AddEntry(entityId,
                            new CombatEvent.DamageTaken {
                                Snapshot = p.Snapshot(),
                                Source = sourceName,
                                Amount = param2,
                                Action = status?.Name.ExtractText() ?? "",
                                Icon = (status?.Icon),
                                Crit = param4 == 1,
                                DamageType = DamageType.Unknown, //not sure if status DoTs have a damage type
                                DisplayType = ActionType.Action, //not applicable
                            });
                    }
                    else {
                        combatEvents.AddEntry(entityId, new CombatEvent.DoT { Snapshot = p.Snapshot(), Amount = param2 });
                    }
                    break;
                case ActorControlCategory.HoT:
                    if (param1 != 0) {
                        var sourceName = Service.ObjectTable.SearchById(param3)?.Name.TextValue;
                        var status = Service.DataManager.GetExcelSheet<Status>().GetRowOrDefault(param1);
                        combatEvents.AddEntry(entityId,
                            new CombatEvent.Healed {
                                Snapshot = p.Snapshot(),
                                Source = sourceName,
                                Amount = param2,
                                Action = status?.Name.ExtractText() ?? "",
                                Icon = status?.Icon,
                                Crit = param4 == 1
                            });
                    } else {
                        combatEvents.AddEntry(entityId, new CombatEvent.HoT { Snapshot = p.Snapshot(), Amount = param2 });
                    }

                    break;
                case ActorControlCategory.MedKit:
                    combatEvents.AddEntry(entityId,
                    new CombatEvent.MedKit {
                        Snapshot = p.Snapshot(),
                        Amount = param2,
                    });
                    break;
                case ActorControlCategory.FallDamage:
                    combatEvents.AddEntry(entityId,
                    new CombatEvent.FallDamage {
                        Snapshot = p.Snapshot(),
                        Amount = param1,
                    });
                    break;
                case ActorControlCategory.Death: {
                    if (combatEvents.Remove(entityId, out var events)) {
                        var death = new Death { PlayerId = entityId, PlayerName = p.Name.TextValue, TimeOfDeath = DateTime.Now, Events = events };
                        plugin.DeathsPerPlayer.AddEntry(entityId, death);
                        plugin.NotificationHandler.DisplayDeath(death);
                    }

                    break;
                }
            }
        } catch (Exception e) {
            Service.PluginLog.Error(e, "Caught unexpected exception");
        }
    }

    private unsafe void ProcessPacketEffectResultDetour(uint targetId, IntPtr actionIntegrityData, byte isReplay) {
        processPacketEffectResultHook.Original(targetId, actionIntegrityData, isReplay);

        try {
            var message = (AddStatusEffect*)actionIntegrityData;
            if (!plugin.ConditionEvaluator.ShouldCapture(targetId))
                return;

            if (Service.ObjectTable.SearchById(targetId) is not IPlayerCharacter p)
                return;

            var effects = (StatusEffectAddEntry*)message->Effects;
            var effectCount = Math.Min(message->EffectCount, 4u);
            for (uint j = 0; j < effectCount; j++) {
                var effect = effects[j];
                var effectId = effect.EffectId;
                if (effectId <= 0)
                    continue;
                // negative durations will remove effect
                if (effect.Duration < 0)
                    continue;
                var source = Service.ObjectTable.SearchById(effect.SourceActorId)?.Name.TextValue;
                var status = Service.DataManager.GetExcelSheet<Status>().GetRowOrDefault(effectId);

                combatEvents.AddEntry(targetId,
                    new CombatEvent.StatusEffect {
                        Snapshot = p.Snapshot(),
                        Id = effectId,
                        StackCount = effect.StackCount <= status?.MaxStacks ? effect.StackCount : 0u,
                        Icon = status?.Icon,
                        Status = status?.Name.ExtractText(),
                        Description = status?.Description.ExtractText(),
                        Category = (StatusCategory)(status?.StatusCategory ?? 0),
                        Source = source,
                        Duration = effect.Duration
                    });
            }
        } catch (Exception e) {
            Service.PluginLog.Error(e, "Caught unexpected exception");
        }
    }

    public void CleanCombatEvents() {
        try {
            var entriesToRemove = new List<ulong>();
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
            Service.PluginLog.Error(e, "Error while clearing events");
        }
    }

    public void Dispose() {
        processPacketActionEffectHook.Dispose();
        processPacketEffectResultHook.Dispose();
        processPacketActorControlHook.Dispose();
    }
}
