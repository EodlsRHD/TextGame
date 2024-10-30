using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class TutorialData
{
    public int id = 0;

    public string content = string.Empty;
    public List<TutorialAnswer> answers = null;

    public int isQuest = -1;

    public TutorialData(DataManager.Tutorial_Data data)
    {
        id = data.id;
        content = data.content;

        answers = new List<TutorialAnswer>();
        answers.AddRange(data.answers);

        isQuest = data.isQuest;
    }
}

[Serializable]
public class TutorialAnswer
{
    public string answer = string.Empty;
    public int next = 0;
}

[Serializable]
public class BlockImageTemplate
{
    public eMapObject type = eMapObject.Non;
    public Sprite sprite = null;

    public List<Sprite> sprites = null;
}

[Serializable]
public class SoundTemplate
{
    public eSfx type = eSfx.Non;
    public AudioClip clip = null;
    public List<AudioClip> clips = null;
}

[Serializable]
public class SkillData
{
    public short id = 0;

    public short level = 0;

    public string name = string.Empty;
    public string description = string.Empty;

    public short coolDown = 0;
    public short useMp = 0;

    public StrengtheningTool tool = null;

    #region Get

    public short GetStat_Value(eStats type)
    {
        int Level = level == 0 ? 1 : (int)(level * 0.33f);

        switch(type)
        {
            case eStats.HP:
                return (short)((Level) * tool.hp.value);

            case eStats.MP:
                return (short)((Level) * tool.mp.value);

            case eStats.AP:
                return (short)((Level) * tool.ap.value);

            case eStats.EXP:
                return (short)((Level) * tool.exp.value);

            case eStats.Coin:
                return (short)((Level) * tool.coin.value);

            case eStats.Attack:
                return (short)((Level) * tool.attack.value);

            case eStats.Defence:
                return (short)((Level) * tool.defence.value);
        }

        return 0;
    }

    public short GetStat_Percent(eStats type)
    {
        int Level = level == 0 ? 1 : (int)(level * 0.33f);

        switch(type)
        {
            case eStats.HP:
                return (short)((Level) * tool.hp.percent);

            case eStats.MP:
                return (short)((Level) * tool.mp.percent);

            case eStats.AP:
                return (short)((Level) * tool.ap.percent);

            case eStats.EXP:
                return (short)((Level) * tool.exp.percent);

            case eStats.Coin:
                return (short)((Level) * tool.coin.percent);

            case eStats.Attack:
                return (short)((Level) * tool.attack.percent);

            case eStats.Defence:
                return (short)((Level) * tool.defence.percent);
        }

        return 0;
    }

    public short Get_Value()
    {
        int Level = level == 0 ? 1 : (int)(level * 0.33f);

        return (short)(Level * tool.value);
    }

    #endregion
}

[Serializable]
public class ItemData
{
    public short id = 0;

    public string name = string.Empty;
    public string description = string.Empty;

    public short price = 0;

    public StrengtheningTool tool = null;

    public int currentNodeIndex = 0;
}

[Serializable]
public class StrengtheningTool
{
    public short duration = 0;

    public StrengtheningToolStat hp = null;
    public StrengtheningToolStat mp = null;
    public StrengtheningToolStat ap = null;
    public StrengtheningToolStat exp = null;
    public StrengtheningToolStat coin = null;
    public StrengtheningToolStat attack = null;
    public StrengtheningToolStat defence = null;

    public eDir dir = eDir.Non;

    public eStrengtheningTool needStatus = eStrengtheningTool.Non;
    public eStrengtheningTool grantStatus = eStrengtheningTool.Non;

    public short range = 0;
    public float value = 0;

    public bool revealMap = false;

    public eMaintain maintain = eMaintain.Non;
}

[Serializable]
public class StrengtheningToolStat
{
    public short value = 0;
    public short percent = 0;

    public StrengtheningToolStat(short v, short p)
    {
        value = v;
        percent = p;
    }
}

[Serializable]
public class UserData
{
    public CreatureStats stats
    {
        get { return data.stats; }
    }

    public short maximumEXP
    {
        get { return (short)(stats.exp.maximum  * level * 0.6f); }
    }

    public CreatureData data = null;

    public short level = 1;

    public void PlusExp(short value, Action<bool> onResultCallback)
    {
        bool isUp = stats.PlusExp(value, maximumEXP);

        level += (isUp == true) ? (short)1 : (short)0;

        onResultCallback?.Invoke(isUp);
    }
}

[Serializable]
public class CreatureData
{
    public short id = 0;
   
    public string name = string.Empty;
    public string description = string.Empty;
    public short spriteIndex = 0;

    public CreatureStats stats = null;

    public eStrengtheningTool defultStatus = eStrengtheningTool.Non;
    public short defultStatusValue = 0;

    public bool haveSkill = false;
    public List<short> skillIndexs = null;
    public List<short> itemIndexs = null;

    public List<Duration> skill_Duration = null;
    public List<Duration> item_Duration = null;
    public List<Skill_CoolDown> coolDownSkill = null;
    public List<AbnormalStatus> abnormalStatuses = null;

    public int currentNodeIndex = 0;
    public bool isDead = false;
}

[Serializable]
public class CreatureStats
{
    public short creatureLevel = 1;

    public CreatureStat coin = null;
    public CreatureStat hp = null;
    public CreatureStat mp = null;
    public CreatureStat ap = null;
    public CreatureStat exp = null;
    public CreatureStat attack = null;
    public CreatureStat defence = null;
    public CreatureStat vision = null;
    public CreatureStat attackRange = null;
    public CreatureStat evesion = null;

    public void Maximum()
    {
        hp.Maximum();
        mp.Maximum();
        ap.Maximum();
    }

    public bool PlusExp(short value, short maximumExp)
    {
        exp.current += (short)(value + (value * 0.1f * exp.percent));

        if(exp.current > maximumExp)
        {
            exp.current -= maximumExp;

            return true;
        }

        return false;
    }

    public void PlusCoin(short value)
    {
        coin.current += (short)(value + (value * 0.1f * coin.percent));
    }

    public bool MinusCoin(short value)
    {
        if (coin.current - value < 0)
        {
            return false;
        }

        coin.current -= value;
        return true;
    }

    public void Enforce()
    {
        coin.point += 1;
        hp.point += 1;
        mp.point += 1;
        ap.point += 1;
        exp.point += 1;
        attack.point += 1;
        defence.point += 1;
    }

    public void Reset()
    {
        coin.point = 1;
        hp.point = 1;
        mp.point = 1;
        ap.point = 1;
        exp.point = 1;
        attack.point = 1;
        defence.point = 1;
    }
}

[Serializable]
public class CreatureStat
{
    public short defult = 0;
    public short point = 1;
    public short plus = 0;
    public short percent = 0;
    public short current = 0;

    public short maximum
    {
        get { return (short)((defult * point) + ((defult * point) * 0.1f * percent) + plus); }
    }

    public CreatureStat(short defult, short point, short plus, short percent)
    {
        this.defult = defult;
        this.point = point;
        this.plus = plus;
        this.percent = percent;
        current = (short)(this.defult * this.point);
    }

    public void Maximum()
    {
        current = maximum;
    }

    public void PlusCurrent(short value)
    {
        current += value;

        if(current > maximum)
        {
            current = maximum;
        }
    }

    public void MinusCurrnet(short value)
    {
        current -= value;

        if (current < 0)
        {
            current = 0;
        }
    }

    public bool isUse(short value)
    {
        short Current = this.current;
        Current -= value;

        if(Current < 0)
        {
            return false;
        }

        return true;
    }
}

[Serializable]
public class Duration
{
    public int id = 0;
    public string name = string.Empty;

    public int remaindDuration = 0;
    public int remaindCooldown = 0;

    public eStats stats = eStats.Non;
    public eMaintain maintain = eMaintain.Non;
    public bool isPercent = false;
    public short value;
}

[Serializable]
public class Skill_CoolDown
{
    public int id = 0;
    public string name = string.Empty;

    public int coolDown = 0;
}

[Serializable]
public class AbnormalStatus
{
    public int id = 0;

    public eStrengtheningTool currentStatus = eStrengtheningTool.Non;
    public eMaintain maintain = eMaintain.Non;
    public int duration = 0;

    public float value = 0;
}