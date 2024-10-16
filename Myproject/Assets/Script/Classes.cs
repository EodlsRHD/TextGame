using System.Collections.Generic;
using System;
using UnityEngine;

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
public class AbnormalStatus
{
    public eStrengtheningTool currentStatus = eStrengtheningTool.Non;
    public int statusCount = 0;

    public float value = 0;
}

[Serializable]
public class Skill_CoolDown
{
    public int id = 0;
    public string name = string.Empty;

    public int coolDown = 0;
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
public class Duration
{
    public int ID = 0;
    public string name = string.Empty;

    public int remaindDuration = 0;
    public int remaindCooldown = 0;

    public eStats stats = eStats.Non;
    public bool isPercent = false;
    public short value;
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