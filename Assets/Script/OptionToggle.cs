using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class OptionToggle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public OptionData optionData;
    private Toggle toggle;

    void Awake()
    {
        toggle = GetComponent<Toggle>();
    }

    // マウスがオプションに入ったとき
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("マウスを感知");
        string optionTooltip = "";
        if (optionData != null)
        {
            optionTooltip = "cost:" + optionData.cost + "\n";
            optionTooltip += "効果:" + optionData.effect;
            TooltipManager.Show(optionData.optionName,optionTooltip);
        }
        else
        {
            optionTooltip = "noData";
            Debug.Log("no Data");
        }
    }

    // マウスがオプションから出たとき
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("マウスが離れました");
        TooltipManager.Hide();
    }
}
