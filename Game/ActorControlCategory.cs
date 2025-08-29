namespace DeathRecap.Game;

public enum ActorControlCategory : uint {
    Death = 0x6,
    CancelAbility = 0xF,
    GainEffect = 0x14,
    LoseEffect = 0x15,
    UpdateEffect = 0x16,
    TargetIcon = 0x22,
    Tether = 0x23,
    FallDamage = 0x2D,
    Targetable = 0x36,
    DirectorUpdate = 0x6D,
    SetTargetSign = 0x1F6,
    LimitBreak = 0x1F9,
    MedKit = 0x5DC, //floor pots in Crystalline Conflict
    HoT = 0x604,
    DoT = 0x605
}
