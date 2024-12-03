using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;

public class SoundInfoGenerate : MonoBehaviour
{
    public TMP_Text soundNameText;
    public TMP_Text soundSourceText;
    public TMP_Text soundSceneText;
    public TMP_Text soundCommentText;

    private int trackIndex;
    private AudioManager audioManager;
    private TextAnimator textAnimator;

    private Coroutine commentAnimationCoroutine;

    void Start()
    {
        audioManager = AudioManager.instance;
        textAnimator = TextAnimator.instance;

        if (audioManager == null)
        {
            Debug.LogError("AudioManager が見つかりません。");
            return;
        }

        if (textAnimator == null)
        {
            Debug.LogError("TextAnimator が見つかりません。");
            return;
        }


        audioManager.OnBGMChanged += HandleBGMChanged;
        trackIndex = audioManager.GetCurrentTrackIndex();

        UpdateSoundInfo(trackIndex);
    }

    void OnDestroy()
    {
        if (audioManager != null)
        {
            audioManager.OnBGMChanged -= HandleBGMChanged;
        }
    }

    void HandleBGMChanged(int trackIndex)
    {
        UpdateSoundInfo(trackIndex);
    }

    void UpdateSoundInfo(int trackIndex)
    {
        BGMData currentBGM = audioManager.GetBGMDataAtIndex(trackIndex);

        if (currentBGM != null)
        {
            UpdateUI(currentBGM);
        }
        else
        {
            // BGM が再生されていない場合の表示
            UpdateSoundName("No BGM");
            UpdateSoundSource("-");
            UpdateSoundScene("-");
            UpdateSoundComment("-");
        }
    }

    void UpdateUI(BGMData bgmData)
    {
        UpdateSoundName(bgmData.bgmName);
        UpdateSoundSource(bgmData.bgmSource);
        UpdateSoundScene(bgmData.bgmUseScene);
        UpdateSoundComment(bgmData.bgmComment);
    }

    void UpdateSoundName(string name)
    {
        if (soundNameText != null)
        {
            soundNameText.text = name;
        }
    }

    void UpdateSoundSource(string source)
    {
        if (soundSourceText != null)
        {
            soundSourceText.text = "Source:" + source;
        }
    }

    void UpdateSoundScene(string scene)
    {
        if (soundSceneText != null)
        {
            soundSceneText.text = scene;
        }
    }

    void UpdateSoundComment(string comment)
    {
        if (soundCommentText != null)
        {
            soundCommentText.text = comment;
            if(commentAnimationCoroutine != null)
            {
                StopCoroutine(commentAnimationCoroutine);
            }
            commentAnimationCoroutine = StartCoroutine(textAnimator.SimpleAnimateText(soundCommentText, comment));
        }
    }
}
