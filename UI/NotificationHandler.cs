using System;
using System.Linq;
using System.Numerics;
using System.Text;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using DeathRecap.Events;
using ImGuiNET;

namespace DeathRecap.UI;

public class NotificationHandler : Window{
    private readonly DalamudLinkPayload chatLinkPayload;
    private readonly DeathRecapPlugin plugin;
    private Death? popupDeath;

    public NotificationHandler(DeathRecapPlugin plugin) : base("###deathRecapPopup") {
        this.plugin = plugin;

        PositionCondition = ImGuiCond.FirstUseEver;
        Position = new Vector2(200, 80);

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(200, 60),
            MaximumSize = new Vector2(200, 60)
        };

        Flags |= ImGuiWindowFlags.NoDecoration;
        Flags |= ImGuiWindowFlags.NoResize;
        Flags |= ImGuiWindowFlags.NoCollapse;

        RespectCloseHotkey = false;
        
        chatLinkPayload = Service.PluginInterface.AddChatLinkHandler(0, OnChatLinkClick);
    }

    private void OnChatLinkClick(uint cmdId, SeString msg) {
        plugin.Window.IsOpen = !plugin.Window.IsOpen;
        if (msg.Payloads.ElementAtOrDefault(2) is TextPayload p)
            foreach (var deaths in plugin.DeathsPerPlayer.Values)
                if (deaths.FirstOrDefault()?.PlayerName == p.Text) {
                    plugin.Window.SelectedPlayer = deaths[0].PlayerId;
                    plugin.Window.SelectedDeath = 0;
                    break;
                }
    }

    public override void Draw() {
        var elapsed = (DateTime.Now - popupDeath?.TimeOfDeath)?.TotalSeconds;
        if (!plugin.Window.IsOpen && elapsed < 30) {
            var label = $"Show Death Recap ({30 - elapsed:N0}s)";
            if (popupDeath?.PlayerName is { } playerName)
                label = AppendCenteredPlayerName(label, playerName);

            if (ImGui.Button(label, new Vector2(-1, -1))) {
                plugin.Window.IsOpen = true;
                if (popupDeath?.PlayerId is { } id)
                    plugin.Window.SelectedPlayer = id;

                popupDeath = null;
                IsOpen = false;
            }
        }
        else
        {
            IsOpen = false;
        }
    }

    private static string AppendCenteredPlayerName(string label, string pname) {
        var length = ImGui.CalcTextSize(label).X;
        var spclength = ImGui.CalcTextSize(" ").X;
        var namelength = ImGui.CalcTextSize(pname).X;
        var spccount = (int)Math.Round((namelength - length) / 2f / spclength);
        if (spccount == 0)
            return label + "\n" + pname;
        if (spccount > 0) {
            var strbld = new StringBuilder(spccount * 2 + label.Length + pname.Length + 1);
            strbld.Append(' ', spccount);
            strbld.Append(label);
            strbld.Append(' ', spccount);
            strbld.Append('\n');
            strbld.Append(pname);
            return strbld.ToString();
        } else {
            var strbld = new StringBuilder(-spccount * 2 + label.Length + pname.Length + 1);
            strbld.Append(label);
            strbld.Append('\n');
            strbld.Append(' ', -spccount);
            strbld.Append(pname);
            strbld.Append(' ', -spccount);
            return strbld.ToString();
        }
    }

    public void DisplayDeath(Death death) {
        var displayType = plugin.ConditionEvaluator.GetNotificationType(death.PlayerId);
        switch (displayType) {
            case NotificationStyle.Popup:
                popupDeath = death;
                IsOpen = true;
                break;
            case NotificationStyle.Chat:
                var chatMsg = HasAuthor(plugin.Configuration.ChatType)
                    ? new SeString(chatLinkPayload, new TextPayload("has died "), new UIForegroundPayload(710), new TextPayload("[ Show Death Recap ]"),
                        new UIForegroundPayload(0), RawPayload.LinkTerminator)
                    : new SeString(chatLinkPayload, new UIForegroundPayload(1), new TextPayload(death.PlayerName), new UIForegroundPayload(0),
                        new TextPayload(" has died "), new UIForegroundPayload(710), new TextPayload("[ Show Death Recap ]"), new UIForegroundPayload(0),
                        RawPayload.LinkTerminator);
                Service.ChatGui.PrintChat(new XivChatEntry { Message = chatMsg, Type = plugin.Configuration.ChatType, Name = death.PlayerName });
                break;
            case NotificationStyle.OpenDeathRecap:
                plugin.Window.IsOpen = true;
                plugin.Window.SelectedPlayer = death.PlayerId;
                plugin.Window.SelectedDeath = 0;
                break;
        }
    }

    private static bool HasAuthor(XivChatType chatType) =>
        chatType switch {
            XivChatType.None => false,
            XivChatType.Debug => false,
            XivChatType.Urgent => false,
            XivChatType.Notice => false,
            XivChatType.StandardEmote => false,
            XivChatType.Echo => false,
            XivChatType.SystemError => false,
            XivChatType.SystemMessage => false,
            XivChatType.GatheringSystemMessage => false,
            XivChatType.ErrorMessage => false,
            XivChatType.RetainerSale => false,
            _ => true
        };
}
