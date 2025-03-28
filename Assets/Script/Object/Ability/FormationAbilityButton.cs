using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FormationAbilityButton : MonoBehaviour
{
    public TMP_Text abilityNameText;
    public TMP_Text growValueText;
    public Button learnButton;
    public Button levelUpButton;
    public Button levelDownButton;

    private AbilityDataSO abilityData;
    private int growValue;
    private bool isLearned;

    private FormationCharacterLevelUP characterLevelUP;

    private PlayerData tempPlayerData;
    private PlayerData nowPlayerData;

    public void Initialize(
        AbilityDataSO data, 
        int grow, 
        bool learned, 
        AbilityCategory category, 
        int abilityIndex, 
        PlayerData tempData, 
        PlayerData nowData,
        Color valueColor)
    {
        if (data == null || tempData == null || nowData == null)
        {
            Debug.LogError("Required data is null in FormationAbilityButton.Initialize");
            return;
        }

        abilityData = data;
        growValue = grow;
        isLearned = learned;
        tempPlayerData = tempData;
        nowPlayerData = nowData;

        characterLevelUP = FormationCharacterLevelUP.instance;

        UpdateUI(valueColor);

        learnButton.onClick.RemoveAllListeners();
        learnButton.onClick.AddListener(() => characterLevelUP.OnClickGetAbility(abilityData));

        levelUpButton.onClick.RemoveAllListeners();
        levelUpButton.onClick.AddListener(() => characterLevelUP.OnClickAbilityLevelUp(abilityData));

        levelDownButton.onClick.RemoveAllListeners();
        levelDownButton.onClick.AddListener(() => characterLevelUP.OnClickAbilityLevelDown(abilityData));
    }

    void UpdateUI(Color valueColor)
    {
        if (abilityData == null || nowPlayerData == null)
        {
            Debug.LogError("Required data is null in FormationAbilityButton.UpdateUI");
            return;
        }

        abilityNameText.text = abilityData.abilityName;
        growValueText.text = (abilityData.defaultValue + growValue).ToString();
        growValueText.color = valueColor;

        learnButton.gameObject.SetActive(!isLearned);
        levelUpButton.gameObject.SetActive(isLearned);

        int nowGrowValue = GetNowGrowValue();
        levelDownButton.gameObject.SetActive(isLearned && growValue > nowGrowValue);
    }

    private int GetNowGrowValue()
    {
        if (nowPlayerData == null || abilityData == null)
        {
            Debug.LogError("Required data is null in FormationAbilityButton.GetNowGrowValue");
            return 0;
        }

        SkillState nowState;
        if (nowPlayerData.skillStates.TryGetValue(abilityData.abilityID, out nowState))
        {
            return nowState.growValue;
        }
        return 0;
    }
}
