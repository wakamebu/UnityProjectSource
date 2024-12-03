using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using UnityEngine.EventSystems;

public class TutorialManager : MonoBehaviour
{
    public Button tutorialButton;
    public GameObject tutorialPanel;
    public Image tutorialImage;
    public Button nextButton;
    public Button prevButton;

    public List<Sprite> tutorialSprites;

    private int currentIndex = 0;

    /*/ 将来的に色々なシーンにチュートリアルボタンを配置し、現在読み込まれているシーンに応じて表示するチュートリアルを変更する
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            tooltipObject.SetActive(false);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    //*/

    void Start()
    {
        tutorialPanel.SetActive(false);

        tutorialButton.onClick.AddListener(ShowTutorial);
        nextButton.onClick.AddListener(ShowNextImage);
        prevButton.onClick.AddListener(ShowPreviousImage);
    }

    void ShowTutorial()
    {
        currentIndex = 0;
        Debug.Log("Tutorialを表示します");
        tutorialPanel.SetActive(true);
        UpdateTutorialImage();
    }

    void HideTutorial()
    {
        tutorialPanel.SetActive(false);
        Debug.Log("Tutorialを消しました");
    }

    void ShowNextImage()
    {
        if (currentIndex < tutorialSprites.Count - 1)
        {
            currentIndex++;
            UpdateTutorialImage();
        }
        else
        {
            HideTutorial();
        }
        //選択しているボタンがない状態にする（SelectedSpriteを毎回機能させたいため）
        EventSystem.current.SetSelectedGameObject(null);
    }

    void ShowPreviousImage()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            UpdateTutorialImage();
        }
        EventSystem.current.SetSelectedGameObject(null);
    }

    void UpdateTutorialImage()
    {
        tutorialImage.sprite = tutorialSprites[currentIndex];

        prevButton.gameObject.SetActive(currentIndex > 0);
        nextButton.GetComponentInChildren<TextMeshProUGUI>().text = (currentIndex < tutorialSprites.Count - 1) ? "次へ" : "閉じる";
    }
}
