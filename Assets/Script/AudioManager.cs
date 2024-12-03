using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    private AudioSource audioSource;
    public List<BGMData> bgmDataList = new List<BGMData>();

    public AudioClip topPageBGM; // 一旦特定のBGMをtopで流すことにする

    private float bgmVolume = 1.0f; // BGMの音量（0.0〜1.0）
    private string currentBGMName;

    private bool isRandom = false; // ランダム再生フラグ
    private int currentTrackIndex = 0; // 曲のindex

    private List<AudioClip> currentBGMList = new List<AudioClip>();

    private BGMData currentBGMData; // 現在の BGMData

    public AudioClip CurrentClip
    {
        get { return audioSource.clip; }
    }

    public bool currentRandom
    {
        get { return isRandom; }
    }

    public bool IsPlaying()
    {
        return audioSource != null && audioSource.isPlaying;
    }

    public string GetCurrentBGMName()
    {
        return currentBGMName;
    }

    void LoadAllBGMData()
    {
        bgmDataList = new List<BGMData>();

        string[] cantoFolders = new string[] { "CantoI", "CantoII", "CantoIII", "CantoIV", "CantoV" };

        foreach (string folder in cantoFolders)
        {
            BGMData[] bgmDatas = Resources.LoadAll<BGMData>("BGMData/" + folder);
            bgmDataList.AddRange(bgmDatas);
        }

        // ロードされた BGMData の数をログに表示
        Debug.Log("Loaded " + bgmDataList.Count + " BGMData assets.");
    }

    void Awake()
    {
        // シングルトンの設定
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = gameObject.AddComponent<AudioSource>();

            bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1.0f);
            audioSource.volume = bgmVolume;

            audioSource.loop = false;
            LoadAllBGMData();

            // BGM のロードや再生状態の復元
            string savedBGMName = PlayerPrefs.GetString("CurrentBGMName", "");
            if (!string.IsNullOrEmpty(savedBGMName))
            {
                AudioClip savedClip = FindBGMClipByName(savedBGMName);
                if (savedClip != null)
                {
                    currentTrackIndex = currentBGMList.IndexOf(savedClip);
                    PlayBGM(savedClip, false);
                }
            }
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (!IsPlaying())
        {
            PlayTopPageBGM();
        }
    }

    public void PlayTopPageBGM()
    {
        if (topPageBGM != null)
        {
            SetBGMList(new List<AudioClip> { topPageBGM }, true);
        }
        else
        {
            Debug.LogWarning("トップページ用のBGMを読み込めません");
        }
    }

    public void PlayBGM(AudioClip clip, bool saveState = true)
    {
        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();

            currentBGMName = clip.name;
            currentTrackIndex = currentBGMList.IndexOf(clip);

            // 現在の BGMData を更新
            currentBGMData = bgmDataList.Find(bgmData => bgmData.bgmClip == clip);

            // BGM が変更されたことを通知
            OnBGMChanged?.Invoke(currentTrackIndex);

            if (saveState)
            {
                PlayerPrefs.SetString("CurrentBGMName", currentBGMName);
                PlayerPrefs.Save();
            }
        }
        else
        {
            // clip が null の場合はランダムなクリップを再生
            PlayRandomBGM();
        }
    }

    public event Action<int> OnBGMChanged;

    public void StopBGM()
    {
        audioSource.Stop();
    }

    public void PlayNextTrack()
    {
        if (currentBGMList.Count == 0)
        {
            Debug.LogWarning("再生可能な BGM がありません。");
            return;
        }

        if (isRandom)
        {
            PlayRandomBGM();
        }
        else
        {
            currentTrackIndex = (currentTrackIndex + 1) % currentBGMList.Count;
            PlayBGM(currentBGMList[currentTrackIndex]);
        }
    }

    public void PlayPreviousTrack()
    {
        if (currentBGMList.Count == 0)
        {
            Debug.LogWarning("再生可能な BGM がありません。");
            return;
        }

        if (isRandom)
        {
            PlayRandomBGM();
        }
        else
        {
            currentTrackIndex = (currentTrackIndex - 1 + currentBGMList.Count) % currentBGMList.Count;
            PlayBGM(currentBGMList[currentTrackIndex]);
        }
    }

    // ランダムな BGM を再生
    public void PlayRandomBGM()
    {
        if (currentBGMList.Count == 0)
        {
            Debug.LogWarning("再生可能な BGM がありません。");
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, currentBGMList.Count);
        currentTrackIndex = randomIndex;
        PlayBGM(currentBGMList[currentTrackIndex]);
    }

    // ランダム再生の設定
    public void SetRandom(bool random)
    {
        isRandom = random;
    }

    public void SetVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume); // 0.0〜1.0の範囲に制限
        audioSource.volume = bgmVolume;

        // 音量を保存
        PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
        PlayerPrefs.Save();
    }

    public float GetVolume()
    {
        return bgmVolume;
    }

    private AudioClip FindBGMClipByName(string bgmName)
    {
        foreach (AudioClip clip in currentBGMList)
        {
            if (clip.name == bgmName)
            {
                return clip;
            }
        }
        return null;
    }

    // 再生状態の更新（ループ再生・ランダム再生に対応）
    void Update()
    {
        if (!audioSource.isPlaying)
        {
            if (currentBGMList.Count == 0)
            {
                // 再生する BGM がない場合は何もしない
                return;
            }
            PlayNextTrack();
        }
    }

    public void SetBGMList(List<AudioClip> bgmList, bool playFirstTrack = false)
    {
        if (bgmList != null && bgmList.Count > 0)
        {
            currentBGMList = new List<AudioClip>(bgmList);
            currentTrackIndex = 0;

            if (playFirstTrack)
            {
                // 現在の再生を停止して、新しいリストの最初の曲を再生する
                StopBGM();
                PlayBGM(currentBGMList[currentTrackIndex]);
            }
            else
            {
                // 現在再生中の BGM がリストに含まれているか確認し、インデックスを更新
                int index = currentBGMList.IndexOf(audioSource.clip);
                if (index >= 0)
                {
                    currentTrackIndex = index;
                }
                else
                {
                    // 現在の BGM がリストに含まれていない場合、再生を停止するか、デフォルトの動作を指定
                    // ここでは何もしない
                }
            }
        }
        else
        {
            Debug.LogWarning("空の BGM リストが渡されました。");
            currentBGMList.Clear();
            StopBGM();
        }
    }


    public BGMData GetCurrentBGMData()
    {
        return currentBGMData;
    }

    public BGMData GetBGMDataAtIndex(int index)
    {
        if (currentBGMList != null && index >= 0 && index < currentBGMList.Count)
        {
            AudioClip clip = currentBGMList[index];
            BGMData bgmData = bgmDataList.Find(data => data.bgmClip == clip);
            return bgmData;
        }
        return null;
    }


    public int GetCurrentTrackIndex()
    {
        return currentTrackIndex;
    }
}
