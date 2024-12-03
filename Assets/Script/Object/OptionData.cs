using UnityEngine;

[CreateAssetMenu(fileName = "OptionData", menuName = "Skill/Option")]
public class OptionData : ScriptableObject
{
    public string optionName;
    public int cost;
    [TextArea(1,6)]
    public string effect;
    public float successDamageMultiplier; //ダメージ倍率(20%なら0.2f)
    public float failDamageMultiplier;
    public string bonusCondition;  //"SANチェック成功時"
    public int mpCostIncrease;    // MPコスト増加量
    public int sanCostIncrease;   // SAN値コスト増加量
    public string constellation;  //炎 水 風 土 (氷) 光 闇 のどれか
}