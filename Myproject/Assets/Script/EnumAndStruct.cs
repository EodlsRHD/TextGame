public enum eScene
{
    SplashScene = 0,
    Ui,
    Lobby,
    Game
}

public enum eControl
{
    Non = -1,
    Up,
    Left,
    Right,
    Down,
    Attack,
    Defence,
    Skill,
    Rest,
    Bag,
    SearchNearby
}

public enum eRoundClear
{
    Non = -1,
    First,
    Load,
    Success,
    Fail,
    Restart
}

public enum eCreature
{
    Non = -1,
    Monster,
    Item,
    Shop,
    Exit,
    Blocker,
    Player,
    Bonfire,
    Guide,
}

public enum eMapObject
{
    Non = -1,
    Monster,
    Item,
    Shop,
    Exit,
    Blocker,
    Player,
    Fog,
    Bonfire,
    UseBonfire,
    Guide,
    Ground
}


public enum eStats
{
    Non = -1,
    Level,
    HP,
    MP,
    AP,
    EXP,
    Attack,
    Defence,
    Vision,
    AttackRange,
    Coin
}

public enum eCardShape
{
    Non = -1,
    Clob,
    Heart,
    Diamond,
    Spade
}

public enum eRankings
{
    Non = -1,
    HighCard,
    OnePair,
    TwoPair,
    ThreeofaKind,
    Straight,
    Flush,
    FullHouse,
    FourofaKind,
    StraightFlush,
    RoyalStraightFlush
}

public enum eBattleAction
{
    Non = -1,
    Bat,
    Raise,
    ALLin,
    Fold
}

public enum eWinorLose
{
    Non = -1,
    Win,
    Lose,
    Draw
}

public enum eEffect_IncreaseDecrease
{
    Non = -1,
    Increase,
    Decrease,
    ALLDecrease
}

public enum eSfx
{
    Non = -1,
    Error, //
    Confirm, //
    ButtonPress, //
    Window, //
    Attack, //
    Blocked, //
    Hit_light,//
    Hit_hard,//
    LevelUp,
    Coin, //
    Item, 
    ChestOpen,
    UseItem, //
    UseSkill,
    ExitOpen, //
    Enter, //
    ShopEnter, //
    Death, //
    RoundSuccess,
    RoundFail,
    TurnPage, //
    GotoLobby, //
    Bonfire,
    MenuOpen,
    MenuClose,
    Map
}

public enum eBgm
{
    Non = -1,
    Lobby,
    Ingame,
    Battle,
    Shop
}

public enum eLanguage
{
    Non = -1,
    Kor,
    Eng
}

public enum eBennerAd
{
    Non = -1,
    Gamemenu,
    Popup
}

public enum eDir
{
    Non = -1,
    X,
    Y
}

public enum Ease
{
    Unset = 0,
    Linear = 1,
    InSine = 2,
    OutSine = 3,
    InOutSine = 4,
    InQuad = 5,
    OutQuad = 6,
    InOutQuad = 7,
    InCubic = 8,
    OutCubic = 9,
    InOutCubic = 10,
    InQuart = 11,
    OutQuart = 12,
    InOutQuart = 13,
    InQuint = 14,
    OutQuint = 15,
    InOutQuint = 16,
    InExpo = 17,
    OutExpo = 18,
    InOutExpo = 19,
    InCirc = 20,
    OutCirc = 21,
    InOutCirc = 22,
    InElastic = 23,
    OutElastic = 24,
    InOutElastic = 25,
    InBack = 26,
    OutBack = 27,
    InOutBack = 28,
    InBounce = 29,
    OutBounce = 30,
    InOutBounce = 31,
    Flash = 32,
    InFlash = 33,
    OutFlash = 34,
    InOutFlash = 35,
}

public enum eTutorialQuest
{
    Non = -1,
    Attack
}

public enum eStatus
{
    Non = -1,
    UnableAct,
    ContinuousDamage, 
    Recovery,
    Slowdown, 
    Incubation, 
    Camouflage, 
    split, 
    BloodSucking, 
    Hardness,
    ReductionHalf, 
    AttackBlocking, 
    Stealth,
    SkillBlocking,
    QuickAttack,
    Teleportation,
    Non_Teleportation,
    Resurrection,
    RecallMinion,
    SkillReflect,
    Remove_ContinuousDamage,
    Invincibility,
    OpenExit,
    FindStealth_Incubation_Camouflage
}

public enum eAttackDirection
{
    Non = -1,
    Up,
    Left,
    Right,
    Down,
    All,
    DesignateDirection,
    DesignateCoordination
}