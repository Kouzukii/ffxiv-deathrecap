using System;
using System.Linq;
using System.Numerics;
using System.Text;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface;
using ImGuiNET;

namespace DeathRecap {
    public class NotificationHandler {
        private readonly DalamudLinkPayload chatLinkPayload;
        private readonly DeathRecapPlugin plugin;
        private Death? popupDeath;
        private bool windowWasMoved;

        public NotificationHandler(DeathRecapPlugin plugin) {
            this.plugin = plugin;

            chatLinkPayload = Service.PluginInterface.AddChatLinkHandler(0, OnChatLinkClick);
        }

        private void OnChatLinkClick(uint cmdId, SeString msg) {
            plugin.Window.ShowDeathRecap = true;
            if (msg.Payloads.ElementAtOrDefault(2) is TextPayload p)
                foreach (var deaths in plugin.DeathsPerPlayer.Values)
                    if (deaths.FirstOrDefault()?.PlayerName == p.Text)
                        plugin.Window.SelectedPlayer = deaths[0].PlayerId;
        }

        public void Draw() {
            var elapsed = (DateTime.Now - popupDeath?.TimeOfDeath)?.TotalSeconds;
            if (!plugin.Window.ShowDeathRecap && elapsed < 30) {
                var viewport = ImGui.GetMainViewport();
                var initialPos = new Vector2(viewport.WorkPos.X + viewport.WorkSize.X / 2 - 100 * ImGuiHelpers.GlobalScale,
                    viewport.WorkPos.Y + viewport.WorkSize.Y / 2 - 40 * ImGuiHelpers.GlobalScale);
                ImGui.SetNextWindowPos(initialPos, ImGuiCond.FirstUseEver);
                ImGui.SetNextWindowSize(ImGuiHelpers.ScaledVector2(200, 80));
                if (ImGui.Begin(windowWasMoved ? "###deathRecapPopup" : "(Drag me somewhere)###deathRecapPopup",
                        ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse)) {
                    windowWasMoved = ImGui.GetWindowPos() != initialPos;
                    var label = $"Show Death Recap ({30 - elapsed:N0}s)";
                    if (popupDeath?.PlayerName is { } playerName)
                        label = AppendCenteredPlayerName(label, playerName);

                    if (ImGui.Button(label, new Vector2(-1, -1))) {
                        plugin.Window.ShowDeathRecap = true;
                        if (popupDeath?.PlayerId is { } id)
                            plugin.Window.SelectedPlayer = id;

                        popupDeath = null;
                    }

                    ImGui.End();
                }
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
}
