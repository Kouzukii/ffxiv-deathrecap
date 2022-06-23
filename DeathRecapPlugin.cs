using System;
using System.Collections.Generic;
using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Hooking;
using Dalamud.Plugin;
using DeathRecap.Messages;

namespace DeathRecap {
    public class DeathRecapPlugin : IDalamudPlugin {
        public string Name => "DeathRecap";
        public DeathRecapWindow Window { get; }

        public ConfigWindow ConfigWindow { get; }

        public Configuration Configuration { get; }

        public ConditionEvaluator ConditionEvaluator { get; }

        public CombatEventCapture CombatEventCapture { get; }

        public NotificationHandler NotificationHandler { get; }

        public Dictionary<uint, List<Death>> DeathsPerPlayer { get; } = new();

        private DateTime lastClean = DateTime.Now;
        public delegate void ReceiveAbilityDelegate(int sourceId, IntPtr sourceCharacter, IntPtr pos,
    IntPtr effectHeader, IntPtr effectArray, IntPtr effectTrail);
        public Hook<ReceiveAbilityDelegate> ReceiveAbilityHook;
        public delegate void ActorControlSelfDelegate(uint entityId, uint id, uint arg0, uint arg1, uint arg2,
           uint arg3, uint arg4, uint arg5, ulong targetId, byte a10);
        public Hook<ActorControlSelfDelegate> ActorControlSelfHook;
        public delegate void ActionIntegrityDelegate(uint targetId, IntPtr _ActionIntegrityData, bool isReplay);
        public Hook<ActionIntegrityDelegate> ActionIntegrityDelegateHook;
        public DeathRecapPlugin(DalamudPluginInterface pluginInterface) {
            Service.Initialize(pluginInterface);

            Configuration = Configuration.Get(pluginInterface);
            Window = new DeathRecapWindow(this);
            ConfigWindow = new ConfigWindow(this);
            ConditionEvaluator = new ConditionEvaluator(this);
            CombatEventCapture = new CombatEventCapture(this);
            NotificationHandler = new NotificationHandler(this);

            pluginInterface.UiBuilder.Draw += UiBuilderOnDraw;
            //Service.GameNetwork.NetworkMessage += CombatEventCapture.GameNetworkOnNetworkMessage;
            Service.Framework.Update += FrameworkOnUpdate;
            ReceiveAbilityHook = new Hook<ReceiveAbilityDelegate>(
        Service.SigScanner.ScanText("4C 89 44 24 ?? 55 56 57 41 54 41 55 41 56 48 8D 6C 24 ??"),
        CombatEventCapture.ReceiveAbilityEffect);
            ReceiveAbilityHook.Enable();
            ActorControlSelfHook = new Hook<ActorControlSelfDelegate>(
                    Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 0F B7 0B 83 E9 64"), CombatEventCapture.ReceiveActorControlSelf);
            ActorControlSelfHook.Enable();
            ActionIntegrityDelegateHook = new Hook<ActionIntegrityDelegate>(
                   Service.SigScanner.ScanText("48 8B C4 44 88 40 18 89 48 08"), CombatEventCapture.ActionIntegrityDelegateDE);
            ActionIntegrityDelegateHook.Enable();
            var commandInfo = new CommandInfo((_, _) => Window.ShowDeathRecap = true) { HelpMessage = "Open the death recap window" };
            Service.CommandManager.AddHandler("/deathrecap", commandInfo);
            Service.CommandManager.AddHandler("/dr", commandInfo);

#if DEBUG
            AddDummyData();
#endif
        }

        private void FrameworkOnUpdate(Framework framework) {
            var now = DateTime.Now;
            if ((now - lastClean).TotalSeconds >= 10) {
                CombatEventCapture.CleanCombatEvents();
                lastClean = now;
            }
        }

        private void UiBuilderOnDraw() {
            Window.Draw();
            ConfigWindow.Draw();
            NotificationHandler.Draw();
        }

        public void Dispose() {
            //Service.GameNetwork.NetworkMessage -= CombatEventCapture.GameNetworkOnNetworkMessage;
            ReceiveAbilityHook.Disable();
            ActionIntegrityDelegateHook.Disable();
            ActorControlSelfHook.Disable();
            Service.Framework.Update -= FrameworkOnUpdate;
            Service.CommandManager.RemoveHandler("/deathrecap");
            Service.CommandManager.RemoveHandler("/dr");
        }

#if DEBUG
        private void AddDummyData() {
            DeathsPerPlayer.Add(0, new List<Death> {
                new Death {
                    PlayerId = 0,
                    PlayerName = "Testing",
                    TimeOfDeath = DateTime.Now,
                    Events = new List<CombatEvent> {
                        new CombatEvent.Healed {
                            Action = "Heal",
                            Amount = 1234,
                            Crit = true,
                            Icon = 406,
                            Source = "White Mage",
                            Snapshot = new CombatEvent.EventSnapshot {
                                Time = DateTime.Now,
                                CurrentHp = 5000,
                                MaxHp = 10000,
                                StatusEffects = new List<uint> { 2, 3 }
                            }
                        },
                        new CombatEvent.DamageTaken {
                            Action = "Deal Damage",
                            Amount = 9001,
                            Crit = true,
                            Parried = true,
                            Icon = 61108,
                            Source = "Boss",
                            DamageType = DamageType.Slashing,
                            DisplayType = ActionEffectDisplayType.ShowActionName,
                            Snapshot = new CombatEvent.EventSnapshot {
                                Time = DateTime.Now,
                                BarrierPercent = 10,
                                CurrentHp = 35000,
                                MaxHp = 100000,
                                StatusEffects = new List<uint> { 2, 3 }
                            }
                        },
                        new CombatEvent.DamageTaken {
                            Action = "Deal Magic Damage",
                            Amount = 15327,
                            DirectHit = true,
                            Icon = 3202,
                            Source = "Boss",
                            DamageType = DamageType.Magic,
                            DisplayType = ActionEffectDisplayType.ShowActionName,
                            Snapshot = new CombatEvent.EventSnapshot {
                                Time = DateTime.Now,
                                BarrierPercent = 30,
                                CurrentHp = 37000,
                                MaxHp = 100000,
                                StatusEffects = new List<uint> { 2, 3 }
                            }
                        }
                    }
                }
            });
            NotificationHandler.DisplayDeath(DeathsPerPlayer[0][0]);
        }
#endif
    }
}
