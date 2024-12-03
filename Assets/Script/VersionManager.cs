using UnityEngine;
using System.IO;

public class VersionManager : MonoBehaviour
{
    public static VersionManager instance { get; private set; } 

    public string CurrentVersion { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadLocalVersion();
    }

    private void LoadLocalVersion()
    {
        // Resourcesフォルダからversion.jsonを読み込む
        TextAsset versionFile = Resources.Load<TextAsset>("version");
        if (versionFile != null)
        {
            VersionInfo versionInfo = JsonUtility.FromJson<VersionInfo>(versionFile.text);
            CurrentVersion = versionInfo.version;
            Debug.Log("Current Version: " + CurrentVersion);
        }
        else
        {
            Debug.LogError("Version file not found!");
        }
    }
}
