using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace DeathRecap;

#pragma warning disable 8618
// ReSharper disable UnusedAutoPropertyAccessor.Local
internal class Service {
    [PluginService]
    [RequiredVersion("1.0")]
    internal static DalamudPluginInterface PluginInterface { get; private set; }

    [PluginService]
    [RequiredVersion("1.0")]
    internal static CommandManager CommandManager { get; private set; }

    [PluginService]
    [RequiredVersion("1.0")]
    internal static DataManager DataManager { get; private set; }

    [PluginService]
    [RequiredVersion("1.0")]
    internal static ITextureProvider TextureProvider { get; private set; }

    [PluginService]
    [RequiredVersion("1.0")]
    internal static ChatGui ChatGui { get; private set; }

    [PluginService]
    [RequiredVersion("1.0")]
    internal static ObjectTable ObjectTable { get; private set; }

    [PluginService]
    [RequiredVersion("1.0")]
    internal static PartyList PartyList { get; private set; }

    [PluginService]
    [RequiredVersion("1.0")]
    internal static Condition Condition { get; private set; }

    [PluginService]
    [RequiredVersion("1.0")]
    internal static ClientState ClientState { get; private set; }

    [PluginService]
    [RequiredVersion("1.0")]
    internal static Framework Framework { get; private set; }

    internal static void Initialize(DalamudPluginInterface pluginInterface) {
        pluginInterface.Create<Service>();
    }
}
#pragma warning restore 8618
