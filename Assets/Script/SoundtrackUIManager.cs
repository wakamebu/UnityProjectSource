using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class SoundtrackUIManager : MonoBehaviour
{
    // UI 要素への参照
    public Button playButton;
    public Button stopButton;
    public Toggle randomToggle;
    public Slider volumeSlider;
    public Button nextTrackButton;
    public Button previousTrackButton;

    // トグルのラベル
    public TextMeshProUGUI randomToggleLabel;

    private AudioManager audioManager;

    void Start()
    {
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("AudioManager が存在しません。シーンに AudioManager を追加してください。");
            return;
        }

        // ボタンのリスナーを設定
        playButton.onClick.AddListener(OnPlayButtonClicked);
        stopButton.onClick.AddListener(OnStopButtonClicked);
        nextTrackButton.onClick.AddListener(OnNextTrackButtonClicked);
        previousTrackButton.onClick.AddListener(OnPreviousTrackButtonClicked);

        // トグルのリスナーを設定
        randomToggle.onValueChanged.AddListener(OnRandomToggleChanged);

        // ボリュームスライダーのリスナーを設定
        volumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);

        // 初期設定
        volumeSlider.value = audioManager.GetVolume();
        randomToggle.isOn = audioManager.currentRandom;
        UpdateRandomToggleLabel();
    }

    void OnPlayButtonClicked()
    {
        if (!audioManager.IsPlaying())
        {
            audioManager.PlayBGM(audioManager.CurrentClip);
        }
    }

    void OnStopButtonClicked()
    {
        audioManager.StopBGM();
    }

    void OnRandomToggleChanged(bool isOn)
    {
        audioManager.SetRandom(isOn);
        UpdateRandomToggleLabel();
    }

    void UpdateRandomToggleLabel()
    {
        if (randomToggleLabel != null)
        {
            randomToggleLabel.text = randomToggle.isOn ? "ランダム再生 ON" : "ランダム再生 OFF";
        }
    }

    void OnVolumeSliderChanged(float value)
    {
        audioManager.SetVolume(value);
    }

    void OnNextTrackButtonClicked()
    {
        Debug.Log("次のトラックを再生します");
        audioManager.PlayNextTrack();
    }

    void OnPreviousTrackButtonClicked()
    {
        Debug.Log("前のトラックを再生します");
        audioManager.PlayPreviousTrack();
    }
}
