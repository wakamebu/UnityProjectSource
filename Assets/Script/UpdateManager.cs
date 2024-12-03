using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
using System.IO.Compression;
using System.Collections;
using TMPro;
using System.Diagnostics;

public class StandaloneUpdater : MonoBehaviour
{
    // 更新情報のURL
    private const string versionUrl = "http://fproject.starfree.jp/updates/version.json";
    private string currentVersion;

    void Start()
    {
        LoadLocalVersion();
        StartCoroutine(CheckForUpdates());
    }

    //updater起動
    void StartUpdaterAndExit()
    {
        string updaterPath = Path.Combine(Application.dataPath, "Updater", "Updater.exe");
        string updateFilePath = Path.Combine(Application.persistentDataPath, "update.zip");
        string appExePath = Path.Combine(Application.dataPath, "..", Application.productName + ".exe");
        string installDir = Path.Combine(Application.dataPath, "..");

        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = updaterPath;
        startInfo.Arguments = $"\"{updateFilePath}\" \"{appExePath}\" \"{installDir}\"";
        startInfo.UseShellExecute = false;

        UnityEngine.Debug.Log("startInfo.Arguments is" + startInfo.Arguments);

        Process.Start(startInfo);

        // アプリケーションを終了する
        Application.Quit();
    }

    // ローカルのバージョン情報を読み込む
    void LoadLocalVersion()
    {
        // Resourcesフォルダからversion.jsonを読み込む
        TextAsset versionFile = Resources.Load<TextAsset>("version");
        if (versionFile != null)
        {
            VersionInfo versionInfo = JsonUtility.FromJson<VersionInfo>(versionFile.text);
            currentVersion = versionInfo.version;
            UnityEngine.Debug.Log("Current Version: " + currentVersion);
        }
        else
        {
            UnityEngine.Debug.LogError("Version file not found!");
            currentVersion = "0.0.0"; // デフォルトのバージョンを設定
        }
    }

    // サーバーから最新バージョンを取得して更新をチェックする
    IEnumerator CheckForUpdates()
    {
        UnityWebRequest request = UnityWebRequest.Get(versionUrl);
        UnityEngine.Debug.Log("Update URL: " + versionUrl);
        
        // 証明書の検証を無効化(SSLが有効ならばスルーする)
        request.certificateHandler = new BypassCertificateHandler();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            UnityEngine.Debug.LogError("Update check failed: " + request.error);
        }
        else
        {
            string json = request.downloadHandler.text;
            VersionInfo serverVersion = JsonUtility.FromJson<VersionInfo>(json);

            if (serverVersion.version != currentVersion)
            {
                UnityEngine.Debug.Log($"New version available: {serverVersion.version}");
                UnityEngine.Debug.Log("Downloading update from URL: " + serverVersion.url);
                StartCoroutine(DownloadAndApplyUpdate(serverVersion.url));
            }
            else
            {
                UnityEngine.Debug.Log("App is up-to-date");
            }
        }
    }

    // 更新ファイルをダウンロードして適用する
    IEnumerator DownloadAndApplyUpdate(string updateUrl)
    {
        NotifyRestart();
        if (updateUrl.StartsWith("https://"))
        {
            updateUrl = updateUrl.Replace("https://", "http://");
        }

        UnityWebRequest request = UnityWebRequest.Get(updateUrl);
        string tempFilePath = Path.Combine(Application.persistentDataPath, "update.zip");
        UnityEngine.Debug.Log("update data from " + tempFilePath);
        //UnityEngine.Debug.Log("app path is " + Application.dataPath, "..", Application.productName + ".exe");
        //UnityEngine.Debug.Log("Updater path is " + Application.dataPath, "..", "Updater.exe");

        // 証明書の検証を無効化
        // request.certificateHandler = new BypassCertificateHandler();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            UnityEngine.Debug.LogError($"Update download failed: {request.error}, Response Code: {request.responseCode}");
            UnityEngine.Debug.LogError("Response: " + request.downloadHandler.text);
        }
        else
        {
            File.WriteAllBytes(tempFilePath, request.downloadHandler.data);
            UnityEngine.Debug.Log("Update downloaded to " + tempFilePath);
            StartUpdaterAndExit();
            ApplyUpdate(tempFilePath);
        }
    }

    // ダウンロードした更新を適用する
    void ApplyUpdate(string zipFilePath)
    {
        string appDirectory = Application.dataPath; // アプリのインストールディレクトリ
        string extractPath = Path.Combine(appDirectory, "..");
        UnityEngine.Debug.Log("Update to " + appDirectory);

        ZipFile.ExtractToDirectory(zipFilePath, extractPath, true);

        UnityEngine.Debug.Log("Update applied.");
        NotifyRestart();
    }

    // 更新完了をユーザーに通知する
    void NotifyRestart()
    {
        // シンプルなUIを表示する
        GameObject canvas = new GameObject("RestartNotificationCanvas");
        Canvas c = canvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler cs = canvas.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(canvas.transform, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 200);
        panel.AddComponent<CanvasRenderer>();
        panel.AddComponent<Image>().color = new Color(0, 0, 0, 0.8f);

        GameObject textObj = new GameObject("RestartText");
        textObj.transform.SetParent(panel.transform, false);
        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.sizeDelta = new Vector2(380, 180);
        textRt.localPosition = Vector3.zero;

        // TextMesh Pro のテキストコンポーネントを追加
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "更新があります。アプリケーションが再起動するまでお待ちください。";
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.red;
        text.fontSize = 20; // 必要に応じてフォントサイズを設定

        // フォントアセットをロードして適用
        // フォントアセットは Resources フォルダ内に配置する必要があります
        TMP_FontAsset customFont = Resources.Load<TMP_FontAsset>("Fonts/SourceHanSansJP-Bold SDF");
        if (customFont != null)
        {
            text.font = customFont;
        }
        else
        {
            UnityEngine.Debug.LogError("フォントアセットが見つかりませんでした。パスとファイル名を確認してください。");
        }
    }

    // 証明書の検証を無効化するハンドラ
    private class BypassCertificateHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            // 常に証明書を信頼する
            return true;
        }
    }

    // バージョン情報を保持するクラス
    [System.Serializable]
    private class VersionInfo
    {
        public string version;
        public string url;
    }
}
