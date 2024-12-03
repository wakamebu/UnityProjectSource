using UnityEngine;

[CreateAssetMenu(fileName = "BaseSkillData", menuName = "Skill/BaseSkill")]
public class BaseSkillData : ScriptableObject
{
    public string skillName;
    public string description;
    public int baseSkillDamage;
    public int baseBonus; //SANc成功時にもらえるボーナス
    public int baseMPCost;  
    public string baseSANCos;
    public int maxSyncLevel; //最大同期レベル

    //同期レベルによるボーナス係数
    //public float bonusSANCos;
}
