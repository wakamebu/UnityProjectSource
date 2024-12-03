using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;

public class Feo
{
    public string skillName;
    public BaseSkillData baseSkillData;
    public AttributeData attributeData;
    public List<OptionData> optionDataList;
    public int syncLevel;

    public int totalFailDamage;
    public int totalSuccessDamage;
    public int totalMPCost;
    public int totalSANCost;

    // スキルの初期化
    public void Initialize()
    {
        skillName = baseSkillData.skillName;
        CalculateStats();
    }

    // ステータスを計算するメソッド
    private void CalculateStats()
    {
        int baseDamage = baseSkillData.baseSkillDamage;
        int baseBonus = baseSkillData.baseBonus;

        float syncDamageBonus = 0.8f;
        int syncLevelDamage = Mathf.FloorToInt(syncDamageBonus * Mathf.Pow(syncLevel, 2));

        float totalSuccessDamageMultiplier = optionDataList.Sum(option => option.successDamageMultiplier);
        float totalFailDamageMultiplier = optionDataList.Sum(option => option.failDamageMultiplier);

        float successDamageMultiplier = 1 + totalSuccessDamageMultiplier;
        float failDamageMultiplier = 1 + totalFailDamageMultiplier;

        totalFailDamage = Mathf.FloorToInt((baseDamage + syncLevelDamage) * failDamageMultiplier);
        totalSuccessDamage = Mathf.FloorToInt((baseDamage + syncLevelDamage + baseBonus) * successDamageMultiplier);

        //あとで再計算するやつ
        totalSANCost = syncLevel * 3 + optionDataList.Sum(option => option.sanCostIncrease);
        totalMPCost = baseSkillData.baseMPCost + optionDataList.Sum(option => option.mpCostIncrease);
    }
}

public class FEOManager : MonoBehaviour
{
    public static FEOManager instance;
    public List<Feo> feoList = new List<Feo>();

    void Awake()
    {
        // シングルトンの設定
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void LoadFEO(List<FeoData> feoDataList)
    {
        Debug.Log("LoadFEO Start");
        feoList.Clear();

        foreach (FeoData feoData in feoDataList)
        {
            Feo feo = CreateFeoFromData(feoData);
            if(feo != null){
                feoList.Add(feo);
                Debug.Log("Loaded FEO" + feo.skillName);
            }
            else
            {
                Debug.LogWarning("FEOのロードに失敗しました" + feoData.baseSkillName);
            }
        }
    }

    private Feo CreateFeoFromData(FeoData feoData)
    {
        BaseSkillData baseSkill = Resources.Load<BaseSkillData>("BaseSkills/" + feoData.baseSkillName);
        if(baseSkill == null){
            Debug.LogWarning("BaseSkillDataが見つかりません" + feoData.baseSkillName);
            return null;
        }

        AttributeData attributeData = Resources.Load<AttributeData>("Attributes/" + feoData.attributeName);
        if(attributeData == null){
            Debug.LogWarning("AttributeDataが見つかりません" + feoData.attributeName);
            return null;
        }

        List<OptionData> optionDataList = new List<OptionData>();
        foreach (string optionName in feoData.selectedOptionNames)
        {
            OptionData optionData = Resources.Load<OptionData>("Options/" + optionName);
            if(optionData != null){
                optionDataList.Add(optionData);
            }
            else
            {
                Debug.LogWarning("OptionDataが見つかりません" + optionName);
                return null;
            }
        }

        Feo feo = new Feo();
        feo.baseSkillData = baseSkill;
        feo.attributeData = attributeData;
        feo.optionDataList = optionDataList;
        feo.syncLevel = feoData.syncLevel;
        return feo;
    }
}
