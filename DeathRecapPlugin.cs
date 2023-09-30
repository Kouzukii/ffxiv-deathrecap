using System;
using System.Collections.Generic;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using DeathRecap.Events;
using DeathRecap.UI;

namespace DeathRecap;

public class DeathRecapPlugin : IDalamudPlugin {
    public DeathRecapWindow Window { get; }

    public ConfigWindow ConfigWindow { get; }

    public Configuration Configuration { get; }

    public ConditionEvaluator ConditionEvaluator { get; }

    public CombatEventCapture CombatEventCapture { get; }

    public NotificationHandler NotificationHandler { get; }

    public WindowSystem WindowSystem { get; }

    public Dictionary<uint, List<Death>> DeathsPerPlayer { get; } = new();

    private DateTime lastClean = DateTime.Now;

    public DeathRecapPlugin(DalamudPluginInterface pluginInterface) {
        Service.Initialize(pluginInterface);

        Configuration = Configuration.Get(pluginInterface);
        Window = new DeathRecapWindow(this);
        ConfigWindow = new ConfigWindow(this);
        ConditionEvaluator = new ConditionEvaluator(this);
        CombatEventCapture = new CombatEventCapture(this);
        NotificationHandler = new NotificationHandler(this);
        WindowSystem = new WindowSystem("DeathRecap");

        WindowSystem.AddWindow(Window);
        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(NotificationHandler);

        pluginInterface.UiBuilder.Draw += () => WindowSystem.Draw();
        pluginInterface.UiBuilder.OpenMainUi += () => Window.Toggle();
        pluginInterface.UiBuilder.OpenConfigUi += () => ConfigWindow.Toggle();
        Service.Framework.Update += FrameworkOnUpdate;
        var commandInfo = new CommandInfo((_, _) => Window.Toggle()) { HelpMessage = "Open the death recap window" };
        Service.CommandManager.AddHandler("/deathrecap", commandInfo);
        Service.CommandManager.AddHandler("/dr", commandInfo);

#if DEBUG
        try {
            DummyData.AddDummyData(this);
        } catch (Exception e) {
            Service.PluginLog.Error(e, "Failed to add dummy data");
        }
#endif
    }

    private void FrameworkOnUpdate(IFramework framework) {
#if !DEBUG
        var now = DateTime.Now;
        if ((now - lastClean).TotalSeconds >= 10) {
            CombatEventCapture.CleanCombatEvents();
            lastClean = now;
        }
#endif
    }


    public void Dispose() {
        CombatEventCapture.Dispose();
        Service.Framework.Update -= FrameworkOnUpdate;
        Service.CommandManager.RemoveHandler("/deathrecap");
        Service.CommandManager.RemoveHandler("/dr");
    }
}
