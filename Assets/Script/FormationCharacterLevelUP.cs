using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FormationCharacterLevelUP : MonoBehaviour
{
    // UI 要素
    [Header("UI Elements")]
    public Toggle[] tabToggles;
    public GameObject statusPanel;
    public GameObject abilityPanel;
    public GameObject artsPanel;
    public GameObject corePanel;

    private TMP_Text nameText;
    public TMP_Text levelText;
    public TMP_Text gainedSoulText;
    public TMP_Text nextLevelRemainingText;
    //Attribute
    public TMP_Text remainingBonusAttributePoints;
    public TMP_Text strengthValue;
    public TMP_Text dexterityValue;
    public TMP_Text inteligenceValue;
    public TMP_Text constitutionValue;
    public TMP_Text powerValue;
    public TMP_Text appearanceValue;
    public TMP_Text sizeValue;
    public TMP_Text educationValue;
    //BaseStats
    public TMP_Text hpMaxValue;
    public TMP_Text mpMaxValue;
    public TMP_Text sanMaxValue;
    public TMP_Text confuseMaxValue;
    public TMP_Text apMaxValue;
    public TMP_Text strDamageBonusValue;
    public TMP_Text dexDamageBonusValue;
    public TMP_Text intDamageBonusValue;

    //内部データ
    private PlayerData nowPlayerData;
    private PlayerManager playerManager;
    private FormationSelectScript formationSelectScript;
    private int selectedCharacterIndex;
    private int gainedSoulTotal;
    private int nowLevel;
    public int baseHP;
    public int baseConf;

    public static FormationCharacterLevelUP instance;

    void Start()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }
        //init
        for(int i = 0; i < tabToggles.Length;i++)
        {
            int index = i;
            tabToggles[i].onValueChanged.AddListener((bool value) => {
                if(value)
                {
                    selectToggles(index);
                }
            });
            Debug.Log("トグルのリスナーを設定");
        }

        if(tabToggles.Length > 0)
        {
            tabToggles[0].isOn = true;
        }
    }

    public void selectCharaterUpdate(PlayerData playerData)
    {
        gainedSoulTotal = playerData.experience;
        //nowPlayerData = playerData;
        if(nameText != null) nameText.text = playerData.playerName;
        if(levelText != null) levelText.text = playerData.level.ToString();
        nowLevel = playerData.level;

        if(strengthValue != null) strengthValue.text = playerData.strength.ToString();
        if(dexterityValue != null) dexterityValue.text = playerData.dexterity.ToString();
        if(inteligenceValue != null) inteligenceValue.text = playerData.inteligence.ToString();
        if(constitutionValue != null) constitutionValue.text = playerData.constitution.ToString();
        int con = playerData.constitution;
        if(powerValue != null) powerValue.text = playerData.power.ToString();
        if(appearanceValue != null) appearanceValue.text = playerData.appearance.ToString();
        if(sizeValue != null) sizeValue.text = playerData.size.ToString();
        if(educationValue != null) educationValue.text = playerData.education.ToString();
        
        if(hpMaxValue != null) hpMaxValue.text = playerData.hp.ToString();
        int hp = playerData.hp;
        if(mpMaxValue != null) mpMaxValue.text = playerData.mp.ToString();
        if(sanMaxValue != null) sanMaxValue.text = playerData.san.ToString();
        //以下必要なデータ更新

        CalculateConfuse(con,hp);
    }

    void CalculateConfuse(int con,int hp)
    {
        int total = 0;
        int conFifth = Mathf.FloorToInt(con / 5);
        int hpThird = Mathf.FloorToInt(hp/3);
        total += conFifth + hpThird;
        if(confuseMaxValue != null)confuseMaxValue.text = total.ToString();
        baseConf = total;
    }

    void selectToggles(int index)
    {
        //Status Ability Arts Coreの順番
        statusPanel.SetActive(index == 0);
        abilityPanel.SetActive(index == 1);
        artsPanel.SetActive(index == 2);
        corePanel.SetActive(index == 3);

        if (statusPanel.activeSelf == true){
            for (int i = 1; i < tabToggles.Length ; i++){
                tabToggles[i].isOn = false;
            }
        } else if (abilityPanel.activeSelf == true){
            for (int i = 0 ; i <tabToggles.Length ; i++){
                if (i == 1) continue;
                tabToggles[i].isOn = false;
            }
        } else if (artsPanel.activeSelf == true){
            for (int i = 0 ; i <tabToggles.Length ; i++){
                if (i == 2) continue;
                tabToggles[i].isOn = false;
            }
        } else if (corePanel.activeSelf == true){
            for (int i = 0 ; i <tabToggles.Length ; i++){
                if (i == 3) continue;
                tabToggles[i].isOn = false;
            }
        }
    }

}
