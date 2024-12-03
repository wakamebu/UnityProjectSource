using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    private string saveFilePath;
    private SaveData savedata;
    private int currentDataVersion;
    private int checkDataVersion;

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
        if (File.Exists(saveFilePath))
        { 
            savedata = saveData;
            currentDataVersion = savedata.dataVersion;
            if (currentDataVersion != checkDataVersion)
            {
                Debug.Log("セーブデータのデータバージョンが異なります");
                DeleteSaveData();
            }
            try
            {
                string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
                string encryptedJson = EncryptionUtility.EncryptString(json);

                File.WriteAllText(saveFilePath, encryptedJson);
                Debug.Log("ゲームデータを保存しました: " + saveFilePath);
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
                Debug.Log("ゲームデータを作成しました: " + saveFilePath);
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
        if (File.Exists(saveFilePath))
        {
            try
            {
                string encryptedJson = File.ReadAllText(saveFilePath);
                string json = EncryptionUtility.DecryptString(encryptedJson);

                SaveData saveData = JsonConvert.DeserializeObject<SaveData>(json);
                checkDataVersion = saveData.dataVersion;
                Debug.Log("ゲームデータを読み込みました: " + saveFilePath);
                return saveData;
            }
            catch (System.Exception e)
            {
                Debug.LogError("ロード中にエラーが発生しました: " + e.Message);
                return null;
            }
        }
        else
        {
            Debug.LogWarning("セーブデータが存在しません: " + saveFilePath);
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
