using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class FormationCharacterLevelUP : MonoBehaviour
{
    // UI 要素
    [Header("Tab Toggles")] 
    public Toggle[] tabToggles;

    //Panel(タブで切り替える対象)
    public GameObject statusPanel;
    public GameObject abilityPanel;
    public GameObject artsPanel;
    public GameObject corePanel;

    //UI表示フィールド
    public TMP_Text nameTextFC;
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
    //BonusPoints
    public TMP_Text attributeBonusPointsValue;
    public TMP_Text abilityBonusPointsValue;
    public TMP_Text atleastArtsSoulValue;
    public TMP_Text atleastCoresSoulValue;

    //内部データ
    private PlayerData nowPlayerData;
    private PlayerManager playerManager;
    private FormationSelectScript formationSelectScript;
    public Sprite selectedCharacterSpirite;
    private int selectedCharacterIndex;
    private int gainedSoulTotal;
    private int nowLevel;
    private int baseHP;
    private int baseMP;
    private int baseSAN;
    private int baseConf;
    private int tempLevel;
    private int tempLevelup;
    private int tempUsedSoul;
    private int tempBonusPoints;

    [SerializeField] private Button levelUpButton;
    [SerializeField] private Button levelDownButton;
    //確認パネル
    [SerializeField] private GameObject levelUpConfirmPanel;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    public static FormationCharacterLevelUP instance;

    void Awake()
    {
        // 初期は非表示
        if(levelUpConfirmPanel != null) levelUpConfirmPanel.SetActive(false);
        if(levelDownButton != null) levelDownButton.gameObject.SetActive(false);
    }

    void Start()
    {
        if(instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }

        if(levelUpConfirmPanel != null) levelUpConfirmPanel.SetActive(false);

        playerManager = PlayerManager.instance;

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
        nowPlayerData = playerData;
        StartCoroutine(GetTotalGainedSoul(nowPlayerData));

        //文字型に治す
        if(nameTextFC != null) nameTextFC.text = nowPlayerData.playerName;
        if(levelText != null) levelText.text = nowPlayerData.level.ToString();
        nowLevel = nowPlayerData.level;

        if(strengthValue != null) strengthValue.text = nowPlayerData.strength.ToString();
        if(dexterityValue != null) dexterityValue.text = nowPlayerData.dexterity.ToString();
        if(inteligenceValue != null) inteligenceValue.text = nowPlayerData.inteligence.ToString();
        if(constitutionValue != null) constitutionValue.text = nowPlayerData.constitution.ToString();
        int con = nowPlayerData.constitution;
        if(powerValue != null) powerValue.text = nowPlayerData.power.ToString();
        if(appearanceValue != null) appearanceValue.text = nowPlayerData.appearance.ToString();
        if(sizeValue != null) sizeValue.text = nowPlayerData.size.ToString();
        if(educationValue != null) educationValue.text = nowPlayerData.education.ToString();
        
        if(hpMaxValue != null) hpMaxValue.text = nowPlayerData.hp.ToString();
        int hp = nowPlayerData.hp;
        if(mpMaxValue != null) mpMaxValue.text = nowPlayerData.mp.ToString();
        if(sanMaxValue != null) sanMaxValue.text = nowPlayerData.san.ToString();
        
        //selectedCharacterSpirite = nowPlayerData.characterSprite;
        CalculateBaseStat(con,nowPlayerData.size,nowPlayerData.power,nowLevel,nowPlayerData);
        CalculateStrDamageBonus(nowPlayerData.strength);
        CalculateDexDamageBonus(nowPlayerData.dexterity);
        CalculateIntDamageBonus(nowPlayerData.inteligence);
        RecalculateBonusPoints();

        int levelUpCost = GetLevelUpCost(nowLevel);
        nextLevelRemainingText.text = levelUpCost.ToString();
        bool canLevelUp = (gainedSoulTotal >= levelUpCost && levelUpCost > 0);
        levelUpButton.gameObject.SetActive(canLevelUp);

        tempLevel = nowPlayerData.level;
        tempUsedSoul = nowPlayerData.usedSoul;
        RecalcTempUI();

        playerManager.SavePlayerData();
    }

    void CalculateBaseStat(int con,int size,int pow,int lv,PlayerData nowPlayerData)
    {
        //HP
        int totalHpBase = con + size;
        int lavelbonus = lv * 2;
        int baseFifth = Mathf.FloorToInt(totalHpBase / 5);
        baseHP = baseFifth + lavelbonus;
        if(hpMaxValue != null)hpMaxValue.text = baseHP.ToString();
        nowPlayerData.hp = baseHP;

        //MP
        int powFifth = Mathf.FloorToInt(pow / 5);
        baseMP = powFifth + lv;
        if(mpMaxValue != null)mpMaxValue.text = baseMP.ToString();
        nowPlayerData.mp = baseMP;

        //SAN
        baseSAN = pow;
        if(sanMaxValue != null)sanMaxValue.text = baseSAN.ToString();
        nowPlayerData.san = baseSAN;

        //AP
        int baseAP = 6;
        int bonusAP = Mathf.FloorToInt(lv / 5);
        int totalAP = baseAP + bonusAP;
        if(apMaxValue != null)apMaxValue.text = totalAP.ToString();
        nowPlayerData.ap = totalAP;

        CalculateConfuse(con,baseHP,nowPlayerData);
    }

    void CalculateConfuse(int con,int hp,PlayerData nowPlayerData)
    {
        int total = 0;
        int conFifth = Mathf.FloorToInt(con / 5);
        int hpThird = Mathf.FloorToInt(hp/3);
        total += conFifth + hpThird;
        if(confuseMaxValue != null)confuseMaxValue.text = total.ToString();
        baseConf = total;
        nowPlayerData.confuse = total;
    }

    void CalculateStrDamageBonus(int str)
    {
        int baseStrDamageBonus = 0;
        string strBonusText = "1d";
        if(str < 15)
        {
            baseStrDamageBonus = -6;
        }else if(str < 30)
        {
            baseStrDamageBonus = -4;
        }else if(str < 40)
        { 
            baseStrDamageBonus = -3;
        }else if(str < 50)
        {
            baseStrDamageBonus = -2;
        }else if(str <65)
        {
            baseStrDamageBonus = 0;
        }else if(str < 75)
        {
            baseStrDamageBonus = 1;
        }else if(str < 90)
        {
            baseStrDamageBonus = 2;
        }else if(str < 100)
        {
            baseStrDamageBonus = 3;
        }else if(str < 110)
        {
            baseStrDamageBonus = 4;
        }else if(str < 120)
        {
            baseStrDamageBonus = 5;
        }else if(str < 140)
        {
            baseStrDamageBonus = 6;
        }else if(str < 160)
        {
            baseStrDamageBonus = 7;
        }else if(str < 180)
        {
            baseStrDamageBonus = 8;
        }else if(str < 200)
        {
            baseStrDamageBonus = 9;
        }else{
            baseStrDamageBonus = 10;
        }
        strBonusText += baseStrDamageBonus.ToString();
        if(baseStrDamageBonus < 0)
        {
            baseStrDamageBonus = baseStrDamageBonus * -1;
            strBonusText = "-1d" + baseStrDamageBonus.ToString();
        }
        if(strBonusText == "1d0")strBonusText = "0";
        if(strBonusText == "1d1")strBonusText = "1";
        if(strDamageBonusValue != null)strDamageBonusValue.text = strBonusText;
    }

    void CalculateDexDamageBonus(int dex)
    {
        int baseDexDamageBonus = 0;
        string dexBonusText = "1d";
        if(dex < 15)
        {
            baseDexDamageBonus = -6;
        }else if(dex < 30)
        {
            baseDexDamageBonus = -4;
        }else if(dex < 40)
        { 
            baseDexDamageBonus = -3;
        }else if(dex < 50)
        {
            baseDexDamageBonus = -2;
        }else if(dex <65)
        {
            baseDexDamageBonus = 0;
        }else if(dex < 75)
        {
            baseDexDamageBonus = 1;
        }else if(dex < 90)
        {
            baseDexDamageBonus = 2;
        }else if(dex < 100)
        {
            baseDexDamageBonus = 3;
        }else if(dex < 110)
        {
            baseDexDamageBonus = 4;
        }else if(dex < 120)
        {
            baseDexDamageBonus = 5;
        }else if(dex < 140)
        {
            baseDexDamageBonus = 6;
        }else if(dex < 160)
        {
            baseDexDamageBonus = 7;
        }else if(dex < 180)
        {
            baseDexDamageBonus = 8;
        }else if(dex < 200)
        {
            baseDexDamageBonus = 9;
        }else{
            baseDexDamageBonus = 10;
        }
        dexBonusText += baseDexDamageBonus.ToString();
        if(dexBonusText == "1d0")dexBonusText = "0";
        if(dexBonusText == "1d1")dexBonusText = "1";
        if(baseDexDamageBonus < 0)
        {
            baseDexDamageBonus = baseDexDamageBonus * -1;
            dexBonusText = "-1d" + baseDexDamageBonus.ToString();
        }
        if(dexDamageBonusValue != null)dexDamageBonusValue.text = dexBonusText;
    }

    void CalculateIntDamageBonus(int intel)
    {
        int baseIntDamageBonus = 0;
        string intBonusText = "1d";
        if(intel < 15)
        {
            baseIntDamageBonus = -6;
        }else if(intel < 30)
        {
            baseIntDamageBonus = -4;
        }else if(intel < 40)
        { 
            baseIntDamageBonus = -3;
        }else if(intel < 50)
        {
            baseIntDamageBonus = -2;
        }else if(intel <65)
        {
            baseIntDamageBonus = 0;
        }else if(intel < 75)
        {
            baseIntDamageBonus = 1;
        }else if(intel < 90)
        {
            baseIntDamageBonus = 2;
        }else if(intel < 100)
        {
            baseIntDamageBonus = 3;
        }else if(intel < 110)
        {
            baseIntDamageBonus = 4;
        }else if(intel < 120)
        {
            baseIntDamageBonus = 5;
        }else if(intel < 140)
        {
            baseIntDamageBonus = 6;
        }else if(intel < 160)
        {
            baseIntDamageBonus = 7;
        }else if(intel < 180)
        {
            baseIntDamageBonus = 8;
        }else if(intel < 200)
        {
            baseIntDamageBonus = 9;
        }else{
            baseIntDamageBonus = 10;
        }
        intBonusText += baseIntDamageBonus.ToString();
        if(intBonusText == "1d0")intBonusText = "0";
        if(intBonusText == "1d1")intBonusText = "1";
        if(baseIntDamageBonus < 0)
        {
            baseIntDamageBonus = baseIntDamageBonus * -1;
            intBonusText = "-1d" + baseIntDamageBonus.ToString();
        }
        if(intDamageBonusValue != null)intDamageBonusValue.text = intBonusText;
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
        
        playerManager.SavePlayerData();
    }

    public IEnumerator GetTotalGainedSoul(PlayerData playerData)
    {
        const string expUrl = "http://fproject.starfree.jp/updates/exp.json";
        UnityWebRequest request = UnityWebRequest.Get(expUrl);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("check failed: " + request.error);
        }
        else
        {
            // JSON文字列を取得
            string json = request.downloadHandler.text;

            // ExpData にパース
            ExpData expData = JsonUtility.FromJson<ExpData>(json);

            if (expData != null)
            {
                int serverExp = expData.totalExperience;
                Debug.Log($"サーバーから取得した経験値: {serverExp}");

                if (playerData != null)
                {
                    if (playerData.experience != serverExp)
                    {
                        Debug.Log("経験値を更新します。");
                        playerData.experience = serverExp;
                        playerManager.SavePlayerData();
                    }
                    else
                    {
                        Debug.Log("経験値は最新です。");
                    }
                }
            }
            else
            {
                Debug.LogWarning("expData is null. JSONのパースに失敗したかも。");
            }
        }
        
        RecalculateSouls(playerData);
    }

    // ボーナスポイントの再計算
    public void RecalculateBonusPoints()
    {
        UnityEngine.Debug.Log("ボーナスポイント計算開始");

        //lvごとに5ポイントのステータスと技能ポイントを獲得 ただし、1lv目は0
        int bonusPoints = 0;
        int lv = nowPlayerData.level - 1;
        bonusPoints = lv * 5;
        int bonusPointsFromAttribute = bonusPoints;
        int bonusPointsFromAbility = bonusPoints;

        if(nowPlayerData.usedAttributeBonus != 0)
        {
            bonusPointsFromAttribute -= nowPlayerData.usedAttributeBonus;
        }
        if(nowPlayerData.usedAbilityBonus != 0)
        {
            bonusPointsFromAbility -= nowPlayerData.usedAbilityBonus;
        }

        if(attributeBonusPointsValue != null)attributeBonusPointsValue.text = "残りポイント: " + bonusPointsFromAttribute.ToString();
        if(abilityBonusPointsValue != null)abilityBonusPointsValue.text = "残りポイント: " + bonusPointsFromAbility.ToString();
    }

    //level → level+1 に必要なコスト
    public static int GetLevelUpCost(int level)
    {
        // レベルが 1 未満なら想定外として 0 を返す
        if (level < 1) 
            return 0;

        // レベル1～9: レベル×100
        if (level < 10) // (1 <= level <= 9)
        {
            return level * 100;
        }
        // レベル10～18: 10→11で1000、その後 +200 ずつ増加
        //  10->11=1000, 11->12=1200, 12->13=1400, ..., 18->19=2600
        else if (level < 19) // (10 <= level <= 18)
        {
            // level=10 → 1000 + 200×(10-10)=1000
            // level=11 → 1000 + 200×(11-10)=1200
            // ...
            // level=18 → 1000 + 200×(8)=2600
            return 1000 + 200 * (level - 10);
        }
        // レベル19だけ特別に 3000 (19->20)
        else if (level == 19)
        {
            return 3000; 
        }
        // レベル20～29: 20->21=3400から +400 ずつ増加
        //  20->21=3400, 21->22=3800, 22->23=4200, ..., 29->30=7000
        else if (level < 30) // (20 <= level <= 29)
        {
            // level=20 → 3000 + 400×(20-19)=3400
            // level=21 → 3000 + 400×(2)=3800
            // ...
            // level=29 → 3000 + 400×(10)=7000
            return 3000 + 400 * (level - 19);
        }
        // レベル30以上はもう上がらない想定（または別途定義）
        return 0;
    }

    // ソウルの再計算
    public void RecalculateSouls(PlayerData playerData)
    {
        gainedSoulTotal = nowPlayerData.experience - nowPlayerData.usedSoul;
        gainedSoulText.text = gainedSoulTotal.ToString();
        
        atleastArtsSoulValue.text = "残りソウル: " + gainedSoulTotal.ToString();
        atleastCoresSoulValue.text = "残りソウル: " + gainedSoulTotal.ToString(); 
    }

    public void OnclickLevelUpButton()
    {
        if(tempLevel >= 30)
        {
            UnityEngine.Debug.Log("これ以上レベルを上げられません");
            return;
        }

        int cost = GetLevelUpCost(tempLevel); 
        int tempRemainSoul = nowPlayerData.experience - tempUsedSoul;

        if(tempRemainSoul >= cost && cost > 0)
        {
            // レベルを1上げる
            tempLevel++;
            // 使った分だけ tempUsedSoul を増やす
            tempUsedSoul += cost;
            // ボーナスポイントを加算 (1レベル上がるごとに5ポイント、など)
            tempBonusPoints += 5;

            // UIを更新
            RecalcTempUI();
        }
        else
        {
            Debug.Log("ソウルが足りない or cost=0");
        }
    }

    public void OnclickLevelDownButton()
    {
        if(tempLevel > nowPlayerData.level)
        {
            // tempLevelから1引いて、そのコストを戻す必要がある

            // 下げる前のレベル = tempLevel-1 → tempLevel
            // たとえば今tempLevelが10なら「(9->10)に使ったコスト」を取り返す
            int cost = GetLevelUpCost(tempLevel - 1);
            
            tempLevel--;
            tempUsedSoul -= cost; // 使ったソウルを返す
            tempBonusPoints -= 5; // 付与したボーナスポイントも取り消す

            // 念のため0未満にならないようClamp
            if(tempUsedSoul < nowPlayerData.usedSoul) 
            {
                tempUsedSoul = nowPlayerData.usedSoul;
            }
            if(tempBonusPoints < 0) tempBonusPoints = 0;

            RecalcTempUI();
        }
    }

    public void LevelUp(int tempLevelup)
    {
        if (gainedSoulTotal >= 0)
        {
            nowPlayerData.usedSoul += tempUsedSoul;
            nowPlayerData.level += tempLevelup;
            tempLevelup = 0;
            tempUsedSoul = 0;
            levelText.text = nowPlayerData.level.ToString();
            RecalculateSouls(nowPlayerData);
            CalculateBaseStat(nowPlayerData.constitution,nowPlayerData.size,nowPlayerData.power,nowPlayerData.level,nowPlayerData);
            CalculateStrDamageBonus(nowPlayerData.strength);
            CalculateDexDamageBonus(nowPlayerData.dexterity);
            CalculateIntDamageBonus(nowPlayerData.inteligence);

            levelText.color = Color.white;
            attributeBonusPointsValue.color = Color.white;
            playerManager.SavePlayerData();
        }
        else
        {
            Debug.Log("ソウルが足りません");
        }
    }

    private void ShowLevelUpConfirmPanel()
    {
        if(levelUpConfirmPanel != null)
        {
            levelUpConfirmPanel.SetActive(true);
        }
    }

    public void OnConfirmLevelUp()
    {
        nowPlayerData.level = tempLevel;
        nowPlayerData.usedSoul = tempUsedSoul;
        //後日、ボーナスはどうするか決める。

        levelUpConfirmPanel.SetActive(false);
        RecalculateSouls(nowPlayerData);
        RecalculateBonusPoints();
        playerManager.SavePlayerData();
    }

    public void OnCancelLevelUp()
    {
        tempLevel = nowPlayerData.level;
        tempUsedSoul = nowPlayerData.usedSoul;
        tempBonusPoints = nowPlayerData.usedAttributeBonus;
        RecalcTempUI();
        levelUpConfirmPanel.SetActive(false);
    }

    private void RecalcTempUI()
    {
        // 仮レベル表示を更新
        if(levelText != null)
        {
            levelText.text = tempLevel.ToString();
            levelText.color = (tempLevel == nowPlayerData.level) ? Color.white : Color.green;
        }

        // 仮ソウル残量
        int tempRemainSoul = nowPlayerData.experience - tempUsedSoul;
        if(gainedSoulText != null)
        {
            gainedSoulText.text = tempRemainSoul.ToString();
        }

        // ボーナスポイント表示
        // たとえば、 attributeBonusPointsValue には tempBonusPoints 表示
        if(attributeBonusPointsValue != null)
        {
            attributeBonusPointsValue.text = $"残りポイント: {tempBonusPoints}";
            attributeBonusPointsValue.color = (tempBonusPoints > 0) ? Color.green : Color.white;
        }

        // レベルダウンボタンの表示切り替え
        if(levelDownButton != null)
        {
            levelDownButton.gameObject.SetActive(tempLevel > nowPlayerData.level);
        }

        //ConfirmPanelの表示切替
        if(tempLevel > nowPlayerData.level)
        {
            ShowLevelUpConfirmPanel();
        }
        else
        {
            if(levelUpConfirmPanel != null)
            {
                levelUpConfirmPanel.SetActive(false);
            }
        }

        // レベルアップボタンの有効/無効
        //   次のレベルアップに必要なコストが払えるかどうか
        if(levelUpButton != null)
        {
            int cost = GetLevelUpCost(tempLevel);
            if(tempLevel >= 30 || (tempRemainSoul < cost))
            {
                levelUpButton.interactable = false;
            }
            else
            {
                levelUpButton.interactable = true;
            }
        }
    }


}
