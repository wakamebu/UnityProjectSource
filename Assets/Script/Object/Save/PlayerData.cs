using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    public string playerName; //キャラクターの名前。六人分を想定。ここで誰のデータかを判別する
    public int level;
    public int experience; //獲得した魂の数
    public int usedSoul; //使用した魂の数

    //初期基礎ステータス
    public int hp;
    public int mp;
    public int san;
    public int confuse;
    public int ap;

    //ボーナス基礎ステータス
    public int bonusHP;
    public int bonusMP;
    public int bonusSan;
    public int bonusConfuse;

    //能力値(ここは初期値)
    public int strength;
    public int inteligence;
    public int dexterity;
    public int power;
    public int constitution;
    public int appearance;
    public int size;
    public int education;

    //ボーナス能力値
    public int bonusStrength;
    public int bonusInteligence;
    public int bonusDexterity;
    public int bonusPower;
    public int bonusConstitution;
    public int bonusAppearance;
    public int bonusSize;
    public int bonusEducation;

    //ボーナスポイント管理
    public int usedAttributeBonus;
    public int usedAbilityBonus;

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

    //立ち絵
    //public Sprite characterSprite;
}