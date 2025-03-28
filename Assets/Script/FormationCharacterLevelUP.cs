using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class FormationCharacterLevelUP : MonoBehaviour
{
    // UI 要素
    [Header("Tab Toggles")] 
    public Toggle[] tabToggles;

    //Panel(タブで切り替える対象)
    [Header("Panel")] 
    public GameObject statusPanel;
    public GameObject abilityPanel;
    public GameObject artsPanel;
    public GameObject corePanel;

    //UI表示フィールド
    [Header("UI Elements")] 
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
    private PlayerData tempPlayerData;
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
    private int remainingTempBonusAbilityPoint;

    [SerializeField] private Button levelUpButton;
    [SerializeField] private Button levelDownButton;

    // Size=8
    //  0:STR, 1:DEX, 2:INT, 3:CON, 4:POW, 5:APP, 6:SIZ, 7:EDU
    [SerializeField] private Button[] attributeUpButton;
    [SerializeField] private Button[] attributeDownButton;

    //確認パネル
    [SerializeField] private GameObject actionConfirmPanel;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    public static FormationCharacterLevelUP instance;

    // 保留中のアクション
    public enum PendingActionType
    {
        None,
        LevelUp,
        LevelDown,
        AttributeUp,
        AttributeDown,
        AcquireAbility,
        AbilityLevelUp,
        AbilityLevelDown
    }
    private PendingActionType pendingActionType = PendingActionType.None;

    // 保留中のアクションのパラメータ
    private int pendingLevel;
    private int pendingSoulCost;
    private string pendingAbilityID;
    private int pendingAbilityPointCost;
    private int pendingAttributeIndex;
    private int pendingAttributePointCost;

    [Header("Ability Master Set")]
    public AbilityDataSet abilityDataSet;

    [Header("Ability UI Elements")] 
    public GameObject abilityButtonPrefab;
    public Transform searchAndActionParent;
    public Transform battleAndNegotiationParent;
    public Transform knowledgeAndLanguageParent;

    void Awake()
    {
        // 初期は非表示
        if(actionConfirmPanel != null) actionConfirmPanel.SetActive(false);
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

        if(actionConfirmPanel != null) actionConfirmPanel.SetActive(false);

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
        
        // AttributeUpButton のリスナー設定
        for(int i = 0; i < attributeUpButton.Length; i++)
        {
            int index = i;
            attributeUpButton[i].onClick.AddListener(() => OnClickAttributeUpButton(index));
        }

        // AttributeDownButton のリスナー設定
        for(int i = 0; i < attributeDownButton.Length; i++)
        {
            int index = i;
            attributeDownButton[i].onClick.AddListener(() => OnClickAttributeDownButton(index));
        }

        if(tabToggles.Length > 0)
        {
            tabToggles[0].isOn = true;
        }
    }

    public async Task OpenFormationUI(PlayerData actualData)
    {
        // 実際のデータを記憶 (これがゲーム中の本番データ)
        this.nowPlayerData = actualData;

        // nowPlayerData の内容を tempPlayerData に複製
        tempPlayerData = new PlayerData(nowPlayerData);

        // 非同期でUIの更新を行う
        await Task.Run(() => {
            // データの初期化処理
            InitializeTempData();
        });

        // UIの更新はメインスレッドで実行
        RecalcTempUI();
    }

    private void InitializeTempData()
    {
        tempPlayerData.skillStates = new Dictionary<string, SkillState>();
        foreach (var kvp in nowPlayerData.skillStates)
        {
            SkillState copy = new SkillState
            {
                isLearned = kvp.Value.isLearned,
                growValue = kvp.Value.growValue
            };
            tempPlayerData.skillStates[kvp.Key] = copy;
        }
    }

    public void ShowActionConfirmPanel(PendingActionType actionType)
    {
        pendingActionType = actionType;
        if (actionConfirmPanel != null) actionConfirmPanel.SetActive(true);
    }

    public void HideActionConfirmPanel()
    {
        pendingActionType = PendingActionType.None;
        if (actionConfirmPanel != null) actionConfirmPanel.SetActive(false);
    }

    public async Task selectCharaterUpdate(PlayerData playerData)
    {
        nowPlayerData = playerData;
        await OpenFormationUI(nowPlayerData);
        await GetTotalGainedSoul(tempPlayerData);

        //文字型に治す
        if(nameTextFC != null) nameTextFC.text = tempPlayerData.playerName;
        if(levelText != null) levelText.text = tempPlayerData.level.ToString();

        if(strengthValue != null) strengthValue.text = tempPlayerData.strength.ToString();
        if(dexterityValue != null) dexterityValue.text = tempPlayerData.dexterity.ToString();
        if(inteligenceValue != null) inteligenceValue.text = tempPlayerData.inteligence.ToString();
        if(constitutionValue != null) constitutionValue.text = tempPlayerData.constitution.ToString();
        if(powerValue != null) powerValue.text = tempPlayerData.power.ToString();
        if(appearanceValue != null) appearanceValue.text = tempPlayerData.appearance.ToString();
        if(sizeValue != null) sizeValue.text = tempPlayerData.size.ToString();
        if(educationValue != null) educationValue.text = tempPlayerData.education.ToString();
        
        if(hpMaxValue != null) hpMaxValue.text = tempPlayerData.hp.ToString();
        if(mpMaxValue != null) mpMaxValue.text = tempPlayerData.mp.ToString();
        if(sanMaxValue != null) sanMaxValue.text = tempPlayerData.san.ToString();
        
        CalculateBaseStat(tempPlayerData);
        CalculateStrDamageBonus(tempPlayerData.strength);
        CalculateDexDamageBonus(tempPlayerData.dexterity);
        CalculateIntDamageBonus(tempPlayerData.inteligence);

        int levelUpCost = GetLevelUpCost(tempPlayerData.level);
        nextLevelRemainingText.text = levelUpCost.ToString();
        bool canLevelUp = (gainedSoulTotal >= levelUpCost && levelUpCost > 0);
        levelUpButton.gameObject.SetActive(canLevelUp);

        // アビリティリストを更新
        RefreshAbilityList();

        RecalcTempUI();
        playerManager.SavePlayerData();
    }

    void CalculateBaseStat(PlayerData tempPlayerData)
    {
        //HP
        int con = tempPlayerData.constitution;
        int size = tempPlayerData.size;
        int pow = tempPlayerData.power;
        int lv = tempPlayerData.level;
        con += tempPlayerData.bonusConstitution;
        size += tempPlayerData.bonusSize;
        int totalHpBase = Mathf.FloorToInt((Mathf.Floor((3 * size + 2 * con) / 4) + lv * 5) * 0.8f);
        if(hpMaxValue != null)hpMaxValue.text = totalHpBase.ToString();
        tempPlayerData.hp = totalHpBase;

        pow += tempPlayerData.bonusPower;
        //MP
        int powFifth = Mathf.FloorToInt(pow / 5);
        int totalMpBase = Mathf.FloorToInt((Mathf.Floor((powFifth + lv * 5) * 0.8f)));
        if(mpMaxValue != null)mpMaxValue.text = totalMpBase.ToString();
        tempPlayerData.mp = totalMpBase;

        //SAN
        int baseSAN = pow;
        if(sanMaxValue != null)sanMaxValue.text = baseSAN.ToString();
        tempPlayerData.san = baseSAN;

        //AP
        int baseAP = 6;
        int bonusAP = Mathf.FloorToInt(lv / 5);
        int totalAP = baseAP + bonusAP;
        if(apMaxValue != null)apMaxValue.text = totalAP.ToString();
        tempPlayerData.ap = totalAP;

        CalculateConfuse(tempPlayerData);
    }

    void CalculateConfuse(PlayerData tempPlayerData)
    {
        int total = 0;
        int conFifth = Mathf.FloorToInt(tempPlayerData.constitution / 3);
        int hpThird = Mathf.FloorToInt(tempPlayerData.hp/5);
        total += conFifth + hpThird;
        if(confuseMaxValue != null)confuseMaxValue.text = total.ToString();
        tempPlayerData.confuse = total;
    }

    void CalculateStrDamageBonus(int str)
    {
        int baseStrDamageBonus = 0;
        str += tempPlayerData.bonusStrength;

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
        int bonusDex = 0;
        bonusDex = tempPlayerData.bonusDexterity;
        dex += bonusDex;
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
        int bonusInt = 0;
        bonusInt = tempPlayerData.bonusInteligence;
        intel += bonusInt;
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
                RefreshAbilityList();
                tabToggles[i].isOn = false;
            }
            // アビリティタブが選択された時にUIを更新
            RecalcTempUI();
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

    public async Task GetTotalGainedSoul(PlayerData playerData)
    {
        const string expUrl = "https://fproject02.stars.ne.jp/updates/exp.json";
        using (UnityWebRequest request = UnityWebRequest.Get(expUrl))
        {
            var operation = request.SendWebRequest();
            await operation;

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
                            // nowPlayerDataとtempPlayerDataの両方に経験値を設定
                            nowPlayerData.experience = serverExp;
                            tempPlayerData.experience = serverExp;
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
        }
        
        RecalculateSouls(playerData);
        RecalcTempUI();
    }

    // ボーナスポイントの再計算
    private void RecalculateBonusPoints()
    {
        // レベルに応じて (レベル -1) * 5 が基礎ポイント
        int lv = tempPlayerData.level - 1;
        if(lv < 0) lv = 0;
        int basePoints = lv * 5;

        // 既に振り分けたポイント
        int usedAttributeBonusPoints = tempPlayerData.usedAttributeBonus;
        int usedAbilityBonusPoints = tempPlayerData.usedAbilityBonus;

        // 残り
        int remainAttributePoints = basePoints - usedAttributeBonusPoints;
        int remainAbilityPoints = basePoints - usedAbilityBonusPoints;
        if(remainAttributePoints < 0) remainAttributePoints = 0;

        // UIに表示
        if(attributeBonusPointsValue != null)
        {
            attributeBonusPointsValue.text = "残りポイント: " + remainAttributePoints;
            abilityBonusPointsValue.text = "残りポイント: " + remainAbilityPoints;
        }

        remainingTempBonusAbilityPoint = remainAbilityPoints;

        // Debug.Log($"[RecalculateAttributeBonusPoints] base:{basePoints}, used:{usedAttributeBonusPoints}, remainAttributePoints:{remainAttributePoints}");
        // Debug.Log($"[RecalculateAbilityBonusPoints] base:{basePoints}, used:{usedAbilityBonusPoints}, remainAbilityPoints:{remainAbilityPoints}");
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
        gainedSoulTotal = playerData.experience - playerData.usedSoul;
        gainedSoulText.text = gainedSoulTotal.ToString();
        
        atleastArtsSoulValue.text = "残りソウル: " + gainedSoulTotal.ToString();
        atleastCoresSoulValue.text = "残りソウル: " + gainedSoulTotal.ToString(); 
    }

    public void OnclickLevelUpButton()
    {
        if(tempPlayerData.level >= 30)
        {
            UnityEngine.Debug.Log("これ以上レベルを上げられません");
            return;
        }

        int cost = GetLevelUpCost(tempPlayerData.level); 
        int tempRemainSoul = tempPlayerData.experience - tempPlayerData.usedSoul;

        if(tempRemainSoul >= cost && cost > 0)
        {
            // レベルを1上げる
            tempPlayerData.level++;
            // 使った分だけ tempUsedSoul を増やす
            tempPlayerData.usedSoul += cost;
            RecalculateSouls(tempPlayerData);
            CalculateBaseStat(tempPlayerData);
            CalculateStrDamageBonus(tempPlayerData.strength);
            CalculateDexDamageBonus(tempPlayerData.dexterity);
            CalculateIntDamageBonus(tempPlayerData.inteligence);

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
        if(tempPlayerData.level > nowPlayerData.level)
        {
            // tempLevelから1引いて、そのコストを戻す必要がある

            // 下げる前のレベル = tempLevel-1 → tempLevel
            // たとえば今tempLevelが10なら「(9->10)に使ったコスト」を取り返す
            int cost = GetLevelUpCost(tempPlayerData.level - 1);
            
            tempPlayerData.level--;
            tempPlayerData.usedSoul -= cost; // 使ったソウルを返す

            // 念のため0未満にならないようClamp
            if(tempPlayerData.usedSoul < nowPlayerData.usedSoul) 
            {
                tempPlayerData.usedSoul = nowPlayerData.usedSoul;
            }
            RecalcTempUI();
        }
    }

    public void OnConfirmAction()
    {
        switch (pendingActionType)
        {
            case PendingActionType.None:
                break;
            case PendingActionType.LevelUp:
                ApplyChangesFromTemp();
                break;
            case PendingActionType.LevelDown:
                ApplyChangesFromTemp();
                break;
            case PendingActionType.AcquireAbility:
                ApplyChangesFromTemp();
                break;
            case PendingActionType.AttributeUp:
                ApplyChangesFromTemp();
                break;
            case PendingActionType.AttributeDown:
                ApplyChangesFromTemp();
                break;
            default:
                ApplyChangesFromTemp();
                break;
        }
        HideActionConfirmPanel();
        RecalculateSouls(nowPlayerData);
        RecalcTempUI();
        RefreshAbilityList();
        playerManager.SavePlayerData();
    }

    private void ApplyChangesFromTemp()
    {
        Debug.Log($"ApplyChangesFromTemp - Before - Experience: {nowPlayerData.experience}, UsedSoul: {nowPlayerData.usedSoul}");
        nowPlayerData.level = tempPlayerData.level;
        nowPlayerData.usedSoul = tempPlayerData.usedSoul;
        nowPlayerData.usedAttributeBonus = tempPlayerData.usedAttributeBonus;
        nowPlayerData.usedAbilityBonus = tempPlayerData.usedAbilityBonus;
        Debug.Log($"ApplyChangesFromTemp - After - Experience: {nowPlayerData.experience}, UsedSoul: {nowPlayerData.usedSoul}");

        nowPlayerData.bonusStrength = tempPlayerData.bonusStrength;
        nowPlayerData.bonusDexterity = tempPlayerData.bonusDexterity;
        nowPlayerData.bonusInteligence = tempPlayerData.bonusInteligence;
        nowPlayerData.bonusConstitution = tempPlayerData.bonusConstitution;
        nowPlayerData.bonusPower = tempPlayerData.bonusPower;
        nowPlayerData.bonusAppearance = tempPlayerData.bonusAppearance;
        nowPlayerData.bonusSize = tempPlayerData.bonusSize;
        nowPlayerData.bonusEducation = tempPlayerData.bonusEducation;

        // 基本ステータスの更新
        CalculateBaseStat(nowPlayerData);
        CalculateStrDamageBonus(nowPlayerData.strength);
        CalculateDexDamageBonus(nowPlayerData.dexterity);
        CalculateIntDamageBonus(nowPlayerData.inteligence);

        // スキル状態を更新
        nowPlayerData.skillStates.Clear();
        foreach (var kvp in tempPlayerData.skillStates)
        {
            SkillState newState = new SkillState
            {
                isLearned = kvp.Value.isLearned,
                growValue = kvp.Value.growValue
            };
            nowPlayerData.skillStates[kvp.Key] = newState;
        }

        // PlayerManagerのplayerDatasリスト内のデータも更新
        if (playerManager != null && playerManager.playerDatas != null)
        {
            int playerIndex = playerManager.playerSelectedindex;
            if (playerIndex >= 0 && playerIndex < playerManager.playerDatas.Count)
            {
                playerManager.playerDatas[playerIndex] = new PlayerData(nowPlayerData);
                Debug.Log($"PlayerManagerのplayerDatasリスト内のデータを更新しました。インデックス: {playerIndex}");
                Debug.Log($"更新後のプレイヤーデータ - レベル: {nowPlayerData.level}, HP: {nowPlayerData.hp}, MP: {nowPlayerData.mp}, SAN: {nowPlayerData.san}");
            }
        }

        // 確認後、tempPlayerDataをnowPlayerDataの状態に戻す
        tempPlayerData = new PlayerData(nowPlayerData);
        InitializeTempData();
    }

    public void OnCancelAction()
    {
        switch (pendingActionType)
        {
            case PendingActionType.AcquireAbility:
                CancelAbilityChanges();
                break;
            default:
                // tempPlayerDataをnowPlayerDataの状態に戻す
                tempPlayerData = new PlayerData(nowPlayerData);
                break;
        }
        RecalculateSouls(nowPlayerData);
        RecalcTempUI();
        HideActionConfirmPanel();
    }

    public void OnClickAttributeUpButton(int index)
    {
        int cost = 5;
        int remainingBonusPoints = (tempPlayerData.level - 1) * 5 - tempPlayerData.usedAttributeBonus;

        if(remainingBonusPoints >= cost)
        {
            tempPlayerData.usedAttributeBonus += cost;
            switch(index)
            {
                case 0: //STR
                    tempPlayerData.bonusStrength += cost;
                    break;
                case 1: // DEX
                    tempPlayerData.bonusDexterity += cost;
                    break;
                case 2: // INT
                    tempPlayerData.bonusInteligence += cost;
                    break;
                case 3: // CON
                    tempPlayerData.bonusConstitution += cost;
                    break;
                case 4: // POW
                    tempPlayerData.bonusPower += cost;
                    break;
                case 5: // APP
                    tempPlayerData.bonusAppearance += cost;
                    break;
                case 6: // SIZ
                    tempPlayerData.bonusSize += cost;
                    break;
                case 7: // EDU
                    tempPlayerData.bonusEducation += cost;
                    break;
            }
        }

        RecalcTempUI();
    }

    public void OnClickAttributeDownButton(int index)
    {
        int cost = 5;
        if(tempPlayerData.usedAttributeBonus > nowPlayerData.usedAttributeBonus)
        {
            switch(index)
            {
                case 0: //STR
                    if(tempPlayerData.bonusStrength > nowPlayerData.bonusStrength)
                    {
                        tempPlayerData.bonusStrength -= cost;
                        tempPlayerData.usedAttributeBonus -= cost;
                    }
                    break;
                case 1: // DEX
                    if(tempPlayerData.bonusDexterity > nowPlayerData.bonusDexterity)
                    {
                        tempPlayerData.bonusDexterity -= cost;
                        tempPlayerData.usedAttributeBonus -= cost;
                    }
                    break;
                case 2: // INT
                    if(tempPlayerData.bonusInteligence > nowPlayerData.bonusInteligence)
                    {
                        tempPlayerData.bonusInteligence -= cost;
                        tempPlayerData.usedAttributeBonus -= cost;
                    }
                    break;
                case 3: // CON
                    if(tempPlayerData.bonusConstitution > nowPlayerData.bonusConstitution)
                    {
                        tempPlayerData.bonusConstitution -= cost;
                        tempPlayerData.usedAttributeBonus -= cost;
                    }
                    break;
                case 4: // POW
                    if(tempPlayerData.bonusPower > nowPlayerData.bonusPower)
                    {
                        tempPlayerData.bonusPower -= cost;
                        tempPlayerData.usedAttributeBonus -= cost;
                    }
                    break;
                case 5: // APP
                    if(tempPlayerData.bonusAppearance > nowPlayerData.bonusAppearance)
                    {
                        tempPlayerData.bonusAppearance -= cost;
                        tempPlayerData.usedAttributeBonus -= cost;
                    }
                    break;
                case 6: // SIZ
                    if(tempPlayerData.bonusSize > nowPlayerData.bonusSize)
                    {
                        tempPlayerData.bonusSize -= cost;
                        tempPlayerData.usedAttributeBonus -= cost;
                    }
                    break;
                case 7: // EDU
                    if(tempPlayerData.bonusEducation > nowPlayerData.bonusEducation)
                    {
                        tempPlayerData.bonusEducation -= cost;
                        tempPlayerData.usedAttributeBonus -= cost;
                    }
                    break;
            }
        }
        RecalcTempUI();
    }

    //RecalcTempUI() で呼び出し、見た目のAttributeを更新
    private void UpdateAttributeDisplay()
    {
        int showStr = tempPlayerData.strength + tempPlayerData.bonusStrength;
        strengthValue.text = showStr.ToString();
        int nowStr = nowPlayerData.strength + nowPlayerData.bonusStrength;
        if (showStr > nowStr)
        {
            strengthValue.color = Color.green;
        }
        else
        {
            strengthValue.color = Color.white;
        }

        int showDex = tempPlayerData.dexterity + tempPlayerData.bonusDexterity;
        dexterityValue.text = showDex.ToString();
        int nowDex = nowPlayerData.dexterity + nowPlayerData.bonusDexterity;
        if (showDex > nowDex)
        {
            dexterityValue.color = Color.green;
        }
        else
        {
            dexterityValue.color = Color.white;
        }

        int showInt = tempPlayerData.inteligence + tempPlayerData.bonusInteligence;
        inteligenceValue.text = showInt.ToString();
        int nowInt = nowPlayerData.inteligence + nowPlayerData.bonusInteligence;
        if (showInt > nowInt)
        {
            inteligenceValue.color = Color.green;
        }
        else
        {
            inteligenceValue.color = Color.white;
        }

        int showCon = tempPlayerData.constitution + tempPlayerData.bonusConstitution;
        constitutionValue.text = showCon.ToString();
        int nowCon = nowPlayerData.constitution + nowPlayerData.bonusConstitution;
        if (showCon > nowCon)
        {
            constitutionValue.color = Color.green;
        }
        else
        {
            constitutionValue.color = Color.white;
        }

        int showPow = tempPlayerData.power + tempPlayerData.bonusPower;
        powerValue.text = showPow.ToString();
        int nowPow = nowPlayerData.power + nowPlayerData.bonusPower;
        if (showPow > nowPow)
        {
            powerValue.color = Color.green;
        }
        else
        {
            powerValue.color = Color.white;
        }

        int showApp = tempPlayerData.appearance + tempPlayerData.bonusAppearance;
        appearanceValue.text = showApp.ToString();
        int nowApp = nowPlayerData.appearance + nowPlayerData.bonusAppearance;
        if (showApp > nowApp)
        {
            appearanceValue.color = Color.green;
        }
        else
        {
            appearanceValue.color = Color.white;
        }

        int showSiz = tempPlayerData.size + tempPlayerData.bonusSize;
        sizeValue.text = showSiz.ToString();
        int nowSiz = nowPlayerData.size + nowPlayerData.bonusSize;
        if (showSiz > nowSiz)
        {
            sizeValue.color = Color.green;
        }
        else
        {
            sizeValue.color = Color.white;
        }

        int showEdu = tempPlayerData.education + tempPlayerData.bonusEducation;
        educationValue.text = showEdu.ToString();
        int nowEdu = nowPlayerData.education + nowPlayerData.bonusEducation;
        if (showEdu > nowEdu)
        {
            educationValue.color = Color.green;
        }
        else
        {
            educationValue.color = Color.white;
        }
    }

    public void RecalcTempUI()
    {
        // 仮レベル表示を更新
        if(levelText != null)
        {
            levelText.text = tempPlayerData.level.ToString();
            levelText.color = (tempPlayerData.level == nowPlayerData.level) ? Color.white : Color.green;
        }

        // 仮ソウル残量
        int tempRemainSoul = tempPlayerData.experience - tempPlayerData.usedSoul;
        if(gainedSoulText != null)
        {
            gainedSoulText.text = tempRemainSoul.ToString();
        }

        // ボーナスポイント表示
        if(attributeBonusPointsValue != null)
        {
            RecalculateBonusPoints();
        }

        // レベルダウンボタンの表示切り替え
        if(levelDownButton != null)
        {
            levelDownButton.gameObject.SetActive(tempPlayerData.level > nowPlayerData.level);
        }

        //ConfirmPanelの表示切替
        if(tempPlayerData.level > nowPlayerData.level || tempPlayerData.usedAttributeBonus > nowPlayerData.usedAttributeBonus)
        {
            ShowActionConfirmPanel(PendingActionType.LevelUp);
        }
        else
        {
            if(actionConfirmPanel != null)
            {
                actionConfirmPanel.SetActive(false);
            }
        }

        // レベルアップボタンの有効/無効
        //   次のレベルアップに必要なコストが払えるかどうか
        if(levelUpButton != null)
        {
            int cost = GetLevelUpCost(tempPlayerData.level);
            if(tempPlayerData.level >= 30 || (tempRemainSoul < cost))
            {
                levelUpButton.interactable = false;
                levelUpButton.gameObject.SetActive(false);
            }
            else
            {
                levelUpButton.interactable = true;
                levelUpButton.gameObject.SetActive(true);
            }
        }

        //ボーナスポイントの有効/無効
        if(attributeUpButton != null)
        {
            for(int i = 0; i < attributeUpButton.Length; i++)
            {
                int remainingBonusPoints = (tempPlayerData.level - 1) * 5 - tempPlayerData.usedAttributeBonus;
                bool canUse = (remainingBonusPoints > 0);
                attributeUpButton[i].gameObject.SetActive(canUse);
            }
        }

        if(attributeDownButton != null)
        {
            for(int i = 0; i < attributeDownButton.Length; i++)
            {
                bool canUse = (tempPlayerData.usedAttributeBonus > nowPlayerData.usedAttributeBonus);
                attributeDownButton[i].gameObject.SetActive(canUse);
            }
        }
        
        UpdateAttributeDisplay();
        CalculateBaseStat(tempPlayerData);
        CalculateStrDamageBonus(tempPlayerData.strength);
        CalculateDexDamageBonus(tempPlayerData.dexterity);
        CalculateIntDamageBonus(tempPlayerData.inteligence);
    }

    // アビリティリストの表示を更新するメソッド
    public void RefreshAbilityList()
    {
        if (tempPlayerData == null)
        {
            Debug.LogError("tempPlayerDataが設定されていません");
            return;
        }

        ClearAllCategories();
        GenerateAbilityButtons(abilityDataSet.searchAndActionAbilities, searchAndActionParent);
        GenerateAbilityButtons(abilityDataSet.combatAndNegotiationAbilities, battleAndNegotiationParent);
        GenerateAbilityButtons(abilityDataSet.knowledgeAndLanguageAbilities, knowledgeAndLanguageParent);
    }

    // アビリティボタンの生成
    void GenerateAbilityButtons(AbilityDataSO[] abilities, Transform parent)
    {
        if (abilities == null || parent == null || tempPlayerData == null)
            return;

        // 残りポイントの計算
        int remainingBonus = (tempPlayerData.level - 1) * 5 - tempPlayerData.usedAbilityBonus;
        // bool canLevelUp = remainingBonus > 0;

        foreach (var abilityDataSO in abilities)
        {
            bool canLevelUpAbility = remainingTempBonusAbilityPoint > 0;
            if (abilityDataSO == null) continue;

            // tempPlayerDataから現在の状態を取得
            SkillState stateTemp;
            tempPlayerData.skillStates.TryGetValue(abilityDataSO.abilityID, out stateTemp);

            bool isLearnedTemp = abilityDataSO.defaultLearnBool == 1;
            int growValueTemp = 0;

            if (stateTemp != null)
            {
                isLearnedTemp = stateTemp.isLearned;
                growValueTemp = stateTemp.growValue;
            }

            // nowPlayerDataから元の状態を取得
            SkillState stateNow;
            nowPlayerData.skillStates.TryGetValue(abilityDataSO.abilityID, out stateNow);
            bool isLearnedNow = stateNow != null && stateNow.isLearned;
            int growValueNow = stateNow != null ? stateNow.growValue : 0;

            // 差分があるかどうか判定して色を決定
            bool hasDiff = isLearnedTemp != isLearnedNow || growValueTemp != growValueNow;
            Color valueColor = hasDiff ? Color.green : Color.white;

            // ボタンの生成と初期化
            GameObject abilityObj = Instantiate(abilityButtonPrefab, parent);
            FormationAbilityButton abilityButton = abilityObj.GetComponent<FormationAbilityButton>();
            if (abilityButton != null)
            {
                abilityButton.Initialize(
                    abilityDataSO,
                    growValueTemp,
                    isLearnedTemp,
                    GetCategoryFromParent(parent),
                    System.Array.IndexOf(abilities, abilityDataSO),
                    tempPlayerData,
                    nowPlayerData,
                    valueColor,
                    canLevelUpAbility
                );
            }
        }
    }

    private AbilityCategory GetCategoryFromParent(Transform parent)
    {
        if (parent == searchAndActionParent) return AbilityCategory.SearchAndAction;
        if (parent == battleAndNegotiationParent) return AbilityCategory.CombatAndNegotiation;
        return AbilityCategory.KnowledgeAndLanguage;
    }

    // 技能習得ボタンのクリックハンドラ
    public void OnClickGetAbility(AbilityDataSO abilityData)
    {
        if (!ValidateAbilityOperation(abilityData))
            return;

        int soulCost = abilityData.learnCost;
        if (tempPlayerData.experience - tempPlayerData.usedSoul < soulCost)
        {
            Debug.Log("ソウルが不足しているため習得できません");
            return;
        }

        // tempPlayerDataの更新
        SkillState state;
        if (!tempPlayerData.skillStates.TryGetValue(abilityData.abilityID, out state))
        {
            state = new SkillState();
            tempPlayerData.skillStates[abilityData.abilityID] = state;
        }

        if (state.isLearned)
        {
            Debug.Log("既に習得済みです");
            return;
        }

        state.isLearned = true;
        tempPlayerData.usedSoul += soulCost;

        UpdateUIAndCheckChanges();
    }

    // レベルアップボタンのクリックハンドラ
    public void OnClickAbilityLevelUp(AbilityDataSO abilityData)
    {
        if (!ValidateAbilityOperation(abilityData))
            return;

        int remainingBonus = (tempPlayerData.level - 1) * 5 - tempPlayerData.usedAbilityBonus;
        if (remainingBonus <= 0)
        {
            Debug.Log("能力成長ボーナスポイントが不足しています。");
            return;
        }

        // tempPlayerDataの更新
        SkillState state;
        if (!tempPlayerData.skillStates.TryGetValue(abilityData.abilityID, out state) || !state.isLearned)
        {
            Debug.LogError("未習得の技能を成長させようとしました");
            return;
        }

        state.growValue++;
        tempPlayerData.usedAbilityBonus++;

        // 確認パネルを表示
        UpdateUIAndCheckChanges();
        ShowActionConfirmPanel(PendingActionType.AbilityLevelUp);
        RefreshAbilityList();
    }

    // レベルダウンボタンのクリックハンドラ
    public void OnClickAbilityLevelDown(AbilityDataSO abilityData)
    {
        if (!ValidateAbilityOperation(abilityData))
            return;

        SkillState stateTemp;
        if (!tempPlayerData.skillStates.TryGetValue(abilityData.abilityID, out stateTemp))
        {
            Debug.LogError("tempPlayerDataに技能が見つかりません");
            return;
        }

        SkillState stateNow;
        int nowGrow = 0;
        if (nowPlayerData.skillStates.TryGetValue(abilityData.abilityID, out stateNow))
        {
            nowGrow = stateNow.growValue;
        }

        if (stateTemp.growValue <= nowGrow)
        {
            Debug.Log("これ以上レベルを下げられません");
            return;
        }

        // tempPlayerDataの更新
        stateTemp.growValue--;
        tempPlayerData.usedAbilityBonus--;

        UpdateUIAndCheckChanges();
    }

    // 操作の有効性を検証
    private bool ValidateAbilityOperation(AbilityDataSO abilityData)
    {
        if (tempPlayerData == null || nowPlayerData == null)
        {
            Debug.LogError("PlayerDataが設定されていません");
            return false;
        }

        if (abilityData == null)
        {
            Debug.LogError("abilityDataがnullです");
            return false;
        }

        return true;
    }

    // UI更新と変更チェック
    private void UpdateUIAndCheckChanges()
    {
        RecalcTempUI();
        RefreshAbilityList();
        CheckAndShowConfirmPanel();
    }

    // 確認パネルの表示判定
    private void CheckAndShowConfirmPanel()
    {
        if (HasAbilityChanges())
        {
            ShowActionConfirmPanel(PendingActionType.AcquireAbility);
        }
        else
        {
            HideActionConfirmPanel();
        }
    }

    // アビリティの変更を検出
    public bool HasAbilityChanges()
    {
        if (nowPlayerData == null || tempPlayerData == null)
        {
            Debug.LogError("PlayerDataが設定されていません");
            return false;
        }

        Debug.Log($"=== 差分チェック開始 ===");
        Debug.Log($"nowPlayerData.skillStates.Count: {nowPlayerData.skillStates.Count}");
        Debug.Log($"tempPlayerData.skillStates.Count: {tempPlayerData.skillStates.Count}");

        // tempPlayerDataの変更をチェック
        foreach (var tempKvp in tempPlayerData.skillStates)
        {
            SkillState nowState;
            bool foundNow = nowPlayerData.skillStates.TryGetValue(tempKvp.Key, out nowState);

            Debug.Log($"\nアビリティID: {tempKvp.Key}");
            Debug.Log($"temp - isLearned: {tempKvp.Value.isLearned}, growValue: {tempKvp.Value.growValue}");

            if (!foundNow)
            {
                Debug.Log($"新規アビリティ: {tempKvp.Key}");
                if (tempKvp.Value.isLearned || tempKvp.Value.growValue > 0)
                {
                    Debug.Log($"新規習得差分あり: {tempKvp.Key}");
                    return true;
                }
            }
            else
            {
                Debug.Log($"now - isLearned: {nowState.isLearned}, growValue: {nowState.growValue}");
                if (tempKvp.Value.isLearned != nowState.isLearned)
                {
                    Debug.Log($"習得状態差分あり: {tempKvp.Key}, now: {nowState.isLearned}, temp: {tempKvp.Value.isLearned}");
                    return true;
                }
                if (tempKvp.Value.growValue != nowState.growValue)
                {
                    Debug.Log($"成長値差分あり: {tempKvp.Key}, now: {nowState.growValue}, temp: {tempKvp.Value.growValue}");
                    return true;
                }
            }
        }

        // nowPlayerDataから削除されたアビリティをチェック
        foreach (var nowKvp in nowPlayerData.skillStates)
        {
            if (!tempPlayerData.skillStates.ContainsKey(nowKvp.Key))
            {
                Debug.Log($"\n削除されたアビリティ: {nowKvp.Key}");
                Debug.Log($"now - isLearned: {nowKvp.Value.isLearned}, growValue: {nowKvp.Value.growValue}");
                if (nowKvp.Value.isLearned || nowKvp.Value.growValue > 0)
                {
                    Debug.Log($"削除されたアビリティあり: {nowKvp.Key}");
                    return true;
                }
            }
        }

        Debug.Log("\n=== 差分なし ===");
        return false;
    }

    // カテゴリーのクリア
    private void ClearAllCategories()
    {
        ClearCategory(searchAndActionParent);
        ClearCategory(battleAndNegotiationParent);
        ClearCategory(knowledgeAndLanguageParent);
    }

    private void ClearCategory(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }

    // 変更を確定するメソッド
    public void ConfirmAbilityChanges()
    {
        Debug.Log("ConfirmAbilityChanges");
        if (nowPlayerData == null || tempPlayerData == null)
        {
            Debug.LogError("PlayerDataが設定されていません");
            return;
        }

        // nowPlayerDataを更新
        nowPlayerData.level = tempPlayerData.level;
        nowPlayerData.experience = tempPlayerData.experience;
        nowPlayerData.usedSoul = tempPlayerData.usedSoul;
        nowPlayerData.usedAbilityBonus = tempPlayerData.usedAbilityBonus;

        // スキル状態を更新
        nowPlayerData.skillStates.Clear();
        foreach (var kvp in tempPlayerData.skillStates)
        {
            SkillState newState = new SkillState
            {
                isLearned = kvp.Value.isLearned,
                growValue = kvp.Value.growValue
            };
            nowPlayerData.skillStates[kvp.Key] = newState;
        }

        // PlayerManagerのplayerDatasリスト内のデータも更新
        if (playerManager != null && playerManager.playerDatas != null)
        {
            int playerIndex = playerManager.playerSelectedindex;
            if (playerIndex >= 0 && playerIndex < playerManager.playerDatas.Count)
            {
                playerManager.playerDatas[playerIndex] = new PlayerData(nowPlayerData);
            }
        }

        tempPlayerData = new PlayerData(nowPlayerData);
        InitializeTempData();
    
        RecalcTempUI();
        RefreshAbilityList();
        playerManager.SavePlayerData();
    }

    // 変更をキャンセルするメソッド
    public void CancelAbilityChanges()
    {
        if (nowPlayerData == null || tempPlayerData == null)
        {
            Debug.LogError("PlayerDataが設定されていません");
            return;
        }

        // tempPlayerDataをnowPlayerDataの状態に戻す
        tempPlayerData.level = nowPlayerData.level;
        tempPlayerData.experience = nowPlayerData.experience;
        tempPlayerData.usedSoul = nowPlayerData.usedSoul;
        tempPlayerData.usedAbilityBonus = nowPlayerData.usedAbilityBonus;

        // スキル状態を元に戻す
        tempPlayerData.skillStates.Clear();
        foreach (var kvp in nowPlayerData.skillStates)
        {
            SkillState newState = new SkillState
            {
                isLearned = kvp.Value.isLearned,
                growValue = kvp.Value.growValue
            };
            tempPlayerData.skillStates[kvp.Key] = newState;
        }

        // UIを更新
        RefreshAbilityList();
        RecalcTempUI();
    }
}

