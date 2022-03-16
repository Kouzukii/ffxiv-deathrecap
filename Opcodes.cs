namespace DeathRecap {
    public enum Opcodes : ushort {
        StatusEffectList = 0x275,
        StatusEffectList2 = 0x12f,
        StatusEffectList3 = 0x1d6,
        BossStatusEffectList = 0x114,
        Ability1 = 0x35e,
        Ability8 = 0x2ba,
        Ability16 = 0x10d,
        Ability24 = 0xf7,
        Ability32 = 0x1ca,
        ActorCast = 0x1f4,
        EffectResult = 0x21a,
        EffectResultBasic = 0x16a,
        ActorControl = 0x202,
        ActorControlSelf = 0x301,
        ActorControlTarget = 0x333,
        UpdateHpMpTp = 0x240,
        PlayerSpawn = 0x268,
        NpcSpawn = 0x32f,
        NpcSpawn2 = 0x380,
        ActorMove = 0x3cb,
        ActorSetPos = 0x33c,
        ActorGauge = 0x374,
        PresetWaymark = 0x74,
        Waymark = 0x3ba,
        SystemLogMessage = 0x30b
    }
}