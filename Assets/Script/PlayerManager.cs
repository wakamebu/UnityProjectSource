using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public List<PlayerData> playerDatas;
    public AbilityDataSet abilityDataSet;
    public int playerSelectedindex;

    void Start()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPlayerData();
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }
    }

    void LoadPlayerData()
    {
        Debug.Log("LoadPlayerData開始");
        SaveData saveData = SaveManager.instance.LoadGame();
        
        if(saveData != null && saveData.playerDatas != null && saveData.playerDatas.Count > 0)
        {
            Debug.Log("セーブデータからプレイヤーデータをロードします");
            playerDatas = saveData.playerDatas;
            playerSelectedindex = saveData.selectedPlayerIndex;
            
            if (playerDatas != null && playerDatas.Count > 0)
            {
                Debug.Log($"ロードされたプレイヤーデータ:");
                Debug.Log($"- プレイヤー数: {playerDatas.Count}");
                Debug.Log($"- 選択インデックス: {playerSelectedindex}");
                Debug.Log($"- 最初のプレイヤー:");
                Debug.Log($"  - 名前: {playerDatas[0].playerName}");
                Debug.Log($"  - レベル: {playerDatas[0].level}");
                Debug.Log($"  - 経験値: {playerDatas[0].experience}");
                Debug.Log($"  - 使用ソウル: {playerDatas[0].usedSoul}");
            }
        }
        else
        {
            Debug.Log("セーブデータが存在しないか、無効なデータです。新規データを初期化します。");
            InitializeNewPlayerData();
        }
    }

    public void InitializeNewPlayerData()
    {
        string saveFilePath = Application.persistentDataPath + "/saveData.json";
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("既存のファイルを削除しました: " + saveFilePath);
        }

        UnityEngine.Debug.Log("PlayerData初期化開始");
        playerDatas.Clear();
        PlayerDataScriptableObject[] defaultCharacters = Resources.LoadAll<PlayerDataScriptableObject>("character");
        Debug.Log($"読み込んだキャラクター数: {defaultCharacters.Length}");
        
        foreach (var so in defaultCharacters)
        {
            if (so == null || so.playerData == null)
            {
                Debug.LogError($"PlayerDataScriptableObjectまたはそのplayerDataがnullです。キャラクター名: {so?.name ?? "null"}");
                continue;
            }

            Debug.Log($"キャラクター '{so.name}' の初期化を開始");
            Debug.Log($"playerData: {(so.playerData != null ? "存在します" : "nullです")}");
            Debug.Log($"feos: {(so.playerData?.feos != null ? "存在します" : "nullです")}");
            Debug.Log($"arts: {(so.playerData?.arts != null ? "存在します" : "nullです")}");
            Debug.Log($"cores: {(so.playerData?.cores != null ? "存在します" : "nullです")}");
            Debug.Log($"inventory: {(so.playerData?.inventory != null ? "存在します" : "nullです")}");
            Debug.Log($"equipArts: {(so.playerData?.equipArts != null ? "存在します" : "nullです")}");
            Debug.Log($"equipCores: {(so.playerData?.equipCores != null ? "存在します" : "nullです")}");
            Debug.Log($"skillStates: {(so.playerData?.skillStates != null ? "存在します" : "nullです")}");
            
            PlayerData playerData = new PlayerData(so.playerData);
            playerData.playerName = so.playerData.playerName;
            UnityEngine.Debug.Log(playerData.playerName);

            playerData.level = so.playerData.level;
            playerData.experience = so.playerData.experience;
            playerData.hp = so.playerData.hp;
            playerData.mp = so.playerData.mp;
            playerData.san = so.playerData.san;
            playerData.strength = so.playerData.strength;
            playerData.inteligence = so.playerData.inteligence;
            playerData.dexterity = so.playerData.dexterity;
            playerData.power = so.playerData.power;
            playerData.constitution = so.playerData.constitution;
            playerData.appearance = so.playerData.appearance;
            playerData.size = so.playerData.size;
            playerData.education = so.playerData.education;

            // リストの新しいインスタンスを作成
            playerData.feos = so.playerData.feos != null ? new List<FeoData>(so.playerData.feos) : new List<FeoData>();
            Debug.Log($"feos: {playerData.feos.Count} 個の要素");
            
            playerData.arts = so.playerData.arts != null ? new List<ArtsData>(so.playerData.arts) : new List<ArtsData>();
            Debug.Log($"arts: {playerData.arts.Count} 個の要素");
            
            playerData.cores = so.playerData.cores != null ? new List<CoreData>(so.playerData.cores) : new List<CoreData>();
            Debug.Log($"cores: {playerData.cores.Count} 個の要素");
            
            playerData.inventory = so.playerData.inventory != null ? new List<ItemData>(so.playerData.inventory) : new List<ItemData>();
            Debug.Log($"inventory: {playerData.inventory.Count} 個の要素");
            
            playerData.equipFeo = "";
            playerData.equipArts = so.playerData.equipArts != null ? new List<ArtsData>(so.playerData.equipArts) : new List<ArtsData>();
            Debug.Log($"equipArts: {playerData.equipArts.Count} 個の要素");
            
            playerData.equipCores = so.playerData.equipCores != null ? new List<CoreData>(so.playerData.equipCores) : new List<CoreData>();
            Debug.Log($"equipCores: {playerData.equipCores.Count} 個の要素");
            
            playerDatas.Add(playerData);
            InitializeSkillStates(playerData);
        }
        playerSelectedindex = 0;
        SavePlayerData();
    }

    public void SavePlayerData()
    {
        SaveData saveData = new SaveData();
        saveData.playerDatas = playerDatas;
        saveData.selectedPlayerIndex = playerSelectedindex;

        SaveManager.instance.SaveGame(saveData);
    }

    public PlayerData GetSelectedPlayerData()
    {
        if(playerDatas != null && playerDatas.Count > playerSelectedindex)
        {
            return playerDatas[playerSelectedindex];
        }
        else
        {
            Debug.LogError("選択されたプレイヤーデータが存在しません");
            return null;
        }
    }

    // public void InitializeAndSetArrayLength(PlayerData playerData)
    // {
    //     if(playerData != null)
    //     {
    //         playerData.hasSearchAndActionAbilities = new bool[abilityDataSet.searchAndActionAbilities.Length];
    //         playerData.hasCombatAndNegotiationAbilities = new bool[abilityDataSet.combatAndNegotiationAbilities.Length];
    //         playerData.hasKnowledgeAndLanguageAbilities = new bool[abilityDataSet.knowledgeAndLanguageAbilities.Length];
    //         playerData.searchAndActionAbilitiesGrowValue = new int[abilityDataSet.searchAndActionAbilities.Length];
    //         playerData.combatAndNegotiationAbilitiesGrowValue = new int[abilityDataSet.combatAndNegotiationAbilities.Length];
    //         playerData.knowledgeAndLanguageAbilitiesGrowValue = new int[abilityDataSet.knowledgeAndLanguageAbilities.Length];
    //     }
    //     for(int i = 0 ; i < abilityDataSet.searchAndActionAbilities.Length; i++)
    //     {
    //         AbilityDataSO abilityDataSO = abilityDataSet.searchAndActionAbilities[i];
    //         bool learned = (abilityDataSO.defaultLearnBool == 1);
    //         playerData.hasSearchAndActionAbilities[i] = learned;
    //     }
    //     for(int i = 0 ; i < abilityDataSet.combatAndNegotiationAbilities.Length; i++)
    //     {
    //         AbilityDataSO abilityDataSO = abilityDataSet.combatAndNegotiationAbilities[i];
    //         bool learned = (abilityDataSO.defaultLearnBool == 1);
    //         playerData.hasCombatAndNegotiationAbilities[i] = learned;
    //     }
    //     for(int i = 0 ; i < abilityDataSet.knowledgeAndLanguageAbilities.Length; i++)
    //     {
    //         AbilityDataSO abilityDataSO = abilityDataSet.knowledgeAndLanguageAbilities[i];
    //         bool learned = (abilityDataSO.defaultLearnBool == 1);
    //         playerData.hasKnowledgeAndLanguageAbilities[i] = learned;
    //     }
    //     UnityEngine.Debug.Log("技能の初期化完了");
    // }

    public void InitializeSkillStates(PlayerData playerData)
    {
        if(playerData.skillStates == null)
        {
            playerData.skillStates = new Dictionary<string, SkillState>();
        }
        else
        {
            playerData.skillStates.Clear();
        }

        // 探索・行動スキル
        foreach (var abilityDataSO in abilityDataSet.searchAndActionAbilities)
        {
            // abilityIDをキーにして辞書に登録
            SkillState newState = new SkillState();
            newState.isLearned = (abilityDataSO.defaultLearnBool == 1);
            newState.growValue = 0;
            
            playerData.skillStates[abilityDataSO.abilityID] = newState;
        }

        // 戦闘・交渉スキル
        foreach (var abilityDataSO in abilityDataSet.combatAndNegotiationAbilities)
        {
            SkillState newState = new SkillState();
            newState.isLearned = (abilityDataSO.defaultLearnBool == 1);
            newState.growValue = 0;
            
            playerData.skillStates[abilityDataSO.abilityID] = newState;
        }

        // 知識・言語スキル
        foreach (var abilityDataSO in abilityDataSet.knowledgeAndLanguageAbilities)
        {
            SkillState newState = new SkillState();
            newState.isLearned = (abilityDataSO.defaultLearnBool == 1);
            newState.growValue = 0;
            
            playerData.skillStates[abilityDataSO.abilityID] = newState;
        }

        Debug.Log("技能の初期化完了");
    }

}
