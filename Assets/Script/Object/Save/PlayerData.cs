using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    public string playerName; //キャラクターの名前。六人分を想定。ここで誰のデータかを判別する
    public int level;
    public int experience;
    public int usedSoul;

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

    //初期基礎ステータス
    public int strength;
    public int inteligence;
    public int dexterity;
    public int power;
    public int constitution;
    public int appearance;
    public int size;
    public int education;

    //ボーナス基礎ステータス
    public int bonusStrength;
    public int bonusInteligence;
    public int bonusDexterity;
    public int bonusPower;
    public int bonusConstitution;
    public int bonusAppearance;
    public int bonusSize;
    public int bonusEducation;

    //使用ボーナス
    public int usedAttributeBonus;
    public int usedAbilityBonus;

    //技能
    public Dictionary<string, SkillState> skillStates = new Dictionary<string, SkillState>();

    //アイテムとか
    public List<FeoData> feos;
    public List<ArtsData> arts;
    public List<CoreData> cores;
    public List<ItemData> inventory;

    //装備
    public string equipFeo;
    public List<ArtsData> equipArts;
    public List<CoreData> equipCores;

    //デフォルトコンストラクタ
    public PlayerData()
    {
        // リストとDictionaryの初期化
        skillStates = new Dictionary<string, SkillState>();
        feos = new List<FeoData>();
        arts = new List<ArtsData>();
        cores = new List<CoreData>();
        inventory = new List<ItemData>();
        equipArts = new List<ArtsData>();
        equipCores = new List<CoreData>();
    }

    //コンストラクタ
    public PlayerData(PlayerData original)
    {
        if (original == null)
        {
            Debug.LogError("PlayerDataのコピーコンストラクタにnullが渡されました");
            return;
        }

        playerName = original.playerName;
        level = original.level;
        experience = original.experience;
        usedSoul = original.usedSoul;
        usedAttributeBonus = original.usedAttributeBonus;
        usedAbilityBonus = original.usedAbilityBonus;

        strength = original.strength;
        dexterity = original.dexterity;
        inteligence = original.inteligence;
        constitution = original.constitution;
        power = original.power;
        appearance = original.appearance;
        size = original.size;
        education = original.education;

        bonusStrength = original.bonusStrength;
        bonusDexterity = original.bonusDexterity;
        bonusInteligence = original.bonusInteligence;
        bonusConstitution = original.bonusConstitution;
        bonusPower = original.bonusPower;
        bonusAppearance = original.bonusAppearance;
        bonusSize = original.bonusSize;
        bonusEducation = original.bonusEducation;

        hp = original.hp;
        mp = original.mp;
        san = original.san;
        confuse = original.confuse;
        ap = original.ap;

        // DictionaryのDeepコピー
        skillStates = new Dictionary<string, SkillState>();
        if (original.skillStates != null)
        {
            foreach(var pair in original.skillStates)
            {
                skillStates[pair.Key] = new SkillState
                {
                    isLearned = pair.Value.isLearned,
                    growValue = pair.Value.growValue
                };
            }
        }

        // リストのDeepコピー（要素がクラスならば同様にDeepコピーが必要）
        feos = original.feos != null ? new List<FeoData>(original.feos) : new List<FeoData>();
        arts = original.arts != null ? new List<ArtsData>(original.arts) : new List<ArtsData>();
        cores = original.cores != null ? new List<CoreData>(original.cores) : new List<CoreData>();
        inventory = original.inventory != null ? new List<ItemData>(original.inventory) : new List<ItemData>();
        equipFeo = original.equipFeo;
        equipArts = original.equipArts != null ? new List<ArtsData>(original.equipArts) : new List<ArtsData>();
        equipCores = original.equipCores != null ? new List<CoreData>(original.equipCores) : new List<CoreData>();
    }
}

[System.Serializable]
public class SkillState
{
    public bool isLearned;
    public int growValue;
}
// Compare this snippet from Assets/Script/PlayerManager.cs: