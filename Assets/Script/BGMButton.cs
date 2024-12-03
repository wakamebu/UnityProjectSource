using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BGMButton : MonoBehaviour
{
    public TextMeshProUGUI bgmNameText;
    private AudioClip bgmClip;

    public void Initialize(string bgmName, AudioClip clip)
    {
        bgmNameText.text = bgmName;
        bgmClip = clip;

        // ボタンのクリックイベントを設定
        GetComponent<Button>().onClick.AddListener(OnButtonClicked);
    }

    void OnButtonClicked()
    {
        // AudioManager を使用して BGM を再生
        AudioManager.instance.PlayBGM(bgmClip);
    }
}
