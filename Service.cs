using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace DeathRecap;

#pragma warning disable 8618
// ReSharper disable UnusedAutoPropertyAccessor.Local
internal class Service {
    [PluginService]
    internal static IDalamudPluginInterface PluginInterface { get; private set; }

    [PluginService]
    internal static ICommandManager CommandManager { get; private set; }

    [PluginService]
    internal static IDataManager DataManager { get; private set; }

    [PluginService]
    internal static ITextureProvider TextureProvider { get; private set; }

    [PluginService]
    internal static IChatGui ChatGui { get; private set; }

    [PluginService]
    internal static IObjectTable ObjectTable { get; private set; }

    [PluginService]
    internal static IPartyList PartyList { get; private set; }

    [PluginService]
    internal static ICondition Condition { get; private set; }

    [PluginService]
    internal static IClientState ClientState { get; private set; }

    [PluginService]
    internal static IFramework Framework { get; private set; }

    [PluginService]
    internal static IPluginLog PluginLog { get; private set; }

    [PluginService]
    internal static IGameInteropProvider GameInteropProvider { get; private set; }

    internal static void Initialize(IDalamudPluginInterface pluginInterface) {
        pluginInterface.Create<Service>();
    }
}
#pragma warning restore 8618
