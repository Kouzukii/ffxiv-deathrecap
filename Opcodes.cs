namespace DeathRecap {
    public enum Opcodes : ushort {        
        StatusEffectList = 0xbc,
        StatusEffectList2 = 0x1ff,
        StatusEffectList3 = 0x2af,
        BossStatusEffectList = 0x7e,
        Ability1 = 0x3c7,
        Ability8 = 0x149,
        Ability16 = 0xc1,
        Ability24 = 0x213,
        Ability32 = 0x38b,
        ActorCast = 0x104,
        EffectResult = 0xde,
        EffectResultBasic = 0x2d9,
        ActorControl = 0x22f,
        ActorControlSelf = 0x6b,
        ActorControlTarget = 0x191,
        UpdateHpMpTp = 0x2c9,
        PlayerSpawn = 0x142,
        NpcSpawn = 0x32c,
        NpcSpawn2 = 0x8f,
        ActorMove = 0x370,
        ActorSetPos = 0x395,
        ActorGauge = 0x3b5,
        PresetWaymark = 0x1fe,
        Waymark = 0x67
    }
}