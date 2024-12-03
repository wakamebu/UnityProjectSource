using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityDataSet", menuName = "CharacterData/AbilityDataSet")]
public class AbilityDataSet : ScriptableObject
{
    public string setName; //例：初期用データなど
    public List<AbilityData> AbilityList;
}
