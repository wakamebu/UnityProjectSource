using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager instance;

    public GameObject tooltipObject;

    //すべてのツールチップはタイトル＋説明の形を取る
    public TMP_Text titleText;
    public TMP_Text descriptionText;

    // Start is called before the first frame update
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

    // Update is called once per frame
    void Update()
    {
        if(tooltipObject != null){
            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                tooltipObject.transform.parent as RectTransform,
                Input.mousePosition, null, out position);
            tooltipObject.transform.localPosition = position;
        }
    }

    public static void Show(string title, string description)
    {
        Debug.Log("Tooltipを表示します");
        if (instance != null)
        {
            instance.titleText.text = title;
            instance.descriptionText.text = description;
            instance.tooltipObject.SetActive(true);
            Debug.Log("Tooltipを表示しました");
        }
    }

    public static void Hide()
    {
        if (instance != null)
        {
            instance.tooltipObject.SetActive(false);
        }
    }
}
