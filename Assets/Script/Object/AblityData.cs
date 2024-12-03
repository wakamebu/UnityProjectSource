using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityData", menuName = "CharacterData/AbilityData")]
public class AbilityData : ScriptableObject
{
    //技能
    public string abilityName;
    public int initialValue;
    public int growValue;
    public string description;
}
