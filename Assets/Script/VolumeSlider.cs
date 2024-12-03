using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    private Slider volumeSlider;

    void Start()
    {
        volumeSlider = GetComponent<Slider>();

        // スライダーの初期値をAudioManagerから取得
        if (AudioManager.instance != null)
        {
            volumeSlider.value = AudioManager.instance.GetVolume();
        }

        // スライダーの値が変更されたときに呼ばれるリスナーを設定
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    void OnVolumeChanged(float value)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetVolume(value);
        }
    }
}
