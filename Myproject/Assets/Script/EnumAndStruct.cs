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
    Fog
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
    SceneChange, //
    GotoLobby //
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
