using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using DeathRecap.Game;
using Lumina.Excel.Sheets;
using Action = Lumina.Excel.Sheets.Action;

namespace DeathRecap.Events;

public class CombatEventCapture : IDisposable {
    private readonly Dictionary<ulong, List<CombatEvent>> combatEvents = new();
    private readonly DeathRecapPlugin plugin;

    private unsafe delegate void ProcessPacketActionEffectDelegate(
        int sourceId, IntPtr sourceCharacter, IntPtr pos, ActionEffectHeader* effectHeader, ActionEffect* effectArray, ulong* effectTrail);

    private delegate void ProcessPacketActorControlDelegate(
        uint entityId, uint type, uint statusId, uint amount, uint a5, uint source, uint a7, uint a8, ulong a9, byte flag);

    private delegate void ProcessPacketEffectResultDelegate(uint targetId, IntPtr actionIntegrityData, bool isReplay);

    [Signature("40 55 56 57 41 54 41 55 41 56 48 8D AC 24 68 FF FF FF 48 81 EC 98 01 00 00", DetourName = nameof(ProcessPacketActionEffectDetour))]
    private readonly Hook<ProcessPacketActionEffectDelegate> processPacketActionEffectHook = null!;

    [Signature("E8 ?? ?? ?? ?? 0F B7 0B 83 E9 64", DetourName = nameof(ProcessPacketActorControlDetour))]
    private readonly Hook<ProcessPacketActorControlDelegate> processPacketActorControlHook = null!;

    [Signature("48 8B C4 44 88 40 18 89 48 08", DetourName = nameof(ProcessPacketEffectResultDetour))]
    private readonly Hook<ProcessPacketEffectResultDelegate> processPacketEffectResultHook = null!;

    public CombatEventCapture(DeathRecapPlugin plugin) {
        this.plugin = plugin;

        Service.GameInteropProvider.InitializeFromAttributes(this);

        processPacketActionEffectHook.Enable();
        processPacketActorControlHook.Enable();
        processPacketEffectResultHook.Enable();
    }

    private unsafe void ProcessPacketActionEffectDetour(
        int sourceId, IntPtr sourceCharacter, IntPtr pos, ActionEffectHeader* effectHeader, ActionEffect* effectArray, ulong* effectTrail) {
        processPacketActionEffectHook.Original(sourceId, sourceCharacter, pos, effectHeader, effectArray, effectTrail);

        try {
            uint targets = effectHeader->EffectCount;

            if (targets == 0)
                return;

            var actionId = effectHeader->EffectDisplayType switch {
                ActionEffectDisplayType.MountName => 0xD000000 + effectHeader->ActionId,
                ActionEffectDisplayType.ShowItemName => 0x2000000 + effectHeader->ActionId,
                _ => effectHeader->ActionAnimationId
            };
            Action? action = null;
            string? source = null;
            IGameObject? gameObject = null;
            List<uint>? additionalStatus = null;

            for (var i = 0; i < targets; i++) {
                var actionTargetId = (uint)(effectTrail[i] & uint.MaxValue);
                if (!plugin.ConditionEvaluator.ShouldCapture(actionTargetId))
                    continue;
                if (Service.ObjectTable.SearchById(actionTargetId) is not IPlayerCharacter p)
                    continue;
                for (var j = 0; j < 8; j++) {
                    ref var actionEffect = ref effectArray[i * 8 + j];
                    if (actionEffect.EffectType == 0)
                        continue;
                    uint amount = actionEffect.Value;
                    if ((actionEffect.Flags2 & 0x40) == 0x40)
                        amount += (uint)actionEffect.Flags1 << 16;

                    action ??= Service.DataManager.GetExcelSheet<Action>().GetRowOrDefault(actionId);
                    gameObject ??= Service.ObjectTable.SearchById((uint)sourceId);
                    source ??= gameObject?.Name.TextValue;

                    switch (actionEffect.EffectType) {
                        case ActionEffectType.Miss:
                        case ActionEffectType.Damage:
                        case ActionEffectType.BlockedDamage:
                        case ActionEffectType.ParriedDamage:
                            combatEvents.AddEntry(actionTargetId,
                                new CombatEvent.DamageTaken {
                                    // 1203 = Addle
                                    // 1195 = Feint
                                    // 1193 = Reprisal
                                    //  860 = Dismantled
                                    // 1715 = Malodorous, BLU Bad Breath
                                    // 2115 = Conked, BLU Magic Hammer
                                    // 3642 = Candy Cane, BLU Candy Cane
                                    Snapshot =
                                        p.Snapshot(true,
                                            additionalStatus ??= gameObject is IBattleChara b
                                                ? b.StatusList.Select(s => s.StatusId).Where(s => s is 1203 or 1195 or 1193 or 860 or 1715 or 2115 or 3642)
                                                    .ToList()
                                                : []),
                                    Source = source,
                                    Amount = amount,
                                    Action = action?.ActionCategory.RowId == 1 ? "Auto-attack" : action?.Name.ExtractText() ?? "",
                                    Icon = action?.Icon,
                                    Crit = (actionEffect.Param0 & 0x20) == 0x20,
                                    DirectHit = (actionEffect.Param0 & 0x40) == 0x40,
                                    DamageType = (DamageType)(actionEffect.Param1 & 0xF),
                                    Parried = actionEffect.EffectType == ActionEffectType.ParriedDamage,
                                    Blocked = actionEffect.EffectType == ActionEffectType.BlockedDamage,
                                    DisplayType = effectHeader->EffectDisplayType
                                });
                            break;
                        case ActionEffectType.Heal:
                            combatEvents.AddEntry(actionTargetId,
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
        uint entityId, uint type, uint statusId, uint amount, uint a5, uint source, uint a7, uint a8, ulong a9, byte flag) {
        processPacketActorControlHook.Original(entityId, type, statusId, amount, a5, source, a7, a8, a9, flag);

        try {
            if (!plugin.ConditionEvaluator.ShouldCapture(entityId))
                return;

            if (Service.ObjectTable.SearchById(entityId) is not IPlayerCharacter p)
                return;

            switch ((ActorControlCategory)type) {
                case ActorControlCategory.DoT:
                    combatEvents.AddEntry(entityId, new CombatEvent.DoT { Snapshot = p.Snapshot(), Amount = amount });
                    break;
                case ActorControlCategory.HoT:
                    if (statusId != 0) {
                        var sourceName = Service.ObjectTable.SearchById(entityId)?.Name.TextValue;
                        var status = Service.DataManager.GetExcelSheet<Status>().GetRowOrDefault(statusId);
                        combatEvents.AddEntry(entityId,
                            new CombatEvent.Healed {
                                Snapshot = p.Snapshot(),
                                Source = sourceName,
                                Amount = amount,
                                Action = status?.Name.ExtractText() ?? "",
                                Icon = status?.Icon,
                                Crit = source == 1
                            });
                    } else {
                        combatEvents.AddEntry(entityId, new CombatEvent.HoT { Snapshot = p.Snapshot(), Amount = amount });
                    }

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

    private unsafe void ProcessPacketEffectResultDetour(uint targetId, IntPtr actionIntegrityData, bool isReplay) {
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
