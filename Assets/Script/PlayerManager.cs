using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public List<PlayerData> playerDatas;
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
        SaveData saveData = SaveManager.instance.LoadGame();
        if(saveData != null && saveData.playerDatas != null && saveData.playerDatas.Count > 0)
        {
            playerDatas = saveData.playerDatas;
            playerSelectedindex = saveData.selectedPlayerIndex;
            //多分下のメソッドを呼び出すときにNullを吐いているから、ボタンを作ってLoadFEOを走らせるテストを行ったほうが良いと思います。いつかの自分へ。2024/11/10わかば
            //FEOManager.instance.LoadFEO(playerData.feos);
            Debug.Log("プレイヤーデータをロードしました");
        }
        else{
            InitializeNewPlayerData();
            /*
        for(int i = 0; i < 6; i++)
            {
                PlayerData playerData = new PlayerData();
                playerData.playerName = "キャラクター" + (i + 1);
                playerData.level = 1;
                // 他のステータスを初期化
                playerData.hp = 1;
                playerData.mp = 1;
                // 必要に応じて他のフィールドを初期化

                // インベントリの初期化
                playerData.feos = new List<FeoData>();
                playerData.arts = new List<ArtsData>();
                playerData.cores = new List<CoreData>();
                playerData.inventory = new List<ItemData>();

                // 装備品の初期化
                playerData.equipFeo = "";
                playerData.equipArts = new List<ArtsData>();
                playerData.equipCores = new List<CoreData>();

                playerDatas.Add(playerData);
            }
        playerSelectedindex = 0;
        Debug.Log("新しいプレイヤーデータを初期化しました");
        */
        }
    }

    public void InitializeNewPlayerData()
    {
        UnityEngine.Debug.Log("PlayerData初期化開始");
        playerDatas.Clear();
        PlayerDataScriptableObject[] defaultCharacters = Resources.LoadAll<PlayerDataScriptableObject>("character");
        foreach (var so in defaultCharacters)
        {
            PlayerData playerData = new PlayerData();
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

            playerData.feos = so.playerData.feos;
            playerData.arts = so.playerData.arts;
            playerData.cores = so.playerData.cores;
            playerData.inventory = so.playerData.inventory;

            playerData.equipFeo = "";
            playerData.equipArts = so.playerData.equipArts;
            playerData.equipCores = so.playerData.equipCores;
            
            playerDatas.Add(playerData);
        }
        playerSelectedindex = 0;
        SavePlayerData();
        Application.Quit();
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

}
