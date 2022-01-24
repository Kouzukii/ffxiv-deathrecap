using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Logging;
using DeathRecap.Messages;
using ImGuiNET;
using ImGuiScene;
using Lumina.Excel.GeneratedSheets;

namespace DeathRecap {
    public class DeathRecapWindow {
        private static readonly Vector4 ColorHealing = new(0.8196079F, 0.9803922F, 0.6F, 1);
        private static readonly Vector4 ColorDamage = new Vector4(0.9019608F, 0.5019608F, 0.4F, 1);
        private static readonly Vector4 ColorMagicDamage = new Vector4(0.145098F, 0.6F, 0.7450981F, 1f);
        private static readonly Vector4 ColorPhysicalDamage = new Vector4(1F, 0.6392157F, 0.2901961F, 1f);
        private static readonly Vector4 ColorAction = new Vector4(0.7215686F, 0.6588235F, 0.9411765F, 1);
        private static readonly Vector4 ColorGrey = new Vector4(0.5019608F, 0.5019608F, 0.5019608F, 1);

        private readonly DeathRecapPlugin plugin;

        private readonly Dictionary<ushort, TextureWrap> textures = new();

        public bool ShowDeathRecap { get; internal set; }

        private int selectedDeath;
        
        public DeathRecapWindow(DeathRecapPlugin plugin) {
            this.plugin = plugin;
        }

        public void Draw() {
            try {
                bool bShowDeathRecap = (ShowDeathRecap || (DateTime.Now - plugin.LastDeath)?.TotalSeconds < 30);
                ImGui.SetNextWindowSize(ImGuiHelpers.ScaledVector2(800, 350), ImGuiCond.FirstUseEver);
                if (bShowDeathRecap && ImGui.Begin("Death Recap", ref bShowDeathRecap, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoFocusOnAppearing)) {
                    if (ImGui.IsWindowFocused()) ShowDeathRecap = true;
                    if (selectedDeath >= plugin.Deaths.Count || selectedDeath < 0)
                        selectedDeath = 0;

                    ImGui.TextUnformatted("Player");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(200 * ImGuiHelpers.GlobalScale);
                    var playerName = plugin.ClientState.LocalPlayer?.Name?.TextValue ?? "";
                    if (ImGui.BeginCombo("", playerName)) {
                        if (ImGui.Selectable(playerName, true)) {
                        }
                        ImGui.EndCombo();
                    }

                    ImGui.SameLine();
                    bool bCaptureOnlyOwnDeaths = true;
                    ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
                    ImGui.Checkbox("Capture only own deaths", ref bCaptureOnlyOwnDeaths);
                    ImGui.PopStyleVar();

                    ImGui.SameLine(ImGui.GetWindowContentRegionWidth() - ImGuiHelpers.GetButtonSize("Clear").X);
                    if (ImGui.Button("Clear")) {
                        plugin.Deaths.Clear();
                    }

                    ImGui.Separator();

                    ImGui.Columns(2);
                    ImGui.SetColumnWidth(0, 160 * ImGuiHelpers.GlobalScale);
                    ImGui.TextUnformatted("Deaths");
                    ImGui.SetNextItemWidth(-1);
                    ImGui.ListBox("", ref selectedDeath, plugin.Deaths.Select(DeathTitle).Reverse().ToArray(), plugin.Deaths.Count);

                    ImGui.NextColumn();

                    CombatEventTable(plugin.Deaths.Count > 0 ? plugin.Deaths[plugin.Deaths.Count - 1 - selectedDeath] : new List<CombatEvent>());

                    ImGui.End();

                    if (!bShowDeathRecap) {
                        ShowDeathRecap = false;
                        plugin.LastDeath = null;
                    }
                }
            } catch (Exception e) {
                PluginLog.Error(e, "Failed to draw window");
            }
        }

        private void CombatEventTable(List<CombatEvent> deathEvents) {
            var deathTime = DateTime.Now;

            ImGui.BeginTable("deathrecap", 6, ImGuiTableFlags.Borders | ImGuiTableFlags.NoBordersInBody | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable);
            ImGui.TableSetupColumn("Time", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Amount", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Ability");
            ImGui.TableSetupColumn("Source");
            ImGui.TableSetupColumn("HP Before");
            ImGui.TableSetupColumn("Status Effects");
            ImGui.TableHeadersRow();

            for (var i = deathEvents.Count - 1; i >= 0; i--) {
                switch (deathEvents[i]) {
                    case CombatEvent.Death death:
                        deathTime = death.Snapshot.Time;
                        PrintTimeColumn(death, deathTime);

                        ImGui.TableNextColumn(); // Amount

                        ImGui.TableNextColumn(); // Ability
                        ImGui.TextUnformatted("Death");
                        break;
                    case CombatEvent.HoT hot:
                        ImGui.TableNextRow();

                        PrintTimeColumn(hot, deathTime);

                        var total = hot.Amount;
                        while (i > 0 && deathEvents[i - 1] is CombatEvent.HoT h) {
                            hot = h;
                            total += h.Amount;
                            i--;
                        }

                        ImGui.TableNextColumn(); // Amount
                        ImGui.TextColored(ColorHealing, $"+{total:N0}");

                        ImGui.TableNextColumn(); // Ability
                        ImGui.TextUnformatted("Regen");

                        ImGui.TableNextColumn(); // Source

                        PrintHpColumn(hot);
                        break;
                    case CombatEvent.DoT dot:
                        ImGui.TableNextRow();

                        PrintTimeColumn(dot, deathTime);

                        ImGui.TableNextColumn(); // Amount
                        ImGui.TextColored(ColorDamage, $"-{dot.Amount:N0}");

                        ImGui.TableNextColumn(); // Ability
                        ImGui.TextUnformatted($"DoT damage");

                        ImGui.TableNextColumn(); // Source

                        PrintHpColumn(dot);
                        break;
                    case CombatEvent.DamageTaken dt: {
                        ImGui.TableNextRow();

                        PrintTimeColumn(dt, deathTime);

                        ImGui.TableNextColumn(); // Amount
                        var text = $"-{dt.Amount:N0}{(dt.Crit ? dt.DirectHit ? "!!" : "!" : "")}";
                        if (dt.DamageType == DamageType.Magic) {
                            ImGui.TextColored(ColorMagicDamage, text);
                        } else {
                            ImGui.TextColored(ColorPhysicalDamage, text);
                        }

                        if (ImGui.IsItemHovered()) {
                            ImGui.BeginTooltip();
                            ImGui.TextUnformatted($"{dt.DamageType} Damage");
                            if (dt.Crit) ImGui.TextUnformatted("Critical Hit");
                            if (dt.DirectHit) ImGui.TextUnformatted("Direct Hit (+25%)");
                            if (dt.Parried) ImGui.TextUnformatted("Parried (-20%)");
                            if (dt.Blocked) ImGui.TextUnformatted("Blocked (-15%)");
                            ImGui.EndTooltip();
                        }

                        ImGui.TableNextColumn(); // Ability
                        if (dt.DisplayType != ActionEffectDisplayType.HideActionName) {
                            var img = GetIconImage(dt.Icon);
                            if (img != null) {
                                ImGui.Image(img.ImGuiHandle, ImGuiHelpers.ScaledVector2(16, 16));
                                ImGui.SameLine();
                            }

                            ImGui.TextColored(ColorAction, $"{dt.Action}");
                        }

                        ImGui.TableNextColumn(); // Source
                        ImGui.TextUnformatted(dt.Source ?? "");

                        PrintHpColumn(dt);

                        PrintStatusEffectsColumn(dt);
                        break;
                    }
                    case CombatEvent.Healed h: {
                        ImGui.TableNextRow();

                        PrintTimeColumn(h, deathTime);

                        ImGui.TableNextColumn(); // Amount
                        ImGui.TextColored(ColorHealing, $"+{h.Amount:N0}");

                        ImGui.TableNextColumn(); // Ability
                        var img = GetIconImage(h.Icon);
                        if (img != null) {
                            ImGui.Image(img.ImGuiHandle, ImGuiHelpers.ScaledVector2(16, 16));
                            ImGui.SameLine();
                        }

                        ImGui.TextColored(ColorAction, h.Action ?? "");

                        ImGui.TableNextColumn(); // Source
                        ImGui.TextUnformatted(h.Source ?? "");

                        PrintHpColumn(h);

                        PrintStatusEffectsColumn(h);
                        break;
                    }
                    case CombatEvent.StatusEffect s: {
                        ImGui.TableNextRow();

                        PrintTimeColumn(s, deathTime);

                        ImGui.TableNextColumn(); // Amount
                        ImGui.TextUnformatted($"{s.Duration:N0}s");

                        ImGui.TableNextColumn(); // Ability
                        var img = GetIconImage(s.Icon);
                        if (img != null) {
                            ImGui.Image(img.ImGuiHandle, ImGuiHelpers.ScaledVector2(16, 16));
                            ImGui.SameLine();
                        }

                        ImGui.TextUnformatted(s.Status ?? "");
                        if (ImGui.IsItemHovered()) {
                            ImGui.BeginTooltip();
                            ImGui.TextUnformatted(s.Description);
                            ImGui.EndTooltip();
                        }

                        ImGui.TableNextColumn(); // Source
                        ImGui.TextUnformatted(s.Source ?? "");

                        PrintHpColumn(s);

                        PrintStatusEffectsColumn(s);
                        break;
                    }
                }
            }

            ImGui.EndTable();
        }

        private string DeathTitle(List<CombatEvent> combatEvents) {
            var death = combatEvents[0];
            var timeSpan = DateTime.Now.Subtract(death.Snapshot.Time);

            if (timeSpan <= TimeSpan.FromSeconds(60)) {
                return $"{timeSpan.Seconds} seconds ago";
            }

            if (timeSpan <= TimeSpan.FromMinutes(60)) {
                return timeSpan.Minutes > 1 ? $"about {timeSpan.Minutes} minutes ago" : "about a minute ago";
            }

            return timeSpan.Hours > 1 ? $"about {timeSpan.Hours} hours ago" : "about an hour ago";
        }

        private void PrintStatusEffectsColumn(CombatEvent e) {
            if (e.Snapshot.StatusEffects != null) {
                ImGui.TableNextColumn();
                foreach (var effect in e.Snapshot.StatusEffects) {
                    if (plugin.DataManager.GetExcelSheet<Status>()?.GetRow(effect) is { } s) {
                        if (s.IsFcBuff) continue;
                        var img = GetIconImage(s.Icon);
                        if (img != null) {
                            ImGui.SameLine();
                            ImGui.Image(img.ImGuiHandle, new Vector2(16, 16) * ImGuiHelpers.GlobalScale);
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
        }

        private void PrintHpColumn(CombatEvent e) {
            ImGui.TableNextColumn();
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram, 0xFF058837);
            var hpFract = e.Snapshot.CurrentHp / (float?)e.Snapshot.MaxHp ?? 0;
            ImGui.ProgressBar(hpFract, new Vector2(-1, 0), $"{e.Snapshot.CurrentHp:N0}");
            ImGui.PopStyleColor();
            
            var itemMin = ImGui.GetItemRectMin();
            var itemMax = ImGui.GetItemRectMax();

            if (e.Snapshot.BarrierFraction.HasValue) {
                var barrierFract = e.Snapshot.BarrierFraction.Value / 100f;
                ImGui.GetWindowDrawList().PushClipRect(itemMin + new Vector2(0, (itemMax.Y - itemMin.Y) * 0.8f), itemMin + new Vector2((itemMax.X - itemMin.X) * barrierFract, itemMax.Y), true);
                ImGui.GetWindowDrawList().AddRectFilled(itemMin, itemMax, 0xFF33FFFF, ImGui.GetStyle().FrameRounding);
                ImGui.GetWindowDrawList().PopClipRect();
            }
        }

        private void PrintTimeColumn(CombatEvent e, DateTime deathTime) {
            ImGui.TableNextColumn();
            ImGui.TextColored(ColorGrey, $"{(e.Snapshot.Time - deathTime).TotalSeconds:N1}s");
        }

        private TextureWrap? GetIconImage(ushort? icon) {
            if (icon is { } u) {
                if (textures.TryGetValue(u, out var tex))
                    return tex;
                if (plugin.DataManager.GetImGuiTextureIcon(u) is { } t)
                    return textures[u] = t;
            }

            return null;
        }
    }
}