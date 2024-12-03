using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VersionGet : MonoBehaviour
{
    [SerializeField]
    public TMP_Text currentVersionText;
    
    public static VersionGet instance;
    private string currentVersion;
    void Start()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            GetVersion();
            currentVersionText.text = "Version : " + currentVersion;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }
    }

    void GetVersion()
    {
        TextAsset versionFile = Resources.Load<TextAsset>("version");
        if (versionFile != null)
        {
            VersionInfo versionInfo = JsonUtility.FromJson<VersionInfo>(versionFile.text);
            currentVersion = versionInfo.version;
        }
        else
        {
            UnityEngine.Debug.LogError("Version file not found!");
            currentVersion = "0.0.0"; // デフォルトのバージョンを設定
        }
    }
}
