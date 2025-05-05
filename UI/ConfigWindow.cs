using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Text;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;

namespace DeathRecap.UI;

public class ConfigWindow : Window {
    private readonly DeathRecapPlugin plugin;

    public ConfigWindow(DeathRecapPlugin plugin) : base("Death Recap Config", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize) {
        this.plugin = plugin;

        Size = new Vector2(580, 340);
    }

    public override void Draw() {
        var conf = plugin.Configuration;

        ImGui.TextUnformatted("Capture Settings");
        ImGui.Separator();
        ImGui.Columns(3);
        foreach (var (k, v) in conf.EnumCaptureConfigs()) {
            ImGui.PushID(k);
            var bCapture = v.Capture;
            if (ImGui.Checkbox($"Capture {k}", ref bCapture)) {
                v.Capture = bCapture;
                conf.Save();
            }

            var notificationStyle = (int)v.NotificationStyle;
            ImGui.TextUnformatted("On Death");
            if (ImGui.Combo("##2", ref notificationStyle, "Do Nothing\0Chat Message\0Show Popup\0Open Recap")) {
                v.NotificationStyle = (NotificationStyle)notificationStyle;
                conf.Save();
            }

            var bOnlyInstances = v.OnlyInstances;
            if (ImGui.Checkbox("Only in instances", ref bOnlyInstances)) {
                v.OnlyInstances = bOnlyInstances;
                conf.Save();
            }

            OnlyInInstancesTooltip();

            var bDisableInPvp = v.DisableInPvp;
            if (ImGui.Checkbox("Disable in PvP", ref bDisableInPvp)) {
                v.DisableInPvp = bDisableInPvp;
                conf.Save();
            }

            ImGui.PopID();
            ImGui.NextColumn();
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
        if (ImGui.Combo("##3", ref chatType, string.Join('\0', chatTypes.Select(t => t.GetAttribute<XivChatTypeInfoAttribute>()?.FancyName ?? t.ToString())),
                10)) {
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
        
        
        var bRecordJobsAsSourceInPvp = conf.RecordJobsAsSourceInPvp;
        if (ImGui.Checkbox("Record job name as damage source in PvP", ref bRecordJobsAsSourceInPvp)) {
            conf.RecordJobsAsSourceInPvp = bRecordJobsAsSourceInPvp;
            conf.Save();
        }
        RecordJobsAsSourceInPvpTooltip();
    }

    private static void ChatMessageTypeTooltip() {
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Filter category of the \"Chat Message\" death notification.\n" +
                             "\"Debug\" will show up in all chat tabs regardless of configuration.\n" +
                             "Note that this will only affect the way the notification is displayed to you. They will never be visible to others.");
        }
    }

    private static void ChatTipTooltip() {
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Prints the command in the chat to reopen the window the first time you close the Death Recap.");
        }
    }

    private static void RecordJobsAsSourceInPvpTooltip() {
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Uses the job name instead of player name as damage source in PvP.");
        }
    }

    private static void OnlyInInstancesTooltip() {
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Will only show a death notification when in an instance (e.g. a Dungeon)");
        }
    }
}
