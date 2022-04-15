using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Conditions;

namespace DeathRecap {
    public class ConditionEvaluator {
        private readonly HashSet<uint> partyLookup = new(8);
        private readonly DeathRecapPlugin plugin;
        private DateTime partyLookupTime = DateTime.UnixEpoch;

        public ConditionEvaluator(DeathRecapPlugin plugin) {
            this.plugin = plugin;
        }

        private bool LookupPartyMember(uint actorId) {
            if ((DateTime.Now - partyLookupTime).TotalSeconds > 10) {
                partyLookup.Clear();
                for (var i = 0; i < 8; i++)
                    if (Service.PartyList[i]?.ObjectId is { } id)
                        partyLookup.Add(id);
                    else
                        break;
                partyLookupTime = DateTime.Now;
            }

            return partyLookup.Contains(actorId);
        }

        public bool ShouldCapture(uint actorId) {
            if (plugin.Configuration.CaptureOthers)
                return true;

            if (plugin.Configuration.CaptureSelf && actorId == Service.ObjectTable[0]?.ObjectId)
                return true;

            if (plugin.Configuration.CaptureParty && LookupPartyMember(actorId))
                return true;

            return false;
        }

        public NotificationStyle GetNotificationType(uint actorId) {
            if (actorId == Service.ObjectTable[0]?.ObjectId) {
                if (!plugin.Configuration.SelfNotificationOnlyInstances || Service.Condition[ConditionFlag.BoundByDuty])
                    return plugin.Configuration.SelfNotification;

                return NotificationStyle.None;
            }

            if (LookupPartyMember(actorId)) {
                if (!plugin.Configuration.PartyNotificationOnlyInstances || Service.Condition[ConditionFlag.BoundByDuty])
                    return plugin.Configuration.PartyNotification;

                return NotificationStyle.None;
            }

            if (!plugin.Configuration.OthersNotificationOnlyInstances || Service.Condition[ConditionFlag.BoundByDuty])
                return plugin.Configuration.OthersNotification;

            return NotificationStyle.None;
        }
    }
}
