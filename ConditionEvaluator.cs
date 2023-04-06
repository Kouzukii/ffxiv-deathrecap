using Dalamud.Game.ClientState.Conditions;
using DeathRecap.UI;

namespace DeathRecap;

public class ConditionEvaluator {
    private readonly DeathRecapPlugin plugin;

    public ConditionEvaluator(DeathRecapPlugin plugin) {
        this.plugin = plugin;
    }

    private static bool LookupPartyMember(uint actorId) {
        for (var i = 0; i < 8; i++)
            if (Service.PartyList[i]?.ObjectId is { } id)
                if (actorId == id)
                    return true;
        return false;
    }

    public bool ShouldCapture(uint actorId) {
        if (plugin.Configuration.Others.Capture)
            return true;

        if (plugin.Configuration.Self.Capture && actorId == Service.ObjectTable[0]?.ObjectId)
            return true;

        if (plugin.Configuration.Party.Capture && LookupPartyMember(actorId))
            return true;

        return false;
    }

    public NotificationStyle GetNotificationType(uint actorId) {
        if (actorId == Service.ObjectTable[0]?.ObjectId) {
            if (plugin.Configuration.Self.OnlyInstances && !Service.Condition[ConditionFlag.BoundByDuty])
                return NotificationStyle.None;

            if (plugin.Configuration.Self.DisableInPvp && Service.ClientState.IsPvP)
                return NotificationStyle.None;

            return plugin.Configuration.Self.NotificationStyle;
        }

        if (LookupPartyMember(actorId)) {
            if (plugin.Configuration.Party.OnlyInstances && !Service.Condition[ConditionFlag.BoundByDuty])
                return NotificationStyle.None;

            if (plugin.Configuration.Party.DisableInPvp && Service.ClientState.IsPvP)
                return NotificationStyle.None;

            return plugin.Configuration.Party.NotificationStyle;
        }

        if (plugin.Configuration.Others.OnlyInstances && !Service.Condition[ConditionFlag.BoundByDuty])
            return NotificationStyle.None;

        if (plugin.Configuration.Others.DisableInPvp && Service.ClientState.IsPvP)
            return NotificationStyle.None;

        return plugin.Configuration.Others.NotificationStyle;
    }
}
