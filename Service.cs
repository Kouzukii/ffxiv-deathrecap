using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Network;
using Dalamud.IoC;
using Dalamud.Plugin;

#pragma warning disable 8618
namespace DeathRecap {
    // ReSharper disable UnusedAutoPropertyAccessor.Local
    internal class Service {
        internal static void Initialize(DalamudPluginInterface pluginInterface) => pluginInterface.Create<Service>();

        [PluginService]
        [RequiredVersion("1.0")]
        internal static DalamudPluginInterface PluginInterface { get; private set; }

        [PluginService]
        [RequiredVersion("1.0")]
        internal static CommandManager CommandManager { get; private set; }

        [PluginService]
        [RequiredVersion("1.0")]
        internal static GameNetwork GameNetwork { get; private set; }

        [PluginService]
        [RequiredVersion("1.0")]
        internal static DataManager DataManager { get; private set; }

        [PluginService]
        [RequiredVersion("1.0")]
        internal static ChatGui ChatGui { get; private set; }

        [PluginService]
        [RequiredVersion("1.0")]
        internal static ClientState ClientState { get; private set; }

        [PluginService]
        [RequiredVersion("1.0")]
        internal static ObjectTable ObjectTable { get; private set; }
    }
}
#pragma warning restore 8618