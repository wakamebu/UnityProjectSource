using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AbilityDataSO", menuName = "CharacterData/AbilityDataSO")]
public class AbilityDataSO : ScriptableObject
{
    //技能
    [SerializeField]
    // 001などを扱うため
    public string abilityID;
    public string abilityName;
    public int defaultValue;
    public int learnCost = 0;
    public int defaultLearnBool;
    //技能の説明
    [TextArea(1,6)]
    public string abilityDescription;

    public AbilityCategory category;

}