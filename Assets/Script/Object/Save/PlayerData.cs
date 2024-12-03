using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    public string playerName; //キャラクターの名前。六人分を想定。ここで誰のデータかを判別する
    public int level;
    public int experience;

    public int hp;
    public int mp;
    public int san;
    //public int confuse;

    //能力値
    public int strength;
    public int inteligence;
    public int dexterity;
    public int power;
    public int constitution;
    public int appearance;
    public int size;
    public int education;
    // 必要に応じて他のステータスを追加

    //技能

    //インベントリーゾーン
    public List<FeoData> feos;
    public List<ArtsData> arts;
    public List<CoreData> cores;
    public List<ItemData> inventory; // その他のアイテム

    //装備してるやつ
    public string equipFeo;
    public List<ArtsData> equipArts;
    public List<CoreData> equipCores;
}