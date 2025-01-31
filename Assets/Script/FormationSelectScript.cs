using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FormationSelectScript : MonoBehaviour
{
    [Header("UI Elements")]
    public Button[] formationCharacterButtons; 
    public GameObject mainCanvas;
    public GameObject secondCanvas;
    public RectTransform[] secondCanvasUIElements; //画像とUI
    public Toggle setSelectCharacter;

    public int selectedCharacterIndex;

    public FormationCharacterLevelUP formationCharacterLevelUP; 
    [SerializeField] private CharacterImageSetter characterImageSetter;

    private static FormationSelectScript instance;

    void Start()
    {
        if(instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }
        //必ずはじめはキャラを選択するため非表示にする
        mainCanvas.SetActive(true);
        secondCanvas.SetActive(false);

        for(int i = 0; i < formationCharacterButtons.Length;i++)
        {
            int index = i;
            formationCharacterButtons[i].onClick.AddListener(() => OnCharacterButtonClicked(index));
            Debug.Log("ボタンのリスナーを設定");
        }
    }

    void OnCharacterButtonClicked(int index)
    {
        selectedCharacterIndex = index;
        Debug.Log(selectedCharacterIndex + "番目のボタンがクリックされました");

        // PlayerManager の選択インデックスを更新
        PlayerManager.instance.playerSelectedindex = index;

        // PalyerDataGetAndView の表示を更新
        var viewer = FindObjectOfType<PalyerDataGetAndView>();
        if (viewer != null && setSelectCharacter != enabled)
        {
            viewer.RefreshView();
        }

        // CharacterImageSetterでSecondCanvasの画像を更新
        if(characterImageSetter != null)
        {
            characterImageSetter.SetCharacterImage(selectedCharacterIndex);
        }

        StartCoroutine(TransitionToSecondCanvas());

        if(PlayerManager.instance != null && PlayerManager.instance.playerDatas.Count > selectedCharacterIndex)
        {
            PlayerData playerData = PlayerManager.instance.playerDatas[selectedCharacterIndex];
            if (formationCharacterLevelUP == null)
            {
                formationCharacterLevelUP = FormationCharacterLevelUP.instance;
            }
            formationCharacterLevelUP.selectCharaterUpdate(playerData);
            Debug.Log(playerData.playerName + "の情報を表示します");
        }
    }

    public void CloseSecondCanvas()
    {
        // SecondCanvasのUI要素を逆アニメーションで画面外に移動
        float delay = 0f;
        foreach (RectTransform uiElement in secondCanvasUIElements)
        {
            uiElement.DOAnchorPosX(-Screen.width, 0.5f).SetDelay(delay).SetEase(Ease.InBack);
            delay += 0.1f;
        }

        // アニメーションが終わったらSecondCanvasを非表示にし、MainCanvasを表示
        StartCoroutine(TransitionToMainCanvas());
    }

    IEnumerator TransitionToSecondCanvas()
    {
        // MainCanvasのCanvasGroupを取得
        CanvasGroup canvasGroup = mainCanvas.GetComponent<CanvasGroup>();
        if(canvasGroup != null)
        {
            // フェードアウト（0.8秒）
            canvasGroup.DOFade(0f, 0.8f);
            yield return new WaitForSeconds(0.8f);
        }

        // MainCanvasを非表示にする
        mainCanvas.SetActive(false);

        // SecondCanvasをアクティブにする
        secondCanvas.SetActive(true);

        // UI要素の初期位置を画面外（左端）に設定
        foreach (RectTransform uiElement in secondCanvasUIElements)
        {
            Vector3 pos = uiElement.anchoredPosition;
            pos.x = -Screen.width;
            uiElement.anchoredPosition = pos;
        }

        // 各UI要素を順番にアニメーションさせる
        float delay = 0f;
        foreach (RectTransform uiElement in secondCanvasUIElements)
        {
            uiElement.DOAnchorPosX(0f, 0.5f).SetDelay(delay).SetEase(Ease.OutBack);
            delay += 0.1f; // 次の要素のアニメーションを少し遅らせる
        }

    }


    IEnumerator TransitionToMainCanvas()
    {
        yield return new WaitForSeconds(0.5f + (secondCanvasUIElements.Length * 0.1f));

        secondCanvas.SetActive(false);
        mainCanvas.SetActive(true);
        CanvasGroup canvasGroup = mainCanvas.GetComponent<CanvasGroup>();
        if(canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, 0.8f);
        }
    }
}
