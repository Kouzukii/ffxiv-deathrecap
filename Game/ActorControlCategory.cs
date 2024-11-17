namespace DeathRecap.Game;

public enum ActorControlCategory : ushort {
    Death = 0x6,
    CancelAbility = 0xF,
    GainEffect = 0x14,
    LoseEffect = 0x15,
    UpdateEffect = 0x16,
    TargetIcon = 0x22,
    Tether = 0x23,
    Targetable = 0x36,
    DirectorUpdate = 0x6D,
    SetTargetSign = 0x1F6,
    LimitBreak = 0x1F9,
    HoT = 0x604,
    DoT = 0x605
}
