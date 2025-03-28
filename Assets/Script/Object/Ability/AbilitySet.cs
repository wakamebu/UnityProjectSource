using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityDataSet", menuName = "CharacterData/AbilityDataSet")]
public class AbilityDataSet : ScriptableObject
{
    [Header("探索・行動スキル一覧")]
    public AbilityDataSO[] searchAndActionAbilities;

    [Header("戦闘・交渉スキル一覧")]
    public AbilityDataSO[] combatAndNegotiationAbilities;

    [Header("知識スキル一覧")]
    public AbilityDataSO[] knowledgeAndLanguageAbilities;

    // 技能一覧
    public AbilityDataSO[] allAbilities
    {
        get
        {
            HashSet<AbilityDataSO> uniqueAbilities = new HashSet<AbilityDataSO>();
            
            foreach (var ability in searchAndActionAbilities)
            {
                uniqueAbilities.Add(ability);
            }
            
            foreach (var ability in combatAndNegotiationAbilities)
            {
                uniqueAbilities.Add(ability);
            }
            
            foreach (var ability in knowledgeAndLanguageAbilities)
            {
                uniqueAbilities.Add(ability);
            }

            return new List<AbilityDataSO>(uniqueAbilities).ToArray();
        }
    }
}
