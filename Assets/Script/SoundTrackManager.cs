using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class SoundtrackManager : MonoBehaviour
{
    //public Button returnButton;
    public GameObject bgmButtonPrefab;
    public Transform contentParent;    // Scroll View の Content

    // 章データのリスト
    public List<ChapterBGMData> chapterBGMDataList;

    // 章ボタンのリスト
    public List<Button> chapterButtons;

    // 現在選択されている章のボタンを保持
    private Button currentSelectedChapterButton;

    void Start()
    {
        // 章ボタンのリスナーを設定
        for (int i = 0; i < chapterButtons.Count; i++)
        {
            int index = i; // ローカル変数にコピー
            chapterButtons[i].onClick.AddListener(() => OnChapterButtonClicked(chapterButtons[index], chapterBGMDataList[index]));
            // ボタンのテキストを章名に設定
            chapterButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = chapterBGMDataList[index].chapterName;
        }

        // 初期状態で最初の章のBGMリストを表示
        if (chapterBGMDataList.Count > 0)
        {
            OnChapterButtonClicked(chapterButtons[0], chapterBGMDataList[0]);
        }
    }

    void OnChapterButtonClicked(Button chapterButton, ChapterBGMData chapterData)
    {
        // 以前の選択状態をリセット
        if (currentSelectedChapterButton != null)
        {
            currentSelectedChapterButton.interactable = true;
        }
        else
        {
            Debug.LogWarning("ChapterButtonがnullです");
        }

        // 現在のボタンを選択状態にする
        currentSelectedChapterButton = chapterButton;
        

        // BGMリストを表示
        ShowBGMList(chapterData.bgmList);
        AudioManager.instance.SetBGMList(chapterData.bgmList.ConvertAll(bgmData => bgmData.bgmClip),true);
    }

    void ShowBGMList(List<BGMData> bgmList)
    {
        // 既存のBGMボタンを削除
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // BGMボタンを動的に生成
        for (int i = 0; i < bgmList.Count; i++)
        {
            GameObject buttonObj = Instantiate(bgmButtonPrefab, contentParent);
            BGMButton bgmButton = buttonObj.GetComponent<BGMButton>();
            bgmButton.Initialize(bgmList[i].bgmName, bgmList[i].bgmClip);
        }
    }

    /*
    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }//*/
}
