using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Logging;
using DeathRecap.Messages;
using ImGuiNET;
using ImGuiScene;
using Lumina.Excel.GeneratedSheets;

namespace DeathRecap;

public class DeathRecapWindow {
    private static readonly Vector4 ColorHealing = new(0.8196079F, 0.9803922F, 0.6F, 1);
    private static readonly Vector4 ColorDamage = new(0.9019608F, 0.5019608F, 0.4F, 1);
    private static readonly Vector4 ColorMagicDamage = new(0.145098F, 0.6F, 0.7450981F, 1);
    private static readonly Vector4 ColorPhysicalDamage = new(1F, 0.6392157F, 0.2901961F, 1);
    private static readonly Vector4 ColorAction = new(0.7215686F, 0.6588235F, 0.9411765F, 1);
    private static readonly Vector4 ColorGrey = new(0.5019608F, 0.5019608F, 0.5019608F, 1);

    private readonly DeathRecapPlugin plugin;

    private readonly Dictionary<ushort, TextureWrap> textures = new();

    private bool hasShownTip;

    private int selectedDeath;

    public DeathRecapWindow(DeathRecapPlugin plugin) {
        this.plugin = plugin;

#if DEBUG
        ShowDeathRecap = true;
#endif
    }

    public bool ShowDeathRecap { get; internal set; }

    public uint SelectedPlayer { get; internal set; }

    public void Draw() {
        try {
            if (!ShowDeathRecap)
                return;
            var bShowDeathRecap = ShowDeathRecap;
            ImGui.SetNextWindowSize(ImGuiHelpers.ScaledVector2(800, 350), ImGuiCond.FirstUseEver);
            if (ImGui.Begin("Death Recap", ref bShowDeathRecap, ImGuiWindowFlags.NoCollapse)) {
                if (!plugin.DeathsPerPlayer.TryGetValue(SelectedPlayer, out var deaths))
                    deaths = new List<Death>();

                DrawPlayerSelection(deaths.FirstOrDefault()?.PlayerName);

                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.SameLine(0, 15);
                if (ImGui.Button(FontAwesomeIcon.Trash.ToIconString()))
                    plugin.DeathsPerPlayer.Clear();

                ImGui.PopFont();
                if (ImGui.IsItemHovered()) {
                    ImGui.BeginTooltip();
                    ImGui.TextUnformatted("Clear all events");
                    ImGui.EndTooltip();
                }

                ImGui.PushFont(UiBuilder.IconFont);
                var config = FontAwesomeIcon.Cog.ToIconString();
                ImGui.SameLine(ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X - ImGuiHelpers.GetButtonSize(config).X);
                if (ImGui.Button(config))
                    plugin.ConfigWindow.ShowConfig = true;

                ImGui.PopFont();
                if (ImGui.IsItemHovered()) {
                    ImGui.BeginTooltip();
                    ImGui.TextUnformatted("Configuration");
                    ImGui.EndTooltip();
                }

                ImGui.Separator();

                if (selectedDeath < 0 || selectedDeath >= deaths.Count)
                    selectedDeath = 0;

                ImGui.Columns(2);
                ImGui.SetColumnWidth(0, 160 * ImGuiHelpers.GlobalScale);
                ImGui.TextUnformatted("Deaths");
                ImGui.Spacing();
                for (var index = deaths.Count - 1; index >= 0; index--) {
                    ImGui.PushID(index);
                    if (ImGuiComponents.IconButton(FontAwesomeIcon.Ban)) {
                        if (deaths.Count - 1 - selectedDeath < index)
                            selectedDeath--;

                        deaths.RemoveAt(index--);
                    } else {
                        ImGui.SameLine();
                        if (ImGui.Selectable(deaths[index].Title, index == deaths.Count - 1 - selectedDeath))
                            selectedDeath = deaths.Count - 1 - index;
                    }

                    ImGui.PopID();
                }

                ImGui.NextColumn();

                DrawCombatEventTable(deaths.Count > selectedDeath ? deaths[deaths.Count - 1 - selectedDeath] : null);

                ImGui.End();

                if (!bShowDeathRecap) {
                    ShowDeathRecap = false;
                    if (plugin.Configuration.ShowTip && !hasShownTip) {
                        Service.ChatGui.Print("[DeathRecap] Tip: You can reopen this window using /dr or /deathrecap");
                        hasShownTip = true;
                    }
                }
            }
        } catch (Exception e) {
            PluginLog.Error(e, "Failed to draw window");
        }
    }

    private void DrawPlayerSelection(string? selectedPlayerName) {
        void DrawItem(IEnumerable<Death> pdeaths, uint id) {
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
            var processed = new HashSet<uint>();

            if (Service.PartyList.Length > 0) {
                foreach (var pmem in Service.PartyList) {
                    var id = pmem.ObjectId;
                    if (processed.Contains(id) || !plugin.DeathsPerPlayer.TryGetValue(id, out var pdeaths))
                        continue;
                    DrawItem(pdeaths, id);
                    processed.Add(id);
                }
            } else if (Service.ObjectTable[0]?.ObjectId is { } localPlayerId && plugin.DeathsPerPlayer.TryGetValue(localPlayerId, out var pdeaths)) {
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
                ImGuiTableFlags.Borders | ImGuiTableFlags.NoBordersInBody | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable)) {
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
                            ImGui.TextColored(ColorHealing, $"+{total:N0}");

                            ImGui.TableNextColumn(); // Ability
                            ImGui.AlignTextToFramePadding();
                            ImGui.TextUnformatted("Regen");

                            ImGui.TableNextColumn(); // Source

                            DrawHpColumn(hot, total);
                            break;
                        case CombatEvent.DoT dot:
                            ImGui.TableNextRow();

                            DrawTimeColumn(dot, death.TimeOfDeath);

                            ImGui.TableNextColumn(); // Amount
                            ImGui.AlignTextToFramePadding();
                            ImGui.TextColored(ColorDamage, $"-{dot.Amount:N0}");

                            ImGui.TableNextColumn(); // Ability
                            ImGui.AlignTextToFramePadding();
                            ImGui.TextUnformatted("DoT damage");

                            ImGui.TableNextColumn(); // Source

                            DrawHpColumn(dot);
                            break;
                        case CombatEvent.DamageTaken dt: {
                            ImGui.TableNextRow();

                            DrawTimeColumn(dt, death.TimeOfDeath);

                            ImGui.TableNextColumn(); // Amount
                            ImGui.AlignTextToFramePadding();
                            var text = $"-{dt.Amount:N0}{(dt.Crit ? dt.DirectHit ? "!!" : "!" : "")}";
                            if (dt.DamageType == DamageType.Magic)
                                ImGui.TextColored(ColorMagicDamage, text);
                            else
                                ImGui.TextColored(ColorPhysicalDamage, text);

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
                            if (dt.DisplayType != ActionEffectDisplayType.HideActionName) {
                                if (GetIconImage(dt.Icon) is { } img)
                                    InlineIcon(img);

                                ImGui.AlignTextToFramePadding();
                                ImGui.TextColored(ColorAction, $"{dt.Action}");
                            }

                            ImGui.TableNextColumn(); // Source
                            ImGui.AlignTextToFramePadding();
                            ImGui.TextUnformatted(dt.Source ?? "");

                            DrawHpColumn(dt);

                            DrawStatusEffectsColumn(dt);
                            break;
                        }
                        case CombatEvent.Healed h: {
                            ImGui.TableNextRow();

                            DrawTimeColumn(h, death.TimeOfDeath);

                            ImGui.TableNextColumn(); // Amount
                            ImGui.AlignTextToFramePadding();
                            ImGui.TextColored(ColorHealing, $"+{h.Amount:N0}");

                            ImGui.TableNextColumn(); // Ability
                            if (GetIconImage(h.Icon) is { } img)
                                InlineIcon(img);

                            ImGui.AlignTextToFramePadding();
                            ImGui.TextColored(ColorAction, h.Action ?? "");

                            ImGui.TableNextColumn(); // Source
                            ImGui.AlignTextToFramePadding();
                            ImGui.TextUnformatted(h.Source ?? "");

                            DrawHpColumn(h);

                            DrawStatusEffectsColumn(h);
                            break;
                        }
                        case CombatEvent.StatusEffect s: {
                            ImGui.TableNextRow();

                            DrawTimeColumn(s, death.TimeOfDeath);

                            ImGui.TableNextColumn(); // Amount
                            ImGui.AlignTextToFramePadding();
                            ImGui.TextUnformatted($"{s.Duration:N0}s");

                            ImGui.TableNextColumn(); // Ability
                            if (GetIconImage(s.Icon) is { } img)
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
                    }

            ImGui.EndTable();
        }
    }

    private static void InlineIcon(TextureWrap img, float paddingTop = 4, float paddingRight = 4) {
        var before = ImGui.GetCursorPos();
        ImGui.SetCursorPosY(before.Y + paddingTop);
        // ReSharper disable once PossibleLossOfFraction
        ImGui.Image(img.ImGuiHandle, ImGuiHelpers.ScaledVector2(16 * img.Width / img.Height, 16));
        ImGui.SameLine(0, paddingRight);
        ImGui.SetCursorPosY(before.Y);
    }

    private void DrawStatusEffectsColumn(CombatEvent e) {
        if (e.Snapshot.StatusEffects != null) {
            ImGui.TableNextColumn();
            foreach (var effect in e.Snapshot.StatusEffects)
                if (Service.DataManager.GetExcelSheet<Status>()?.GetRow(effect) is { } s) {
                    if (s.IsFcBuff)
                        continue;
                    if (GetIconImage(s.Icon) is { } img) {
                        InlineIcon(img);
                        if (ImGui.IsItemHovered()) {
                            ImGui.BeginTooltip();
                            ImGui.TextUnformatted(s.Name);
                            ImGui.TextUnformatted(s.Description.DisplayedText());
                            ImGui.EndTooltip();
                        }
                    }
                }
        }
    }

    private void DrawHpColumn(CombatEvent e, uint hot = 0) {
        ImGui.TableNextColumn();
        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, 0xFF058837);
        var hpFract = (float)e.Snapshot.CurrentHp / e.Snapshot.MaxHp;
        var change = hot > 0 ? (float)hot / e.Snapshot.MaxHp : 0;
        var overkill = 0;

        switch (e) {
            case CombatEvent.Healed h:
                change = (float)h.Amount / e.Snapshot.MaxHp;
                break;
            case CombatEvent.DamageTaken dt:
                overkill = (int)(dt.Amount - e.Snapshot.CurrentHp);
                change = -((float)dt.Amount / e.Snapshot.MaxHp);
                break;
            case CombatEvent.DoT dot:
                overkill = (int)(dot.Amount - e.Snapshot.CurrentHp);
                change = -((float)dot.Amount / e.Snapshot.MaxHp);
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
            drawList.AddRectFilled(bbMin, bbMax, change > 0 ? 0x50FFFFFFu : 0x50000000u, style.FrameRounding);
            drawList.PopClipRect();
        }

        if (e.Snapshot.BarrierPercent > 0) {
            var barrierFract = e.Snapshot.BarrierPercent / 100f;
            drawList.PushClipRect(bbMin + new Vector2(0, (bbMax.Y - bbMin.Y) * 0.8f), bbMin + bbMax with { X = (bbMax.X - bbMin.X) * barrierFract }, true);
            drawList.AddRectFilled(bbMin, bbMax, 0xFF33FFFF, style.FrameRounding);
            drawList.PopClipRect();
        }

        var textSiye = ImGui.CalcTextSize(text);
        drawList.AddText(
            new Vector2(Math.Clamp(bbMin.X + (bbMax.X - bbMin.X) * hpFract + style.ItemSpacing.X, bbMin.X, bbMax.X - textSiye.X - style.ItemInnerSpacing.X),
                bbMin.Y + (bbMax.Y - bbMin.Y - textSiye.Y) * 0.5f), 0xFFFFFFFF, text);

        if (overkill > 0 && ImGui.IsItemHovered()) {
            ImGui.BeginTooltip();
            ImGui.TextUnformatted($"Overkill by {overkill:N0}");
            ImGui.EndTooltip();
        }
    }

    private void DrawTimeColumn(CombatEvent e, DateTime deathTime) {
        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(ColorGrey, $"{(e.Snapshot.Time - deathTime).TotalSeconds:N1}s");
    }

    private TextureWrap? GetIconImage(ushort? icon) {
        if (icon is { } u) {
            if (textures.TryGetValue(u, out var tex))
                return tex;
            if (Service.DataManager.GetImGuiTextureIcon(u) is { } t)
                return textures[u] = t;
        }

        return null;
    }
}
