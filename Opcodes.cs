namespace DeathRecap {
    public enum Opcodes : ushort {
        StatusEffectList = 0x31D,
        StatusEffectList2 = 0x31E,
        StatusEffectList3 = 0x357,
        BossStatusEffectList = 0x11A,
        Ability1 = 0x21b,
        Ability8 = 0x30e,
        Ability16 = 0x153,
        Ability24 = 0xe1,
        Ability32 = 0x356,
        ActorCast = 0x6F,
        EffectResult = 0x30B,
        EffectResultBasic = 0x3A3,
        ActorControl = 0x2e7,
        ActorControlSelf = 0x28f,
        ActorControlTarget = 0x399,
        UpdateHpMpTp = 0x94,
        PlayerSpawn = 0x2BC,
        NpcSpawn = 0x12F,
        NpcSpawn2 = 0x20A,
        ActorMove = 0x366,
        ActorSetPos = 0x23A,
        ActorGauge = 0x1BE,
        PresetWaymark = 0x75,
        Waymark = 0x15D,
        SystemLogMessage = 0x191
    }
}
