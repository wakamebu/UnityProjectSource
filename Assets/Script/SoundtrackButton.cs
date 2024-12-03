using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SoundtrackButton : MonoBehaviour
{
    public TextMeshProUGUI bgmNameText;
    private Button button;
    private BGMData bgmData;

    public void Initialize(BGMData data)
    {
        bgmData = data;
        bgmNameText.text = bgmData.bgmName;

        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);
    }

    void OnButtonClicked()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayBGM(bgmData.bgmClip);
        }
    }
}
