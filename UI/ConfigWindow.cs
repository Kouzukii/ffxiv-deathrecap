using System;
using System.Linq;
using Dalamud.Game.Text;
using Dalamud.Interface;
using ImGuiNET;

namespace DeathRecap.UI;

public class ConfigWindow {
    private readonly DeathRecapPlugin plugin;

    public ConfigWindow(DeathRecapPlugin plugin) {
        this.plugin = plugin;
    }

    public bool ShowConfig { get; internal set; }

    public void Draw() {
        if (!ShowConfig)
            return;
        var conf = plugin.Configuration;
        var bShowConfig = ShowConfig;
        ImGui.SetNextWindowSize(ImGuiHelpers.ScaledVector2(580, 320));
        if (ImGui.Begin("Death Recap Config", ref bShowConfig, ImGuiWindowFlags.NoCollapse)) {
            ImGui.TextUnformatted("Capture Settings");
            ImGui.Separator();
            ImGui.Columns(3);
            {
                ImGui.PushID(0);
                var bCapture = conf.CaptureSelf;
                if (ImGui.Checkbox("Capture Self", ref bCapture)) {
                    conf.CaptureSelf = bCapture;
                    conf.Save();
                }

                var notificationStyle = (int)conf.SelfNotification;
                ImGui.TextUnformatted("On Death");
                if (ImGui.Combo("##2", ref notificationStyle, "Do Nothing\0Chat Message\0Show Popup\0Open Recap")) {
                    conf.SelfNotification = (NotificationStyle)notificationStyle;
                    conf.Save();
                }

                var bOnlyInstances = conf.SelfNotificationOnlyInstances;
                if (ImGui.Checkbox("Only in instances", ref bOnlyInstances)) {
                    conf.SelfNotificationOnlyInstances = bOnlyInstances;
                    conf.Save();
                }

                OnlyInInstancesTooltip();
                ImGui.PopID();
            }
            ImGui.NextColumn();
            {
                ImGui.PushID(1);
                var bCapture = conf.CaptureParty;
                if (ImGui.Checkbox("Capture Party", ref bCapture)) {
                    conf.CaptureParty = bCapture;
                    conf.Save();
                }

                var notificationStyle = (int)conf.PartyNotification;
                ImGui.TextUnformatted("On Death");
                if (ImGui.Combo("##2", ref notificationStyle, "Do Nothing\0Chat Message\0Show Popup\0Open Recap")) {
                    conf.PartyNotification = (NotificationStyle)notificationStyle;
                    conf.Save();
                }

                var bOnlyInstances = conf.PartyNotificationOnlyInstances;
                if (ImGui.Checkbox("Only in instances", ref bOnlyInstances)) {
                    conf.PartyNotificationOnlyInstances = bOnlyInstances;
                    conf.Save();
                }

                OnlyInInstancesTooltip();
                ImGui.PopID();
            }
            ImGui.NextColumn();
            {
                ImGui.PushID(2);
                var bCapture = conf.CaptureOthers;
                if (ImGui.Checkbox("Capture Others", ref bCapture)) {
                    conf.CaptureOthers = bCapture;
                    conf.Save();
                }

                var notificationStyle = (int)conf.OthersNotification;
                ImGui.TextUnformatted("On Death");
                if (ImGui.Combo("##2", ref notificationStyle, "Do Nothing\0Chat Message\0Show Popup\0Open Recap")) {
                    conf.OthersNotification = (NotificationStyle)notificationStyle;
                    conf.Save();
                }

                var bOnlyInstances = conf.OthersNotificationOnlyInstances;
                if (ImGui.Checkbox("Only in instances", ref bOnlyInstances)) {
                    conf.OthersNotificationOnlyInstances = bOnlyInstances;
                    conf.Save();
                }

                OnlyInInstancesTooltip();
                ImGui.PopID();
            }
            ImGui.Columns();
            ImGui.Separator();
            ImGui.Spacing();
            ImGui.TextUnformatted("General Settings");
            ImGui.Spacing();
            var chatTypes = Enum.GetValues<XivChatType>();
            var chatType = Array.IndexOf(chatTypes, conf.ChatType);
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted("Chat Message Type");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(150 * ImGuiHelpers.GlobalScale);
            if (ImGui.Combo("##3", ref chatType, string.Join('\0', chatTypes.Select(t => t.GetDetails()?.FancyName ?? t.ToString())), 10)) {
                conf.ChatType = chatTypes[chatType];
                conf.Save();
            }

            ChatMessageTypeTooltip();

            var bShowTip = conf.ShowTip;
            if (ImGui.Checkbox("Show chat tip", ref bShowTip)) {
                conf.ShowTip = bShowTip;
                conf.Save();
            }

            ChatTipTooltip();
            var keepEventsFor = conf.KeepCombatEventsForSeconds;
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted("Keep Events for (sec)");
            ImGui.SameLine(ImGuiHelpers.GlobalScale * 140);
            ImGui.SetNextItemWidth(ImGuiHelpers.GlobalScale * 150);
            if (ImGui.InputInt("##4", ref keepEventsFor, 10)) {
                conf.KeepCombatEventsForSeconds = keepEventsFor;
                conf.Save();
            }

            var keepDeathsFor = conf.KeepDeathsForMinutes;
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted("Keep Deaths for (min)");
            ImGui.SameLine(ImGuiHelpers.GlobalScale * 140);
            ImGui.SetNextItemWidth(ImGuiHelpers.GlobalScale * 150);
            if (ImGui.InputInt("##5", ref keepDeathsFor, 10)) {
                conf.KeepDeathsForMinutes = keepDeathsFor;
                conf.Save();
            }

            ImGui.End();

            if (!bShowConfig)
                ShowConfig = false;
        }
    }

    private void ChatMessageTypeTooltip() {
        if (ImGui.IsItemHovered()) {
            ImGui.BeginTooltip();
            ImGui.TextUnformatted(
                "Filter category of the \"Chat Message\" death notification.\n\"Debug\" will show up in all chat tabs regardless of configuration.\nNote that this will only affect the way the notification is displayed to you. They will never be visible to others.");
            ImGui.EndTooltip();
        }
    }

    private static void ChatTipTooltip() {
        if (ImGui.IsItemHovered()) {
            ImGui.BeginTooltip();
            ImGui.TextUnformatted("Prints the command in the chat to reopen the window the first time you close the Death Recap.");
            ImGui.EndTooltip();
        }
    }

    private void OnlyInInstancesTooltip() {
        if (ImGui.IsItemHovered()) {
            ImGui.BeginTooltip();
            ImGui.TextUnformatted("Will only show a death notification when in an instance (e.g. a Dungeon)");
            ImGui.EndTooltip();
        }
    }
}
