using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Network;
using Dalamud.Logging;
using Dalamud.Plugin;
using DeathRecap.Messages;
using Lumina.Excel.GeneratedSheets;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace DeathRecap {
    public class DeathRecapPlugin : IDalamudPlugin {
        public string Name => "DeathRecap";

        internal DalamudPluginInterface PluginInterface { get; }
        internal CommandManager CommandManager { get; }
        internal GameNetwork GameNetwork { get; }
        internal DataManager DataManager { get; }
        internal ChatGui ChatGui { get; }
        internal ClientState ClientState { get; }
        internal ObjectTable ObjectTable { get; }

        public DeathRecapWindow Window { get; }
        public DateTime? LastDeath { get; internal set; }

        private List<CombatEvent> combatEvents = new();

        public List<List<CombatEvent>> Deaths { get; } = new();

        public DeathRecapPlugin(DalamudPluginInterface pluginInterface, CommandManager commandManager, GameNetwork gameNetwork, DataManager dataManager,
            ChatGui chatGui, ClientState clientState, ObjectTable objectTable) {
            PluginInterface = pluginInterface;
            CommandManager = commandManager;
            GameNetwork = gameNetwork;
            DataManager = dataManager;
            ChatGui = chatGui;
            ClientState = clientState;
            ObjectTable = objectTable;

            Window = new DeathRecapWindow(this);

            pluginInterface.UiBuilder.Draw += UiBuilderOnDraw;
            gameNetwork.NetworkMessage += GameNetworkOnNetworkMessage;

            var commandInfo = new CommandInfo((_, _) => Window.ShowDeathRecap = true) { HelpMessage = "Show the last death recap" };
            CommandManager.AddHandler("/deathrecap", commandInfo);
            CommandManager.AddHandler("/dr", commandInfo);
        }

        private void UiBuilderOnDraw() {
            CleanCombatEvents();
            Window.Draw();
        }

        private unsafe void GameNetworkOnNetworkMessage(IntPtr dataptr, ushort opcode, uint placeholder, uint targetactorid,
            NetworkMessageDirection direction) {
            try {
                switch ((Opcodes)opcode) {
                    case Opcodes.ActorControl: {
                        if (targetactorid != ObjectTable[0]?.ObjectId) return;
                        var actorCtrl = (ActorControl142*)dataptr;
                        if (actorCtrl->Category == ActorControlCategory.HoTDoT) {
                            if (actorCtrl->Param2 == 4) {
                                combatEvents.Add(new CombatEvent.HoT { Snapshot = CreateSnapshot(), Amount = actorCtrl->Param3, Id = actorCtrl->Param1 });
                            } else if (actorCtrl->Param2 == 3) {
                                combatEvents.Add(new CombatEvent.DoT { Snapshot = CreateSnapshot(), Amount = actorCtrl->Param3, Id = actorCtrl->Param1 });
                            }
                        } else if (actorCtrl->Category == ActorControlCategory.Death) {
                            var death = combatEvents;
                            combatEvents = new List<CombatEvent>();
                            death.Add(new CombatEvent.Death { Snapshot = CreateSnapshot() });
                            Deaths.Add(death);
                            LastDeath = DateTime.Now;
                        }

                        break;
                    }
                    case Opcodes.EffectResult: {
                        if (targetactorid != ObjectTable[0]?.ObjectId) return;
                        var message = (AddStatusEffect*)dataptr;
                        var effects = (StatusEffectAddEntry*)message->Effects;
                        var effectCount = Math.Min(message->EffectCount, 4u);
                        for (uint j = 0; j < effectCount; j++) {
                            var effect = effects[j];
                            var effectId = effect.EffectId;
                            if (effectId <= 0) continue;
                            var source = ObjectTable.SearchById(effect.SourceActorId)?.Name.TextValue;
                            var status = DataManager.Excel.GetSheet<Status>()?.GetRow(effectId);

                            combatEvents.Add(new CombatEvent.StatusEffect {
                                Snapshot = CreateSnapshot(),
                                Id = effectId,
                                Icon = status?.Icon,
                                Status = status?.Name.RawString,
                                Description = status?.Description.DisplayedText(),
                                Source = source,
                                Duration = effect.Duration
                            });
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

                        if (targetIds == null || effectData == null || targets == 0) break;

                        uint actionId = message->EffectDisplayType switch {
                            ActionEffectDisplayType.MountName => 218103808 + message->ActionId,
                            ActionEffectDisplayType.ShowItemName => 33554432 + message->ActionId,
                            _ => message->ActionAnimationId
                        };
                        Action? action = null;
                        string? source = null;

                        for (var i = 0; i < targets; i++) {
                            uint actionTargetId = (uint)(targetIds[i] & uint.MaxValue);
                            if (actionTargetId != ObjectTable[0]?.ObjectId) continue;
                            for (var j = 0; j < 8; j++) {
                                var actionIndex = i * 64 + j * 8;
                                if (effectData[actionIndex] == 0) continue;
                                var amount = (effectData[actionIndex + 7] << 8) + effectData[actionIndex + 6];
                                if ((effectData[actionIndex + 5] & 0x40) == 0x40) {
                                    amount += effectData[actionIndex + 4] << 16;
                                }

                                var actionType = ConvertActionType(effectData[actionIndex]);
                                action ??= DataManager.Excel.GetSheet<Action>()?.GetRow(actionId);
                                source ??= ObjectTable.SearchById(targetactorid)?.Name.TextValue;

                                switch (actionType) {
                                    case ActionType.Ability:
                                    case ActionType.AbilityBlocked:
                                    case ActionType.AbilityParried:
                                        combatEvents.Add(new CombatEvent.DamageTaken {
                                            Snapshot = CreateSnapshot(true),
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
                                        combatEvents.Add(new CombatEvent.Healed {
                                            Snapshot = CreateSnapshot(true),
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

        private void CleanCombatEvents() {
            if (combatEvents.Count > 500) {
                combatEvents.RemoveRange(0, combatEvents.Count - 500);
                while ((combatEvents[0].Snapshot.Time - DateTime.Now).TotalSeconds > 60) {
                    combatEvents.RemoveAt(0);
                }
            }

            while (Deaths.Count > 10) {
                Deaths.RemoveAt(0);
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

        private CombatEvent.EventSnapshot CreateSnapshot(bool snapEffects = false) {
            var localPlayer = ClientState.LocalPlayer;
            var snapshot = new CombatEvent.EventSnapshot {
                Time = DateTime.Now,
                CurrentHp = localPlayer?.CurrentHp,
                MaxHp = localPlayer?.MaxHp,
                StatusEffects = snapEffects ? localPlayer?.StatusList?.Select(s => s.StatusId).ToList() : null,
                BarrierFraction = PlayerBarrier(localPlayer)
            };
            return snapshot;
        }

        private unsafe byte? PlayerBarrier(PlayerCharacter? player) => *(byte*)(player?.Address + 0x19D9);

        public void Dispose() {
            GameNetwork.NetworkMessage -= GameNetworkOnNetworkMessage;
            CommandManager.RemoveHandler("/deathrecap");
            CommandManager.RemoveHandler("/dr");
        }
    }
}