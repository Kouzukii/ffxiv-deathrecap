using System;
using System.Collections.Generic;

namespace DeathRecap {
    public record Death {
        public uint PlayerId { get; internal init; }
        public string PlayerName { get; internal init; } = null!;
        public DateTime TimeOfDeath { get; internal init; }

        public List<CombatEvent> Events { get; internal init; } = null!;

        public string Title {
            get {
                var timeSpan = DateTime.Now.Subtract(TimeOfDeath);

                if (timeSpan <= TimeSpan.FromSeconds(60))
                    return $"{timeSpan.Seconds} seconds ago";

                if (timeSpan <= TimeSpan.FromMinutes(60))
                    return timeSpan.Minutes > 1 ? $"{timeSpan.Minutes} minutes ago" : "about a minute ago";

                return timeSpan.Hours > 1 ? $"{timeSpan.Hours} hours ago" : "about an hour ago";
            }
        }
    }
}
