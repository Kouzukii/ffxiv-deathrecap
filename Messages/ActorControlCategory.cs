namespace DeathRecap.Messages {
    public enum ActorControlCategory : ushort {
        HoTDoT = 0x17,
        CancelAbility = 0x0f,
        Death = 0x06,
        TargetIcon = 0x22,
        Tether = 0x23,
        GainEffect = 0x14,
        LoseEffect = 0x15,
        UpdateEffect = 0x16,
        Targetable = 0x36,
        DirectorUpdate = 0x6d,
        SetTargetSign = 0x1f6,
        LimitBreak = 0x1f9
    }
}
