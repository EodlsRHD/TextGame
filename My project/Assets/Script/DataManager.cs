using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;

public class DataManager : MonoBehaviour
{
    #region Creatures

    [Serializable]
    public class Creature_Data
    {
        public string name = string.Empty;
        public string description = string.Empty;
        public ushort coin = 0;

        public ushort hp = 0;
        public ushort mp = 0;
        public ushort ap = 0; // Active Point 
        public ushort attack = 0;
        public ushort defence = 0;

        public bool useSkill = false;
        public Skill_Data[] skillDatas = null;
    }

    [Serializable]
    public class User_Data
    {
        public Creature_Data data = null;

        public ushort exp = 0;
    }

    [Serializable]
    public class Skill_Data
    {
        public ushort index = 0;

        public string name = string.Empty;
        public string description = string.Empty;

        public ushort hp = 0;
        public ushort mp = 0;
        public ushort ap = 0;
        public ushort exp = 0;
        public ushort expPercentIncreased = 0;
        public ushort coin = 0;
        public ushort coinPercentIncreased = 0;

        public ushort attack = 0;
        public ushort attackPercentIncreased = 0;
        public ushort defence = 0;
        public ushort defencePercentIncreased = 0;
        public ushort duration = 0;
    }

    [Serializable]
    public class Item_Data
    {
        public ushort index = 0;

        public string name = string.Empty;
        public string description = string.Empty;
        public ushort price = 0;

        public ushort hp = 0;
        public ushort mp = 0;
        public ushort ap = 0;
        public ushort exp = 0;
        public ushort expPercentIncreased = 0;
        public ushort coin = 0;
        public ushort coinPercentIncreased = 0;

        public ushort attack = 0;
        public ushort attackPercentIncreased = 0;
        public ushort defence = 0;
        public ushort defencePercentIncreased = 0;
        public ushort duration = 0;
    }

    #endregion

    #region Map

    [Serializable]
    public class Map_Data
    {
        public Block_Data[] blockDatas = null;

        public Creature_Data userData = null;
        public Creature_Data[] monsterDatas = null;
    }

    [Serializable]
    public class Block_Data
    {
        public ushort x = 0;
        public ushort y = 0;

        public bool isWalkable = false;
        public bool isCreatureExist = false;
    }

    #endregion

    [Serializable]
    public class Save_Data
    {
        public ushort round = 0;

        public User_Data userData = null;
        public Map_Data mapData = null;
    }

    private Save_Data saveData = null;

    public void  Initialize()
    {
        this.gameObject.SetActive(true);
    }

    public void WriteSaveData()
    {

    }

    public void ReadSaveData()
    {
        
    }

    public void ChangeSaveData()
    {

    }

    public void CreateSaveData()
    {
        saveData = new Save_Data();
    }
}
