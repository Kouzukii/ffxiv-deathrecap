namespace DeathRecap {
    public enum Opcodes : ushort {        
        StatusEffectList = 0x0275,
        StatusEffectList2 = 0x1ff,
        StatusEffectList3 = 0x2af,
        BossStatusEffectList = 0x7e,
        Ability1 = 0x3c7,
        Ability8 = 0x149,
        Ability16 = 0xc1,
        Ability24 = 0x213,
        Ability32 = 0x38b,
        ActorCast = 0x01F4,
        EffectResult = 0x021A,
        EffectResultBasic = 0x16A,
        ActorControl = 0x0202,
        ActorControlSelf = 0x0301,
        ActorControlTarget = 0x0333,
        UpdateHpMpTp = 0x0240,
        PlayerSpawn = 0x03DC,
        NpcSpawn = 0x032F,
        NpcSpawn2 = 0x8f,
        ActorMove = 0x03CB,
        ActorSetPos = 0x033C,
        ActorGauge = 0x0374,
        PresetWaymark = 0x0074,
        Waymark = 0x03BA
    }
}
