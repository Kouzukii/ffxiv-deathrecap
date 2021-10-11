namespace DeathRecap.Messages {
    public enum ActorControlCategory : ushort {
        HoTDoT = 23,
        CancelAbility = 0xF,
        Death = 6,
        TargetIcon = 34,
        Tether = 35,
        GainEffect = 20,
        LoseEffect = 21,
        UpdateEffect = 22,
        Targetable = 54,
        DirectorUpdate = 109,
        SetTargetSign = 502,
        LimitBreak = 505
    }
}