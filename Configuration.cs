using System;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace DeathRecap {
    [Serializable]
    public class Configuration : IPluginConfiguration {
        // the below exist just to make saving less cumbersome

        [NonSerialized] private DalamudPluginInterface pluginInterface = null!;

        public bool CaptureSelf { get; set; } = true;
        public bool CaptureParty { get; set; } = true;
        public bool CaptureOthers { get; set; } = false;
        public NotificationStyle SelfNotification { get; set; } = NotificationStyle.Popup;
        public NotificationStyle PartyNotification { get; set; } = NotificationStyle.Chat;
        public NotificationStyle OthersNotification { get; set; } = NotificationStyle.None;
        public bool SelfNotificationOnlyInstances { get; set; } = false;
        public bool PartyNotificationOnlyInstances { get; set; } = false;
        public bool OthersNotificationOnlyInstances { get; set; } = true;
        public bool ShowTip { get; set; } = true;
        public int KeepCombatEventsForSeconds { get; set; } = 60;
        public int KeepDeathsForMinutes { get; set; } = 60;
        public int Version { get; set; } = 0;

        public static Configuration Get(DalamudPluginInterface pluginInterface) {
            var config = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            config.pluginInterface = pluginInterface;
            return config;
        }

        public void Save() {
            pluginInterface.SavePluginConfig(this);
        }
    }
}
