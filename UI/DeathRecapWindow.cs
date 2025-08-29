using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using DeathRecap.Events;
using FFXIVClientStructs.FFXIV.Client.Game;
using Newtonsoft.Json;
using Status = Lumina.Excel.Sheets.Status;

namespace DeathRecap.UI;

public class DeathRecapWindow : Window {
    private const uint ColorOverlayHealed = 0x50FFFFFFu;
    private const uint ColorOverlayDamageTaken = 0x50000000u;
    private const uint ColorBarrier = 0xFF33FFFF;
    private const uint ColorHp = 0xFF058837;
    private const uint ColorHealingText = 0xFF99fad1;
    private const uint ColorDamage = 0xFF6680e6;
    private const uint ColorMagicDamage = 0xFFbe9925;
    private const uint ColorPhysicalDamage = 0xFF4aa3ff;
    private const uint ColorAction = 0xFFf0a8b8;
    private const uint ColorGrey = 0xFF808080;
    private const uint ColorDarkGrey = 0xFF303030;

    private readonly DeathRecapPlugin plugin;

    private bool hasShownTip;

    public DeathRecapWindow(DeathRecapPlugin plugin) : base("Death Recap") {
        this.plugin = plugin;

        SizeCondition = ImGuiCond.FirstUseEver;
        Size = new Vector2(800, 350);
    }

    public ulong SelectedPlayer { get; internal set; }

    public int SelectedDeath { get; internal set; }

    public override void Draw() {
        try {
            if (!plugin.DeathsPerPlayer.TryGetValue(SelectedPlayer, out var deaths))
                deaths = new List<Death>();

            if (SelectedDeath < 0 || SelectedDeath >= deaths.Count)
                SelectedDeath = 0;

            var death = deaths.Count > SelectedDeath ? deaths[deaths.Count - 1 - SelectedDeath] : null;

            DrawPlayerSelection(deaths.FirstOrDefault()?.PlayerName);

            ImGui.SameLine(0, 15);
            if (plugin.Configuration.ShowCombatHistogram)
                ImGui.BeginDisabled();
            if (ImGuiComponents.IconButton("FilterButton", FontAwesomeIcon.Filter))
                ImGui.OpenPopup("death_recap_filter");
            if (ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Filter events");
            }

            if (plugin.Configuration.ShowCombatHistogram)
                ImGui.EndDisabled();

            if (ImGui.BeginPopup("death_recap_filter")) {
                ImGui.TextUnformatted("Row filters");

                void FlagCheckbox(string label, EventFilter flag) {
                    var b = plugin.Configuration.EventFilter.HasFlag(flag);
                    if (!ImGui.Checkbox(label, ref b))
                        return;
                    if (b)
                        plugin.Configuration.EventFilter |= flag;
                    else
                        plugin.Configuration.EventFilter &= ~flag;
                    plugin.Configuration.Save();
                }

                FlagCheckbox("Show damage", EventFilter.Damage);
                FlagCheckbox("Show healing", EventFilter.Healing);
                FlagCheckbox("Show debuffs", EventFilter.Debuffs);
                FlagCheckbox("Show buffs", EventFilter.Buffs);
                ImGui.EndPopup();
            }

            ImGui.SameLine(0, 15);
            if (ImGuiComponents.IconButton("Histogram Button", plugin.Configuration.ShowCombatHistogram ? FontAwesomeIcon.Table : FontAwesomeIcon.ChartBar)) {
                plugin.Configuration.ShowCombatHistogram ^= true;
                plugin.Configuration.Save();
            }

            if (ImGui.IsItemHovered()) {
                ImGui.SetTooltip(plugin.Configuration.ShowCombatHistogram ? "Switch to detailed view" : "Switch to histogram (experimental)");
            }

            ImGui.SameLine(0, 15);
            if (ImGuiComponents.IconButton("Clear All Button", FontAwesomeIcon.Trash))
                ImGui.OpenPopup("death_recap_clearall");

            if (ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Clear all events");
            }

            if (ImGui.BeginPopup("death_recap_clearall")) {
                ImGui.TextUnformatted("Are you sure?");
                if (ImGui.Button("Yes")) {
                    plugin.DeathsPerPlayer.Clear();
                    ImGui.CloseCurrentPopup();
                }

                ImGui.SameLine();
                if (ImGui.Button("No")) {
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }

#if DEBUG
            ImGui.PushFont(UiBuilder.IconFont);
            ImGui.SameLine(0, 15);
            if (ImGuiComponents.IconButton("Copy To Clipboard Button", FontAwesomeIcon.Copy)) {
                ImGui.SetClipboardText(JsonConvert.SerializeObject(death));
            }

            ImGui.PopFont();
            if (ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Copy combat events as JSON");
            }
#endif

            ImGui.PushFont(UiBuilder.IconFont);
            var config = FontAwesomeIcon.Cog.ToIconString();
            ImGui.SameLine(ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X - ImGuiHelpers.GetButtonSize(config).X);
            if (ImGui.Button(config))
                plugin.ConfigWindow.IsOpen = true;

            ImGui.PopFont();
            if (ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Configuration");
            }

            ImGui.Separator();

            ImGui.Columns(2);
            ImGui.SetColumnWidth(0, 160 * ImGuiHelpers.GlobalScale);
            ImGui.TextUnformatted("Deaths");
            ImGui.Spacing();
            for (var index = deaths.Count - 1; index >= 0; index--) {
                ImGui.PushID(index);
                if (ImGui.Selectable(deaths[index].Title, index == deaths.Count - 1 - SelectedDeath))
                    SelectedDeath = deaths.Count - 1 - index;
                ImGui.PopID();
            }

            ImGui.NextColumn();

            if (plugin.Configuration.ShowCombatHistogram)
                DrawCombatHistogram(death);
            else
                DrawCombatEventTable(death);
        } catch (Exception e) {
            Service.PluginLog.Error(e, "Failed to draw window");
        }
    }

    public override void OnClose() {
        if (plugin.Configuration.ShowTip && !hasShownTip) {
            Service.ChatGui.Print("[DeathRecap] Tip: You can reopen this window using /dr or /deathrecap");
            hasShownTip = true;
        }
    }

    private void DrawPlayerSelection(string? selectedPlayerName) {
        void DrawItem(IEnumerable<Death> pdeaths, ulong id) {
            if (pdeaths.FirstOrDefault()?.PlayerName is not { } name)
                return;
            if (ImGui.Selectable(name, id == SelectedPlayer))
                SelectedPlayer = id;
        }

        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted("Player");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(200 * ImGuiHelpers.GlobalScale);
        if (ImGui.BeginCombo("", selectedPlayerName)) {
            var processed = new HashSet<ulong>();

            if (Service.PartyList.Length > 0) {
                foreach (var pmem in Service.PartyList) {
                    var id = pmem.ObjectId;
                    if (processed.Contains(id) || !plugin.DeathsPerPlayer.TryGetValue(id, out var pdeaths))
                        continue;
                    DrawItem(pdeaths, id);
                    processed.Add(id);
                }
            } else if (Service.ObjectTable[0]?.GameObjectId is { } localPlayerId && plugin.DeathsPerPlayer.TryGetValue(localPlayerId, out var pdeaths)) {
                DrawItem(pdeaths, localPlayerId);
                processed.Add(localPlayerId);
            }

            foreach (var (id, pdeaths) in plugin.DeathsPerPlayer) {
                if (processed.Contains(id))
                    continue;
                DrawItem(pdeaths, id);
            }

            ImGui.EndCombo();
        }
    }

    private void DrawCombatEventTable(Death? death) {
        if (ImGui.BeginTable("deathrecap", 6,
                ImGuiTableFlags.Borders | ImGuiTableFlags.NoBordersInBody | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable | ImGuiTableFlags.Reorderable |
                ImGuiTableFlags.Hideable)) {
            ImGui.TableSetupColumn("Time", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Amount", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Ability");
            ImGui.TableSetupColumn("Source");
            ImGui.TableSetupColumn("HP Before");
            ImGui.TableSetupColumn("Status Effects");
            ImGui.TableHeadersRow();

            if (death != null)
                for (var i = death.Events.Count - 1; i >= 0; i--)
                    switch (death.Events[i]) {
                        case CombatEvent.HoT hot:
                            if (!plugin.Configuration.EventFilter.HasFlag(EventFilter.Healing))
                                continue;
                            ImGui.TableNextRow();

                            DrawTimeColumn(hot, death.TimeOfDeath);

                            var total = hot.Amount;
                            while (i > 0 && death.Events[i - 1] is CombatEvent.HoT h) {
                                hot = h;
                                total += h.Amount;
                                i--;
                            }

                            ImGui.TableNextColumn(); // Amount
                            ImGui.AlignTextToFramePadding();
                            ImGuiHelper.TextColored(ColorHealingText, $"+{total:N0}");

                            ImGui.TableNextColumn(); // Ability
                            ImGui.AlignTextToFramePadding();
                            ImGui.TextUnformatted("Regen");

                            ImGui.TableNextColumn(); // Source

                            DrawHpColumn(hot, total);
                            break;
                        case CombatEvent.DoT dot:
                            if (!plugin.Configuration.EventFilter.HasFlag(EventFilter.Damage))
                                continue;
                            ImGui.TableNextRow();

                            DrawTimeColumn(dot, death.TimeOfDeath);

                            ImGui.TableNextColumn(); // Amount
                            ImGui.AlignTextToFramePadding();
                            ImGuiHelper.TextColored(ColorDamage, $"-{dot.Amount:N0}");

                            ImGui.TableNextColumn(); // Ability
                            ImGui.AlignTextToFramePadding();
                            ImGui.TextUnformatted("DoT damage");

                            ImGui.TableNextColumn(); // Source

                            DrawHpColumn(dot);
                            break;
                        case CombatEvent.DamageTaken dt: {
                            if (!plugin.Configuration.EventFilter.HasFlag(EventFilter.Damage))
                                continue;
                            ImGui.TableNextRow();

                            DrawTimeColumn(dt, death.TimeOfDeath);

                            ImGui.TableNextColumn(); // Amount
                            ImGui.AlignTextToFramePadding();
                            var text = $"-{dt.Amount:N0}{(dt.Crit ? dt.DirectHit ? "!!" : "!" : "")}";
                            ImGuiHelper.TextColored(dt.DamageType == DamageType.Magic ? ColorMagicDamage : ColorPhysicalDamage, text);

                            if (ImGui.IsItemHovered()) {
                                ImGui.BeginTooltip();
                                ImGui.TextUnformatted($"{dt.DamageType} Damage");
                                if (dt.Crit)
                                    ImGui.TextUnformatted("Critical Hit");
                                if (dt.DirectHit)
                                    ImGui.TextUnformatted("Direct Hit (+25%)");
                                if (dt.Parried)
                                    ImGui.TextUnformatted("Parried (-20%)");
                                if (dt.Blocked)
                                    ImGui.TextUnformatted("Blocked (-15%)");
                                ImGui.EndTooltip();
                            }

                            ImGui.TableNextColumn(); // Ability
                            if (dt.DisplayType != ActionType.None) {
                                if (GetIconImage(dt.Icon) is { } img)
                                    InlineIcon(img);

                                ImGui.AlignTextToFramePadding();
                                ImGuiHelper.TextColored(ColorAction, $"{dt.Action}");
                            }

                            ImGui.TableNextColumn(); // Source
                            ImGui.AlignTextToFramePadding();
                            ImGui.TextUnformatted(dt.Source ?? "");

                            DrawHpColumn(dt);

                            DrawStatusEffectsColumn(dt);
                            break;
                        }
                        case CombatEvent.Healed h: {
                            if (!plugin.Configuration.EventFilter.HasFlag(EventFilter.Healing))
                                continue;
                            ImGui.TableNextRow();

                            DrawTimeColumn(h, death.TimeOfDeath);

                            ImGui.TableNextColumn(); // Amount
                            ImGui.AlignTextToFramePadding();
                            ImGuiHelper.TextColored(ColorHealingText, $"+{h.Amount:N0}{(h.Crit ? "!" : "")}");

                            ImGui.TableNextColumn(); // Ability
                            if (GetIconImage(h.Icon) is { } img)
                                InlineIcon(img);

                            ImGui.AlignTextToFramePadding();
                            ImGuiHelper.TextColored(ColorAction, h.Action);

                            ImGui.TableNextColumn(); // Source
                            ImGui.AlignTextToFramePadding();
                            ImGui.TextUnformatted(h.Source ?? "");

                            DrawHpColumn(h);

                            DrawStatusEffectsColumn(h);
                            break;
                        }
                        case CombatEvent.StatusEffect s: {
                            switch (s.Category) {
                                case StatusCategory.Beneficial when !plugin.Configuration.EventFilter.HasFlag(EventFilter.Buffs):
                                case StatusCategory.Detrimental when !plugin.Configuration.EventFilter.HasFlag(EventFilter.Debuffs):
                                    continue;
                            }

                            ImGui.TableNextRow();

                            DrawTimeColumn(s, death.TimeOfDeath);

                            ImGui.TableNextColumn(); // Amount
                            ImGui.AlignTextToFramePadding();
                            ImGui.TextUnformatted($"{s.Duration:N0}s");

                            ImGui.TableNextColumn(); // Ability
                            if (GetIconImage(s.Icon, s.StackCount) is { } img)
                                InlineIcon(img);

                            ImGui.AlignTextToFramePadding();
                            ImGui.TextUnformatted(s.Status ?? "");
                            if (ImGui.IsItemHovered()) {
                                ImGui.BeginTooltip();
                                ImGui.TextUnformatted(s.Description);
                                ImGui.EndTooltip();
                            }

                            ImGui.TableNextColumn(); // Source
                            ImGui.AlignTextToFramePadding();
                            ImGui.TextUnformatted(s.Source ?? "");

                            DrawHpColumn(s);

                            DrawStatusEffectsColumn(s);
                            break;
                        }
                        case CombatEvent.MedKit mk: {
                                if (!plugin.Configuration.EventFilter.HasFlag(EventFilter.Healing))
                                    continue;
                                ImGui.TableNextRow();
                                DrawTimeColumn(mk, death.TimeOfDeath);

                                ImGui.TableNextColumn(); // Amount
                                ImGui.AlignTextToFramePadding();
                                ImGuiHelper.TextColored(ColorHealingText, $"+{mk.Amount:N0}");

                                ImGui.TableNextColumn(); // Ability
                                ImGui.AlignTextToFramePadding();
                                ImGui.TextUnformatted("Medicinal Kit");

                                ImGui.TableNextColumn(); // Source

                                DrawHpColumn(mk);
                                break;
                        }
                        case CombatEvent.FallDamage fd: {
                                if (!plugin.Configuration.EventFilter.HasFlag(EventFilter.Damage))
                                    continue;
                                ImGui.TableNextRow();

                                DrawTimeColumn(fd, death.TimeOfDeath);

                                ImGui.TableNextColumn(); // Amount
                                ImGui.AlignTextToFramePadding();
                                var text = $"-{fd.Amount:N0}";
                                ImGuiHelper.TextColored(ColorDamage, text);

                                ImGui.TableNextColumn(); // Ability
                                ImGui.AlignTextToFramePadding();
                                ImGui.TextUnformatted("Fall Damage");

                                ImGui.TableNextColumn(); // Source

                                DrawHpColumn(fd);
                                break;
                            }
                    }

            ImGui.EndTable();
        }
    }

    private void DrawCombatHistogram(Death? death) {
        var style = ImGui.GetStyle();
        ImGui.Dummy(ImGui.GetWindowSize() - ImGui.GetCursorPos() - style.FramePadding * 4);
        var bbMin = ImGui.GetItemRectMin();
        var frameSize = ImGui.GetItemRectSize();
        var hovered = ImGui.IsItemHovered();

        RenderFrame(bbMin, bbMin + frameSize, ImGui.GetColorU32(ImGuiCol.FrameBg), true, style.FrameRounding);

        if (death == null || death.Events.Count == 0)
            return;

        int Next(int i) {
            for (i += 1; i < death.Events.Count && death.Events[i] is CombatEvent.StatusEffect; i++) {
            }

            return i;
        }

        var mousePos = ImGui.GetMousePos();
        var innerPos = bbMin + style.FramePadding;
        var innerSize = frameSize - style.FramePadding * 2;
        var drawList = ImGui.GetWindowDrawList();
        var n = Next(-1);
        if ((death.TimeOfDeath - death.Events[n].Snapshot.Time).TotalSeconds > 20)
            for (; n < death.Events.Count - 1 && (death.TimeOfDeath - death.Events[Next(n)].Snapshot.Time).TotalSeconds > 20; n = Next(n)) {
            }

        var eCur = death.Events[n];
        var yScaleMax = (uint)(eCur.Snapshot.MaxHp * 1.3);
        var tScaleMin = death.TimeOfDeath - TimeSpan.FromSeconds(20);
        var tScaleMax = death.TimeOfDeath;
        var invYScale = 1.0f / yScaleMax;
        var invTScale = 1.0f / (tScaleMax - tScaleMin).TotalSeconds;
        var pntCur = new Vector2(0, 1.0f - Math.Clamp(eCur.Snapshot.CurrentHp * invYScale, 0, 1));
        var minWidth = 5 / innerSize.X;
        var lastBarPos = float.MaxValue;

        ImGui.PushClipRect(bbMin, bbMin + frameSize, true);
        for (; n < death.Events.Count; n = Next(n)) {
            CombatEvent? eNext;
            Vector2 pntNext;
            if (n < death.Events.Count - 1) {
                eNext = death.Events[Math.Min(Next(n), death.Events.Count - 1)];
                pntNext = new Vector2((float)((eNext.Snapshot.Time - tScaleMin).TotalSeconds * invTScale),
                    1.0f - Math.Clamp(eNext.Snapshot.CurrentHp * invYScale, 0, 1));
                if (pntNext.X - pntCur.X < minWidth)
                    pntNext.X = pntCur.X + minWidth;
            } else {
                eNext = null;
                pntNext = new Vector2(1, 1);
            }

            var change = 0L;
            var text = "";
            var numCol = 0xFFu;

            switch (eCur) {
                case CombatEvent.Healed h:
                    change = h.Amount;
                    pntCur.Y -= change * invYScale;
                    text = h.Action;
                    numCol = ColorHealingText;
                    break;
                case CombatEvent.HoT hot:
                    change = hot.Amount;
                    pntCur.Y -= change * invYScale;
                    text = "HoT";
                    numCol = ColorHealingText;
                    break;
                case CombatEvent.DamageTaken dt:
                    change = -dt.Amount;
                    text = dt.Action;
                    numCol = dt.DamageType == DamageType.Magic ? ColorMagicDamage : ColorPhysicalDamage;
                    break;
                case CombatEvent.DoT dot:
                    change = -dot.Amount;
                    text = "DoT";
                    numCol = ColorDamage;
                    break;
                case CombatEvent.MedKit mk:
                    change = mk.Amount;
                    pntCur.Y -= change * invYScale;
                    text = "MedKit";
                    numCol = ColorHealingText;
                    break;
                case CombatEvent.FallDamage fd:
                    change = -fd.Amount;
                    text = "Fall Damage";
                    numCol = ColorDamage;
                    break;
            }

            var changeScaled = (float)change / yScaleMax;

            var pos1 = innerPos + innerSize * pntCur;
            var pos2 = innerPos + innerSize * pntNext with { Y = 1 };
            if (hovered && mousePos.X >= pos1.X && mousePos.X <= pos2.X) {
                ImGui.BeginTooltip();
                switch (eCur) {
                    case CombatEvent.Healed h: {
                        ImGui.AlignTextToFramePadding();
                        ImGui.TextUnformatted("Healed for");
                        ImGui.SameLine(0, 4);
                        ImGuiHelper.TextColored(numCol, $"{h.Amount:N0}");
                        ImGui.SameLine(0, 4);
                        ImGui.TextUnformatted("from");
                        ImGui.SameLine(0, 6);
                        if (GetIconImage(h.Icon) is { } img) {
                            InlineIcon(img);
                        }

                        ImGuiHelper.TextColored(ColorAction, $"{h.Action}");
                        ImGui.SameLine(0, 4);
                        ImGui.TextUnformatted($"by {h.Source ?? ""}.");
                        break;
                    }
                    case CombatEvent.DamageTaken dt: {
                        ImGui.AlignTextToFramePadding();
                        ImGui.TextUnformatted("Took");
                        ImGui.SameLine(0, 4);
                        ImGuiHelper.TextColored(numCol, $"{dt.Amount:N0}");
                        ImGui.SameLine(0, 4);
                        ImGui.TextUnformatted("from");
                        ImGui.SameLine(0, 6);
                        if (GetIconImage(dt.Icon) is { } img) {
                            InlineIcon(img);
                        }

                        ImGuiHelper.TextColored(ColorAction, $"{dt.Action}");
                        ImGui.SameLine(0, 4);
                        ImGui.TextUnformatted($"by {dt.Source ?? ""}.");

                        break;
                    }
                    case CombatEvent.DoT dt:
                        ImGui.TextUnformatted("Took");
                        ImGui.SameLine(0, 4);
                        ImGuiHelper.TextColored(numCol, $"{dt.Amount:N0}");
                        ImGui.SameLine(0, 4);
                        ImGui.TextUnformatted("from damage over time effect.");
                        break;
                    case CombatEvent.HoT hot:
                        ImGui.TextUnformatted("Recovered");
                        ImGui.SameLine(0, 4);
                        ImGuiHelper.TextColored(numCol, $"{hot.Amount:N0}");
                        ImGui.SameLine(0, 4);
                        ImGui.TextUnformatted("HP from healing over time effect.");
                        break;
                    case CombatEvent.MedKit mk:
                        ImGui.TextUnformatted("Healed for");
                        ImGui.SameLine(0, 4);
                        ImGuiHelper.TextColored(numCol, $"{mk.Amount:N0}");
                        ImGui.SameLine(0, 4);
                        ImGui.TextUnformatted("HP from a medicinal kit.");
                        break;
                    case CombatEvent.FallDamage fd:
                        ImGui.TextUnformatted("Took");
                        ImGui.SameLine(0, 4);
                        ImGuiHelper.TextColored(numCol, $"{fd.Amount:N0}");
                        ImGui.SameLine(0, 4);
                        ImGui.TextUnformatted("damage from falling.");
                        break;
                }

                ImGui.TextUnformatted(
                    $"HP: {eCur.Snapshot.CurrentHp:N0} ({(float)eCur.Snapshot.CurrentHp / eCur.Snapshot.MaxHp:P1}) → {eCur.Snapshot.CurrentHp + change:N0} ({(float)(eCur.Snapshot.CurrentHp + change) / eCur.Snapshot.MaxHp:P1})");

                if (eCur.Snapshot.StatusEffects is { Count: > 0 } statusEffects) {
                    ImGui.TextUnformatted("Status Effects");
                    var printSeparator = false;
                    var statusSheet = Service.DataManager.GetExcelSheet<Status>();
                    foreach (var category in statusEffects.Select(s => (Status: statusSheet.GetRow(s.Id), s.StackCount))
                                 .Reverse()
                                 .GroupBy(s => s.Status.StatusCategory)
                                 .OrderByDescending(s => s.Key)) {
                        if (category.Key == 0)
                            continue;
                        if (printSeparator) {
                            ImGuiHelper.TextColored(ColorGrey, "|");
                            ImGui.SameLine(0, 4);
                        } else
                            printSeparator = true;

                        foreach (var s in category) {
                            if (GetIconImage(s.Status.Icon, s.StackCount) is { } img) {
                                InlineIcon(img, 2);
                            }
                        }
                    }
                }

                ImGui.EndTooltip();
            }

            drawList.AddRectFilled(pos1, pos2, ColorHp);
            if (change > 0) {
                drawList.AddRectFilled(pos1, innerPos + innerSize * pntNext with { Y = pntCur.Y + changeScaled }, ColorOverlayHealed);
            } else if (change < 0) {
                drawList.AddRectFilled(pos1, innerPos + innerSize * pntNext with { Y = pntCur.Y - changeScaled }, ColorOverlayDamageTaken);
            }

            drawList.AddLine(pos1, new Vector2(pos1.X, pos2.Y), 0xA0000000, 1f);
            drawList.AddLine(pos1, new Vector2(pos2.X, pos1.Y), 0xA0000000, 1f);
            if (lastBarPos < pos1.Y)
                drawList.AddLine(pos1 with { Y = lastBarPos }, pos1, 0xA0000000, 1f);
            lastBarPos = pos1.Y;

            if (pos2.X - pos1.X > 50) {
                var textSize = ImGui.CalcTextSize(text, false, pos2.X - pos1.X);
                if (innerPos.Y + innerSize.Y - pos1.Y - innerSize.Y * Math.Abs(changeScaled) < textSize.Y) {
                    drawList.AddTextOutlined(pos1 + new Vector2(pos2.X - pos1.X - textSize.X, -textSize.Y * 2 - 10) * 0.5f, 0xFFFFFFFF, 0xFF000000, text,
                        textSize.X);
                } else {
                    drawList.AddTextOutlined(pos1 + (pos2 - pos1 - textSize + innerSize * new Vector2(0, Math.Abs(changeScaled))) * 0.5f, 0xFFFFFFFF,
                        0xFF000000, text, textSize.X);
                }

                var changeText = $"{(change > 0 ? "+" : "")}{change:N0}";
                textSize = ImGui.CalcTextSize(changeText);
                drawList.AddTextOutlined(pos1 + (new Vector2(pos2.X - pos1.X, innerSize.Y * Math.Abs(changeScaled)) - textSize) * 0.5f, numCol, 0xFF000000,
                    changeText);
            }

            if (eNext != null)
                eCur = eNext;
            pntCur = pntNext;
        }

        var maxHpLine = new Vector2(0, innerSize.Y * (1 - eCur.Snapshot.MaxHp * invYScale));
        drawList.AddLine(innerPos + maxHpLine, innerPos + innerSize with { Y = maxHpLine.Y }, ColorGrey);
        var maxHpText = eCur.Snapshot.MaxHp.ToString("N0");
        drawList.AddText(innerPos + maxHpLine - ImGui.CalcTextSize(maxHpText) with { X = 0 }, ColorGrey, maxHpText);

        var maxLabels = (int)(innerSize.X * 0.01);
        while (Math.DivRem(20, maxLabels, out var rem) == 0 || rem != 0) {
            maxLabels--;
        }

        if (maxLabels > 0) {
            var labelPos = (innerSize.X - 1) / maxLabels;
            for (var i = 0; i <= maxLabels; i++) {
                var pos = labelPos * i;
                drawList.AddLine(innerPos + new Vector2(pos, innerSize.Y - 10), innerPos + innerSize with { X = pos }, ColorDarkGrey, 2);
                var text = $"-{20 - 20 / maxLabels * i}";
                var textSize = ImGui.CalcTextSize(text);
                if (i == maxLabels) {
                    pos -= textSize.X + 2;
                } else if (i > 0) {
                    pos -= textSize.X * 0.5f;
                } else {
                    pos += 2;
                }

                drawList.AddText(innerPos + new Vector2(pos, innerSize.Y - textSize.Y - 10), ColorDarkGrey, text);
            }
        }

        ImGui.PopClipRect();
    }

    private static void RenderFrame(Vector2 pMin, Vector2 pMax, uint fillCol, bool border, float rounding) {
        var drawList = ImGui.GetWindowDrawList();
        var style = ImGui.GetStyle();
        drawList.AddRectFilled(pMin, pMax, fillCol, rounding);
        float borderSize = style.FrameBorderSize;
        if (border && borderSize > 0.0f) {
            drawList.AddRect(pMin + new Vector2(1, 1), pMax + new Vector2(1, 1), ImGui.GetColorU32(ImGuiCol.BorderShadow), rounding, 0, borderSize);
            drawList.AddRect(pMin, pMax, ImGui.GetColorU32(ImGuiCol.Border), rounding, 0, borderSize);
        }
    }

    private static void InlineIcon(IDalamudTextureWrap img, float paddingTop = 4, float paddingRight = 4) {
        var imgPos = ImGui.GetCursorScreenPos() + new Vector2(0, paddingTop);
        // ReSharper disable once PossibleLossOfFraction
        var imgSize = ImGuiHelpers.ScaledVector2(16 * img.Width / img.Height, 16);
        ImGui.Dummy(imgSize);
        ImGui.GetWindowDrawList().AddImage(img.Handle, imgPos, imgPos + imgSize);
        ImGui.SameLine(0, paddingRight);
    }

    private void DrawStatusEffectsColumn(CombatEvent e) {
        if (e.Snapshot.StatusEffects is { } statusEffects) {
            ImGui.TableNextColumn();
            var printSeparator = false;
            var sheet = Service.DataManager.GetExcelSheet<Status>();
            foreach (var group in statusEffects.Select(s => (Status: sheet.GetRow(s.Id), s.StackCount))
                         .Reverse()
                         .GroupBy(s => s.Status.StatusCategory)
                         .OrderByDescending(s => s.Key)) {
                if (group.Key == 0)
                    continue;
                if (printSeparator) {
                    ImGui.AlignTextToFramePadding();
                    ImGuiHelper.TextColored(ColorGrey, "|");
                    ImGui.SameLine(0, 4);
                } else
                    printSeparator = true;

                foreach (var s in group) {
                    if (s.Status.IsFcBuff)
                        continue;
                    if (GetIconImage(s.Status.Icon, s.StackCount <= s.Status.MaxStacks ? s.StackCount : 0) is { } img) {
                        InlineIcon(img);
                        if (ImGui.IsItemHovered()) {
                            ImGui.BeginTooltip();
                            ImGui.TextUnformatted(s.Status.Name.ExtractText());
                            ImGui.TextUnformatted(s.Status.Description.ExtractText());
                            ImGui.EndTooltip();
                        }
                    }
                }
            }
        }
    }

    private void DrawHpColumn(CombatEvent e, uint hot = 0) {
        ImGui.TableNextColumn();
        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, ColorHp);
        var hpFract = (float)e.Snapshot.CurrentHp / e.Snapshot.MaxHp;
        var change = hot > 0 ? (float)hot / e.Snapshot.MaxHp : 0;
        var overkill = 0;

        switch (e) {
            case CombatEvent.MedKit mk: change = (float)mk.Amount / e.Snapshot.MaxHp; break;
            case CombatEvent.Healed h: change = (float)h.Amount / e.Snapshot.MaxHp; break;
            case CombatEvent.DamageTaken dt:
                overkill = (int)(dt.Amount - e.Snapshot.CurrentHp);
                change = -((float)dt.Amount / e.Snapshot.MaxHp);
                break;
            case CombatEvent.DoT dot:
                overkill = (int)(dot.Amount - e.Snapshot.CurrentHp);
                change = -((float)dot.Amount / e.Snapshot.MaxHp);
                break;
            case CombatEvent.FallDamage fd:
                overkill = (int)(fd.Amount - e.Snapshot.CurrentHp);
                change = -((float)fd.Amount / e.Snapshot.MaxHp);
                break;
        }

        if (change > 0)
            hpFract += change;

        var text = $"{e.Snapshot.CurrentHp:N0}";
        ImGui.ProgressBar(hpFract, new Vector2(-1, 0), text);
        ImGui.PopStyleColor();

        var bbMin = ImGui.GetItemRectMin();
        var bbMax = ImGui.GetItemRectMax();

        var style = ImGui.GetStyle();
        var drawList = ImGui.GetWindowDrawList();
        if (change is > 0 or < 0) {
            drawList.PushClipRect(bbMin + new Vector2((bbMax.X - bbMin.X) * (hpFract - Math.Abs(change)), 0),
                bbMin + bbMax with { X = (float)Math.Round((bbMax.X - bbMin.X) * hpFract) }, true);
            drawList.AddRectFilled(bbMin, bbMax, change > 0 ? ColorOverlayHealed : ColorOverlayDamageTaken, style.FrameRounding);
            drawList.PopClipRect();
        }

        if (e.Snapshot.BarrierPercent > 0) {
            var barrierFract = e.Snapshot.BarrierPercent / 100f;
            drawList.PushClipRect(bbMin + new Vector2(0, (bbMax.Y - bbMin.Y) * 0.8f), bbMin + bbMax with { X = (bbMax.X - bbMin.X) * barrierFract }, true);
            drawList.AddRectFilled(bbMin, bbMax, ColorBarrier, style.FrameRounding);
            drawList.PopClipRect();
        }

        var textSiye = ImGui.CalcTextSize(text);
        drawList.AddText(new Vector2(
            Math.Clamp(bbMin.X + (bbMax.X - bbMin.X) * hpFract + style.ItemSpacing.X, bbMin.X, bbMax.X - textSiye.X - style.ItemInnerSpacing.X),
            bbMin.Y + (bbMax.Y - bbMin.Y - textSiye.Y) * 0.5f), 0xFFFFFFFF, text);

        if (overkill > 0 && ImGui.IsItemHovered()) {
            ImGui.SetTooltip($"Overkill by {overkill:N0}");
        }
    }

    private void DrawTimeColumn(CombatEvent e, DateTime deathTime) {
        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGuiHelper.TextColored(ColorGrey, $"{(e.Snapshot.Time - deathTime).TotalSeconds:N1}s");
    }

    private static IDalamudTextureWrap? GetIconImage(uint? icon, uint stackCount = 0) {
        if (icon is not { } idx)
            return null;
        if (stackCount > 1)
            idx += stackCount - 1;
        return Service.TextureProvider.TryGetIconPath(idx, out var path)
            ? Service.TextureProvider.GetFromGame(path).GetWrapOrDefault()
            : null;
    }
}
