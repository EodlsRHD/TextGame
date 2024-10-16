using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

[Serializable]
public class Tutorial_answer
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

    public string name = string.Empty;
    public string description = string.Empty;

    public short coolDown = 0;
    public short useMp = 0;

    public StrengtheningTool tool = null;
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

    public Stat_In_De hp = null;
    public Stat_In_De mp = null;
    public Stat_In_De ap = null;
    public Stat_In_De exp = null;
    public Stat_In_De coin = null;
    public Stat_In_De attack = null;
    public Stat_In_De defence = null;

    public eDir dir = eDir.Non;

    public eStrengtheningTool needStatus = eStrengtheningTool.Non;
    public eStrengtheningTool grantStatus = eStrengtheningTool.Non;

    public short range = 0;
    public float value = 0;

    public bool revealMap = false;
}

[Serializable]
public class Stat_In_De
{
    public short value = 0;
    public short percent = 0;

    public Stat_In_De(short v, short p)
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
        get { return (short)(stats.exp.maximum * 0.7f * level); }
    }

    public CreatureData data = null;

    public short level = 1; // 1레벨 (경험치 * 0.4) * 레벨수

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

    public bool recovery = false;
    public byte recoveryCount = 0;

    public bool useSkill = false;
    public List<short> skillIndexs = null;
    public List<short> itemIndexs = null;

    public List<Duration> skill_Duration = null;
    public List<Duration> item_Duration = null;
    public List<Skill_CoolDown> coolDownSkill = null;
    public List<AbnormalStatus> abnormalStatuses = null;

    public int currentNodeIndex = 0;
}

[Serializable]
public class CreatureStats
{
    public CreatureStat coin = null;
    public CreatureStat hp = null;
    public CreatureStat mp = null;
    public CreatureStat ap = null;
    public CreatureStat exp = null;
    public CreatureStat attack = null;
    public CreatureStat defence = null;
    public CreatureStat vision = null;
    public CreatureStat attackRange = null;

    public void Maximum()
    {
        hp.Maximum();
        mp.Maximum();
        ap.Maximum();
    }

    public bool PlusExp(short value, short maximumExp)
    {
        exp.currnet += (short)(value + (value * 0.1f * exp.percent));

        if(exp.currnet > maximumExp)
        {
            exp.currnet -= maximumExp;

            return true;
        }

        return false;
    }

    public bool MinusExp(short value)
    {
        if (exp.currnet - value < 0)
        {
            return false;
        }

        exp.currnet -= value;
        return true;
    }

    public void PlusCoin(short value)
    {
        coin.currnet += (short)(value + (value * 0.1f * coin.percent));
    }

    public bool MinusCoin(short value)
    {
        if (coin.currnet - value < 0)
        {
            return false;
        }

        coin.currnet -= value;
        return true;
    }
}

[Serializable]
public class CreatureStat
{
    public short defult = 0;
    public short point = 1;
    public short plus = 0;
    public short percent = 0;
    public short currnet = 0;

    public short maximum
    {
        get { return (short)((defult * point) + ((defult * point) * 0.1f * percent)); }
    }

    public CreatureStat(short defult, short point, short plus, short percent)
    {
        this.defult = defult;
        this.point = point;
        this.plus = plus;
        this.percent = percent;
        this.plus = plus;
        currnet = (short)(this.defult * this.point);
    }

    public void Maximum()
    {
        currnet = maximum;
    }

    public void PlusCurrent(short value)
    {
        currnet += value;

        if(currnet > maximum)
        {
            currnet = maximum;
        }
    }

    public void MinusCurrnet(short value)
    {
        currnet -= value;

        if (currnet < 0)
        {
            currnet = 0;
        }
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
    public int statusCount = 0;

    public float value = 0;
}