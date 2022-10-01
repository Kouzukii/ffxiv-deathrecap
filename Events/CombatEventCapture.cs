using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using DeathRecap.Game;
using Lumina.Excel.GeneratedSheets;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace DeathRecap.Events;

public class CombatEventCapture : IDisposable {
    private readonly Dictionary<uint, List<CombatEvent>> combatEvents = new();
    private readonly DeathRecapPlugin plugin;

    private delegate void ReceiveAbilityDelegate(int sourceId, IntPtr sourceCharacter, IntPtr pos, IntPtr effectHeader, IntPtr effectArray, IntPtr effectTrail);

    private delegate void ReceiveActorControlSelfDelegate(
        uint entityId, uint id, uint arg0, uint arg1, uint arg2, uint arg3, uint arg4, uint arg5,
        ulong targetId, byte a10);

    private delegate void ActionIntegrityDelegate(uint targetId, IntPtr actionIntegrityData, bool isReplay);

    // ffxiv_dx11.exe+98C490 - 4C 89 44 24 18        - mov [rsp+18],r8
    // ffxiv_dx11.exe+98C495 - 55                    - push rbp
    // ffxiv_dx11.exe+98C496 - 56                    - push rsi
    // ffxiv_dx11.exe+98C497 - 57                    - push rdi
    // ffxiv_dx11.exe+98C498 - 41 54                 - push r12
    // ffxiv_dx11.exe+98C49A - 41 55                 - push r13
    // ffxiv_dx11.exe+98C49C - 41 56                 - push r14
    // ffxiv_dx11.exe+98C49E - 48 8D 6C 24 E1        - lea rbp,[rsp-1F]
    // ffxiv_dx11.exe+98C4A3 - 48 81 EC D8000000     - sub rsp,000000D8 { 216 }
    // ffxiv_dx11.exe+98C4AA - 44 8B F1              - mov r14d,ecx
    // ffxiv_dx11.exe+98C4AD - 4D 8B E1              - mov r12,r9
    // ffxiv_dx11.exe+98C4B0 - 41 0FB7 49 1C         - movzx ecx,word ptr [r9+1C]
    // ffxiv_dx11.exe+98C4B5 - 49 8B F0              - mov rsi,r8
    // ffxiv_dx11.exe+98C4B8 - 4C 8B EA              - mov r13,rdx
    // ffxiv_dx11.exe+98C4BB - E8 2001D2FF           - call ffxiv_dx11.exe+6AC5E0
    // ffxiv_dx11.exe+98C4C0 - 48 8B F8              - mov rdi,rax
    // ffxiv_dx11.exe+98C4C3 - 48 85 C0              - test rax,rax
    // ffxiv_dx11.exe+98C4C6 - 0F84 DC040000         - je ffxiv_dx11.exe+98C9A8
    // ffxiv_dx11.exe+98C4CC - 66 41 83 7C 24 18 00  - cmp word ptr [r12+18],00 { 0 }
    // ffxiv_dx11.exe+98C4D3 - 48 89 9C 24 10010000  - mov [rsp+00000110],rbx
    // ffxiv_dx11.exe+98C4DB - 0F85 52010000         - jne ffxiv_dx11.exe+98C633
    // ffxiv_dx11.exe+98C4E1 - 80 78 2A 85           - cmp byte ptr [rax+2A],-7B { 133 }
    // ffxiv_dx11.exe+98C4E5 - 0F85 48010000         - jne ffxiv_dx11.exe+98C633
    // ffxiv_dx11.exe+98C4EB - 66 83 78 1E 00        - cmp word ptr [rax+1E],00 { 0 }
    // ffxiv_dx11.exe+98C4F0 - 0F84 1D010000         - je ffxiv_dx11.exe+98C613
    // ffxiv_dx11.exe+98C4F6 - F3 0F10 05 226C5B01   - movss xmm0,[ffxiv_dx11.exe+1F43120] { (0.00) }
    // ffxiv_dx11.exe+98C4FE - 48 8D 05 132DE900     - lea rax,[ffxiv_dx11.exe+181F218]
    // ffxiv_dx11.exe+98C505 - F3 0F10 0D 176C5B01   - movss xmm1,[ffxiv_dx11.exe+1F43124] { (0.00) }
    [Signature("4C 89 44 24 ?? 55 56 41 54 41 55 41 56 41 57 48 8D 6C 24 ??", DetourName = nameof(ReceiveAbilityEffectDetour))]
    private readonly Hook<ReceiveAbilityDelegate> receiveAbilityEffectHook = null!;

    // ffxiv_dx11.exe+6C3ED9 - E8 526B0600           - call ffxiv_dx11.exe+72AA30
    // ffxiv_dx11.exe+6C3EDE - 0FB7 0B               - movzx ecx,word ptr [rbx]
    // ffxiv_dx11.exe+6C3EE1 - 83 E9 64              - sub ecx,64 { 100 }
    // ffxiv_dx11.exe+6C3EE4 - 0F84 F3000000         - je ffxiv_dx11.exe+6C3FDD
    // ffxiv_dx11.exe+6C3EEA - 83 E9 01              - sub ecx,01 { 1 }
    // ffxiv_dx11.exe+6C3EED - 0F84 C9000000         - je ffxiv_dx11.exe+6C3FBC
    // ffxiv_dx11.exe+6C3EF3 - 83 E9 08              - sub ecx,08 { 8 }
    // ffxiv_dx11.exe+6C3EF6 - 74 29                 - je ffxiv_dx11.exe+6C3F21
    // ffxiv_dx11.exe+6C3EF8 - 81 F9 39030000        - cmp ecx,00000339 { 825 }
    // ffxiv_dx11.exe+6C3EFE - 0F85 19010000         - jne ffxiv_dx11.exe+6C401D
    // ffxiv_dx11.exe+6C3F04 - 0FB7 43 04            - movzx eax,word ptr [rbx+04]
    // ffxiv_dx11.exe+6C3F08 - 66 89 86 10070000     - mov [rsi+00000710],ax
    // ffxiv_dx11.exe+6C3F0F - B0 01                 - mov al,01 { 1 }
    // ffxiv_dx11.exe+6C3F11 - 48 8B 5C 24 60        - mov rbx,[rsp+60]
    // ffxiv_dx11.exe+6C3F16 - 48 8B 74 24 68        - mov rsi,[rsp+68]
    // ffxiv_dx11.exe+6C3F1B - 48 83 C4 50           - add rsp,50 { 80 }
    // ffxiv_dx11.exe+6C3F1F - 5F                    - pop rdi
    // ffxiv_dx11.exe+6C3F20 - C3                    - ret 
    // ffxiv_dx11.exe+6C3F21 - 8B 86 28060000        - mov eax,[rsi+00000628]
    // ffxiv_dx11.exe+6C3F27 - 85 C0                 - test eax,eax
    // ffxiv_dx11.exe+6C3F29 - 0F84 EE000000         - je ffxiv_dx11.exe+6C401D
    // ffxiv_dx11.exe+6C3F2F - 3B 43 04              - cmp eax,[rbx+04]
    // ffxiv_dx11.exe+6C3F32 - 0F85 E5000000         - jne ffxiv_dx11.exe+6C401D
    // ffxiv_dx11.exe+6C3F38 - C1 E8 10              - shr eax,10 { 16 }
    [Signature("E8 ?? ?? ?? ?? 0F B7 0B 83 E9 64", DetourName = nameof(ReceiveActorControlSelfDetour))]
    private readonly Hook<ReceiveActorControlSelfDelegate> receiveActorControlSelfHook = null!;

    // ffxiv_dx11.exe+7290F0 - 48 8B C4              - mov rax,rsp
    // ffxiv_dx11.exe+7290F3 - 44 88 40 18           - mov [rax+18],r8l
    // ffxiv_dx11.exe+7290F7 - 89 48 08              - mov [rax+08],ecx
    // ffxiv_dx11.exe+7290FA - 53                    - push rbx
    // ffxiv_dx11.exe+7290FB - 55                    - push rbp
    // ffxiv_dx11.exe+7290FC - 57                    - push rdi
    // ffxiv_dx11.exe+7290FD - 48 81 EC 80000000     - sub rsp,00000080 { 128 }
    // ffxiv_dx11.exe+729104 - 83 3D B9568301 03     - cmp dword ptr [ffxiv_dx11.exe+1F5E7C4],03 { (4),3 }
    // ffxiv_dx11.exe+72910B - 41 0FB6 D8            - movzx ebx,r8l
    // ffxiv_dx11.exe+72910F - 48 8B EA              - mov rbp,rdx
    // ffxiv_dx11.exe+729112 - 8B F9                 - mov edi,ecx
    // ffxiv_dx11.exe+729114 - 0F84 C1030000         - je ffxiv_dx11.exe+7294DB
    // ffxiv_dx11.exe+72911A - 48 89 70 10           - mov [rax+10],rsi
    // ffxiv_dx11.exe+72911E - 4C 89 60 E0           - mov [rax-20],r12
    // ffxiv_dx11.exe+729122 - 4C 89 68 D8           - mov [rax-28],r13
    // ffxiv_dx11.exe+729126 - 4C 89 70 D0           - mov [rax-30],r14
    // ffxiv_dx11.exe+72912A - 45 33 F6              - xor r14d,r14d
    // ffxiv_dx11.exe+72912D - 4C 89 78 C8           - mov [rax-38],r15
    // ffxiv_dx11.exe+729131 - 84 DB                 - test bl,bl
    // ffxiv_dx11.exe+729133 - 75 0D                 - jne ffxiv_dx11.exe+729142
    // ffxiv_dx11.exe+729135 - F6 05 87108301 04     - test byte ptr [ffxiv_dx11.exe+1F5A1C3],04 { (0),4 }
    // ffxiv_dx11.exe+72913C - 0F85 02020000         - jne ffxiv_dx11.exe+729344
    // ffxiv_dx11.exe+729142 - 8B D1                 - mov edx,ecx
    [Signature("48 8B C4 44 88 40 18 89 48 08", DetourName = nameof(ActionIntegrityDelegateDetour))]
    private readonly Hook<ActionIntegrityDelegate> actionIntegrityDelegateHook = null!;

    public CombatEventCapture(DeathRecapPlugin plugin) {
        this.plugin = plugin;

        SignatureHelper.Initialise(this);

        receiveAbilityEffectHook.Enable();
        receiveActorControlSelfHook.Enable();
        actionIntegrityDelegateHook.Enable();
    }

    private unsafe void ReceiveAbilityEffectDetour(
        int sourceId, IntPtr sourceCharacter, IntPtr pos, IntPtr effectHeader, IntPtr effectArray,
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
                                        Action = action?.ActionCategory.Row == 1 ? "Auto-attack" : action?.Name?.RawString?.Demangle() ?? "",
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
                                        Action = action?.Name?.RawString?.Demangle() ?? "",
                                        Icon = action?.Icon,
                                        Crit = (effectData[actionIndex + 2] & 1) == 1
                                    });
                                break;
                        }
                    }
            }
        } catch (Exception e) {
            PluginLog.Error(e, "Caught unexpected exception");
        }
    }

    private void ReceiveActorControlSelfDetour(
        uint entityId, uint type, uint buffId, uint param1, uint param2, uint sourceId, uint arg4, uint arg5,
        ulong targetId, byte a10) {
        receiveActorControlSelfHook.Original(entityId, type, buffId, param1, param2, sourceId, arg4, arg5, targetId, a10);

        try {
            if (!plugin.ConditionEvaluator.ShouldCapture(entityId))
                return;

            if (Service.ObjectTable.SearchById(entityId) is not PlayerCharacter p)
                return;

            switch ((ActorControlCategory)type) {
                case ActorControlCategory.DoT when param1 == 3:
                    combatEvents.AddEntry(entityId, new CombatEvent.DoT { Snapshot = p.Snapshot(), Amount = param2 });
                    break;
                case ActorControlCategory.HoT:
                    combatEvents.AddEntry(entityId, new CombatEvent.HoT { Snapshot = p.Snapshot(), Amount = param1 });
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

                combatEvents.AddEntry(targetId,
                    new CombatEvent.StatusEffect {
                        Snapshot = p.Snapshot(),
                        Id = effectId,
                        Icon = status?.Icon,
                        Status = status?.Name.RawString.Demangle(),
                        Description = status?.Description.DisplayedText().Demangle(),
                        Category = (StatusCategory)(status?.Category ?? 0),
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
