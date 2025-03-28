using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    private string saveFilePath;
    private SaveData savedata;
    private const int CURRENT_DATA_VERSION = 2; // 現在のデータバージョンを定数として定義

    void Awake()
    {
        // シングルトンの設定
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            saveFilePath = Application.persistentDataPath + "/saveData.json";
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    // セーブデータを保存
    public void SaveGame(SaveData saveData)
    {
        Debug.Log($"SaveGame開始 - データバージョン: {saveData.dataVersion}, 現在のバージョン: {CURRENT_DATA_VERSION}");
        
        if (File.Exists(saveFilePath))
        { 
            savedata = saveData;
            if (savedata.dataVersion != CURRENT_DATA_VERSION)
            {
                Debug.Log($"セーブデータのデータバージョンが異なります。現在のバージョン: {CURRENT_DATA_VERSION}, セーブデータのバージョン: {savedata.dataVersion}");
                DeleteSaveData();
            }
            try
            {
                string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
                string encryptedJson = EncryptionUtility.EncryptString(json);

                File.WriteAllText(saveFilePath, encryptedJson);
                Debug.Log($"ゲームデータを保存しました: {saveFilePath}");
                Debug.Log($"保存されたデータ: プレイヤー数={saveData.playerDatas?.Count ?? 0}, 選択インデックス={saveData.selectedPlayerIndex}");
                if (saveData.playerDatas != null && saveData.playerDatas.Count > 0)
                {
                    Debug.Log($"最初のプレイヤーのレベル: {saveData.playerDatas[0].level}");
                    Debug.Log($"最初のプレイヤーの経験値: {saveData.playerDatas[0].experience}");
                    Debug.Log($"最初のプレイヤーの使用ソウル: {saveData.playerDatas[0].usedSoul}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("セーブ中にエラーが発生しました: " + e.Message);
            }
        }
        else
        {
            try
            {
                string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
                string encryptedJson = EncryptionUtility.EncryptString(json);

                File.WriteAllText(saveFilePath, encryptedJson);
                Debug.Log($"ゲームデータを作成しました: {saveFilePath}");
                Debug.Log($"保存されたデータ: プレイヤー数={saveData.playerDatas?.Count ?? 0}, 選択インデックス={saveData.selectedPlayerIndex}");
                if (saveData.playerDatas != null && saveData.playerDatas.Count > 0)
                {
                    Debug.Log($"最初のプレイヤーのレベル: {saveData.playerDatas[0].level}");
                    Debug.Log($"最初のプレイヤーの経験値: {saveData.playerDatas[0].experience}");
                    Debug.Log($"最初のプレイヤーの使用ソウル: {saveData.playerDatas[0].usedSoul}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("セーブ中にエラーが発生しました: " + e.Message);
            }
        }
    }

    // セーブデータを読み込み
    public SaveData LoadGame()
    {
        Debug.Log($"LoadGame開始 - セーブファイルパス: {saveFilePath}");
        
        if (File.Exists(saveFilePath))
        {
            try
            {
                string encryptedJson = File.ReadAllText(saveFilePath);
                Debug.Log("暗号化されたJSONを読み込みました");
                
                string json = EncryptionUtility.DecryptString(encryptedJson);
                Debug.Log("JSONを復号化しました");
                Debug.Log($"復号化されたJSON: {json}");

                SaveData saveData = JsonConvert.DeserializeObject<SaveData>(json);
                Debug.Log($"デシリアライズ完了 - データバージョン: {saveData.dataVersion}, 現在のバージョン: {CURRENT_DATA_VERSION}");
                
                if (saveData.dataVersion != CURRENT_DATA_VERSION)
                {
                    Debug.Log($"セーブデータのデータバージョンが異なります。現在のバージョン: {CURRENT_DATA_VERSION}, セーブデータのバージョン: {saveData.dataVersion}");
                    return null;
                }

                if (saveData.playerDatas != null && saveData.playerDatas.Count > 0)
                {
                    Debug.Log($"ロードされたプレイヤーデータ:");
                    Debug.Log($"- プレイヤー数: {saveData.playerDatas.Count}");
                    Debug.Log($"- 選択インデックス: {saveData.selectedPlayerIndex}");
                    Debug.Log($"- 最初のプレイヤー:");
                    Debug.Log($"  - 名前: {saveData.playerDatas[0].playerName}");
                    Debug.Log($"  - レベル: {saveData.playerDatas[0].level}");
                    Debug.Log($"  - 経験値: {saveData.playerDatas[0].experience}");
                    Debug.Log($"  - 使用ソウル: {saveData.playerDatas[0].usedSoul}");
                }
                else
                {
                    Debug.LogWarning("プレイヤーデータが存在しません");
                }

                return saveData;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"ロード中にエラーが発生しました: {e.Message}\n{e.StackTrace}");
                return null;
            }
        }
        else
        {
            Debug.LogWarning($"セーブデータが存在しません: {saveFilePath}");
            return null;
        }
    }

    // セーブデータを削除
    public void DeleteSaveData()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("セーブデータを削除しました: " + saveFilePath);
        }
        else
        {
            Debug.LogWarning("削除するセーブデータが存在しません: " + saveFilePath);
        }
    }
}
