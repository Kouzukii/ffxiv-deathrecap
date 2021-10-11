using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Network;
using Dalamud.Interface;
using Dalamud.Logging;
using Dalamud.Plugin;
using DeathRecap.Messages;
using ImGuiNET;
using ImGuiScene;
using Lumina.Excel.GeneratedSheets;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace DeathRecap {
    public class DeathRecapPlugin : IDalamudPlugin {
        public string Name => "DeathRecap";

        public DalamudPluginInterface PluginInterface { get; }
        public CommandManager CommandManager { get; }
        public GameNetwork GameNetwork { get; }
        public DataManager DataManager { get; }
        public ChatGui ChatGui { get; }
        public ClientState ClientState { get; }
        public ObjectTable ObjectTable { get; }

        private DateTime? lastDeath;
        private bool showDeathRecap;
        private int selectedDeath;

        private List<CombatEvent> combatEvents = new();
        private List<List<CombatEvent>> deaths = new();
        private Dictionary<ushort, TextureWrap> textures = new();

        public DeathRecapPlugin(DalamudPluginInterface pluginInterface, CommandManager commandManager, GameNetwork gameNetwork, DataManager dataManager, ChatGui chatGui, ClientState clientState, ObjectTable objectTable) {
            PluginInterface = pluginInterface;
            CommandManager = commandManager;
            GameNetwork = gameNetwork;
            DataManager = dataManager;
            ChatGui = chatGui;
            ClientState = clientState;
            ObjectTable = objectTable;

            pluginInterface.UiBuilder.Draw += UiBuilderOnDraw;
            pluginInterface.UiBuilder.OpenConfigUi += OpenConfigUI;
            gameNetwork.NetworkMessage += GameNetworkOnNetworkMessage;

            CommandManager.AddHandler("/deathrecap", new CommandInfo((_, _) => OpenConfigUI()) { HelpMessage = "Death Recap Configuration Menu" });
        }

        private void UiBuilderOnDraw() {
            try {
                CleanCombatEvents();
                bool bShowDeathRecap = (showDeathRecap || (DateTime.Now - lastDeath)?.TotalSeconds < 30) && deaths.Count > 0;
                ImGui.SetNextWindowSize(new Vector2(400, 400), ImGuiCond.FirstUseEver);
                if (bShowDeathRecap && ImGui.Begin("Death Recap", ref bShowDeathRecap, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoFocusOnAppearing)) {
                    if (ImGui.IsWindowFocused()) showDeathRecap = true;
                    var deathEvents = deaths[deaths.Count - 1 - selectedDeath];
                    var deathTime = DateTime.Now;

                    void PrintTime(CombatEvent e) {
                        var text = $"{(e.Snapshot.Time - deathTime).TotalSeconds:N1}s:";
                        var textSize = ImGui.CalcTextSize(text);
                        textSize.X = 30 - textSize.X;
                        ImGui.Dummy(textSize);
                        ImGui.PushStyleColor(ImGuiCol.Text, 0xFF808080);
                        ImGui.SameLine();
                        ImGui.Text(text);
                        ImGui.PopStyleColor();
                        ImGui.SameLine();
                    }

                    void PrintHp(CombatEvent e) {
                        ImGui.SameLine();
                        ImGui.Spacing();
                        ImGui.PushStyleColor(ImGuiCol.Text, 0xFF99fad1);
                        ImGui.SameLine();
                        ImGui.Text($"HP: {e.Snapshot.CurrentHp:N0}");
                        ImGui.PopStyleColor();
                    }

                    void PrintStatusEffects(CombatEvent e) {
                        if (e.Snapshot.StatusEffects != null) {
                            foreach (var effect in e.Snapshot.StatusEffects) {
                                if (DataManager.GetExcelSheet<Status>()?.GetRow(effect) is { } s) {
                                    var img = GetIconImage(s.Icon);
                                    if (img != null) {
                                        ImGui.SameLine();
                                        ImGui.Image(img.ImGuiHandle, new Vector2(16, 16));
                                        if (ImGui.IsItemHovered()) {
                                            ImGui.BeginTooltip();
                                            ImGui.Text(s.Name);
                                            ImGui.EndTooltip();
                                        }
                                    }
                                }
                            }
                        }
                    }

                    for (var i = deathEvents.Count - 1; i >= 0; i--) {
                        switch (deathEvents[i]) {
                            case CombatEvent.Death death:
                                deathTime = death.Snapshot.Time;
                                PrintTime(death);
                                ImGui.Text("Death");
                                break;
                            case CombatEvent.HoT hot:
                                PrintTime(hot);
                                ImGui.Text($"Received HoT");
                                var total = hot.Amount;
                                while (i > 0 && deathEvents[i - 1] is CombatEvent.HoT h) {
                                    hot = h;
                                    total += h.Amount;
                                    i--;
                                }

                                ImGui.PushStyleColor(ImGuiCol.Text, 0xFF00FF00);
                                ImGui.SameLine();
                                ImGui.Text($"+{total:N0}");
                                ImGui.PopStyleColor();
                                PrintHp(hot);
                                break;
                            case CombatEvent.DoT dot:
                                PrintTime(dot);
                                ImGui.Text($"Received dot damage");
                                ImGui.PushStyleColor(ImGuiCol.Text, 0xFF6680e6);
                                ImGui.SameLine();
                                ImGui.Text($"{dot.Amount:N0}");
                                ImGui.PopStyleColor();
                                PrintHp(dot);
                                break;
                            case CombatEvent.DamageTaken dt: {
                                PrintTime(dt);
                                ImGui.Text($"Hit for");
                                if (dt.DamageType == DamageType.Magic) {
                                    ImGui.PushStyleColor(ImGuiCol.Text, 0xffbe9925);
                                } else {
                                    ImGui.PushStyleColor(ImGuiCol.Text, 0xff4aa3ff);
                                }

                                ImGui.SameLine();
                                ImGui.Text($"-{dt.Amount:N0}{(dt.Crit ? dt.DirectHit ? "!!" : "!" : "")}");
                                ImGui.PopStyleColor();
                                ImGui.SameLine();
                                ImGui.Text("by");
                                var img = GetIconImage(dt.Icon);
                                if (img != null) {
                                    ImGui.SameLine();
                                    ImGui.Image(img.ImGuiHandle, new Vector2(16, 16));
                                }
                                ImGui.SameLine();
                                ImGui.Text($"{dt.Action} from {dt.Source}");
                                PrintHp(dt);
                                PrintStatusEffects(dt);
                                break;
                            }
                            case CombatEvent.Healed h: {
                                PrintTime(h);
                                ImGui.Text($"Received healing");
                                ImGui.PushStyleColor(ImGuiCol.Text, 0xFF00FF00);
                                ImGui.SameLine();
                                ImGui.Text($"+{h.Amount:N0}");
                                ImGui.PopStyleColor();
                                ImGui.SameLine();
                                ImGui.Text($"from");
                                var img = GetIconImage(h.Icon);
                                if (img != null) {
                                    ImGui.SameLine();
                                    ImGui.Image(img.ImGuiHandle, new Vector2(16, 16));
                                }

                                ImGui.SameLine();
                                ImGui.Text($"{h.Action} by {h.Source}");
                                PrintHp(h);
                                PrintStatusEffects(h);
                                break;
                            }
                            case CombatEvent.StatusEffect s: {
                                PrintTime(s);
                                ImGui.Text($"Received");
                                var img = GetIconImage(s.Icon);
                                if (img != null) {
                                    ImGui.SameLine();
                                    ImGui.Image(img.ImGuiHandle, new Vector2(16, 16));
                                }

                                ImGui.SameLine();
                                ImGui.Text($"{s.Status} ({s.Duration:N0}s) from {s.Source}");
                                break;
                            }
                        }

                        ImGui.Separator();
                    }

                    ImGui.End();

                    if (!bShowDeathRecap) {
                        showDeathRecap = false;
                        lastDeath = null;
                    }
                }
            } catch (Exception e) {
                PluginLog.Error(e, "Failed to draw window");
            }
        }

        private unsafe void GameNetworkOnNetworkMessage(IntPtr dataptr, ushort opcode, uint placeholder, uint targetactorid,
            NetworkMessageDirection direction) {
            try {
                switch ((Opcodes)opcode) {
                    case Opcodes.ActorControl142: {
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
                            deaths.Add(death);
                            lastDeath = DateTime.Now;
                        }

                        break;
                    }
                    case Opcodes.AddStatusEffect: {
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
                                            Blocked = actionType == ActionType.AbilityBlocked
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
            while (combatEvents.Count > 500) {
                if ((combatEvents[0].Snapshot.Time - DateTime.Now).TotalSeconds > 60) {
                    combatEvents.RemoveAt(0);
                } else {
                    break;
                }
            }

            while (deaths.Count > 10) {
                deaths.RemoveAt(0);
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
                StatusEffects = snapEffects ? localPlayer?.StatusList?.Select(s => s.StatusId).ToList() : null
            };
            // barrier is not instantly applied..
            //Task.Run(async () => {
            //    await Task.Delay(2);
            //    snapshot.Barrier = PlayerBarrier;
            //});
            return snapshot;
        }

        private TextureWrap? GetIconImage(ushort? icon) {
            if (icon is { } u) {
                if (textures.TryGetValue(u, out var tex))
                    return tex;
                if (DataManager.GetImGuiTextureIcon(u) is { } t)
                    return textures[u] = t;
            }

            return null;
        }

        private unsafe uint? PlayerBarrier => (uint?)((ulong?)ClientState.LocalPlayer?.MaxHp * *(byte*)(ClientState.LocalPlayer?.Address + 0x1997) / 100);

        private void OpenConfigUI() {
            showDeathRecap = true;
        }

        public void Dispose() {
            GameNetwork.NetworkMessage -= GameNetworkOnNetworkMessage;
            CommandManager.RemoveHandler("/deathrecap");
            PluginInterface.Dispose();
        }
    }
}