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
    Fail
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

public enum eSound
{
    Non = -1,
    ButtonPress,
    ButtonUp,
    ButtonClick,
    PageIn,
    PageOut,
    Attack,
    Blocked,
    Hit,
    LevelUp,
    GetTrophy,
    UseItem,
    UseSkill
}
