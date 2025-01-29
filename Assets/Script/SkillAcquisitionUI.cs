using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine.EventSystems;

public class SkillAcquisitionUI : MonoBehaviour
{
    // UI 要素
    [Header("UI Elements")]
    public TMP_Dropdown baseSkillDropdown;
    public Button[] attributeButtons;
    public Toggle[] optionToggles;
    public TMP_Text remainingPointsText;
    public TMP_Text nameText;
    public TMP_Text baseEffectText;
    public TMP_Text optionText;
    public TMP_Text maxSyncLevelIs;
    public Button copyButton;
    public Button saveButton;
    // public Button returnButton;
    public Button increaseLevelButton;
    public Button decreaseLevelButton;
    public TMP_Text syncLevelText;

    // 内部データ
    private int syncLevel;
    private int maxSyncLevel;
    private int totalPoints;
    private int remainingPoints;
    private int totalFailDamage; //SANc失敗時のダメージ
    private int totalSuccessDamage; //SANc成功時ダメージ
    private int totalMPCost;
    private int totalSANCos;
    private string baseSANCost;
    private float totalSuccessDamageMultiplier;
    private float totalFailDamageMultiplier;

    private AttributeData selectedAttributeData;
    private BaseSkillData selectedBaseSkillData;
    private List<OptionData> selectedOptionDataList = new List<OptionData>();

    // その他の変数
    private Dictionary<string, Color> attributeColors = new Dictionary<string, Color>()
    {
        { "炎", new Color(1, 0, 0, 1) },
        { "水", Color.blue },
        { "風", Color.green },
        { "土", new Color(0.6f, 0.4f, 0.2f) },
        { "氷", Color.cyan },
        { "光", Color.yellow },
        { "闇", Color.magenta },
    };

    public Image[] imagesWithOutline;

    // ベース必殺技データ
    private List<BaseSkillData> baseSkillList = new List<BaseSkillData>();

    void Start()
    {
        // 初期化
        syncLevel = 1;
        maxSyncLevel = 1;
        totalPoints = syncLevel - 1;
        remainingPoints = totalPoints;
        totalFailDamage = 0;
        totalSuccessDamage = 0;
        totalMPCost = 0;
        totalSANCos = 0;

        // リスナーの設定
        baseSkillDropdown.onValueChanged.AddListener(OnBaseSkillSelected);

        if (increaseLevelButton != null)
        {
            increaseLevelButton.onClick.AddListener(IncreaseSyncLevel);
        }
        if (decreaseLevelButton != null)
        {
            decreaseLevelButton.onClick.AddListener(DecreaseSyncLevel);
        }

        if (copyButton != null)
        {
            copyButton.onClick.AddListener(CopySkillInfoToClipboard);
        }

        if (saveButton != null)
        {
            saveButton.onClick.AddListener(SaveSkillToPlayerData);
        } 

        // 属性ボタンのリスナーを設定
        for (int i = 0; i < attributeButtons.Length; i++)
        {
            if (attributeButtons[i] != null)
            {
                int index = i; 
                attributeButtons[i].onClick.AddListener(() => OnAttributeSelected(attributeButtons[index]));
            }
        }

        // オプショントグルのリスナーを設定
        foreach (Toggle toggle in optionToggles)
        {
            toggle.onValueChanged.AddListener((isOn) => OnOptionToggleChanged(toggle, isOn));
        }

        // ベース必殺技をロード
        LoadBaseSkills();

        // 初期表示の更新
        UpdateSyncLevelUI();
        UpdateOptionToggles();
        UpdateStatsBasedOnSyncLevel();
        UpdatePreview();
        UpdateRemainingPointsText();
    }

    void OnApplicationQuit()
    {
        SaveSkillToPlayerData();
    }

    void OnDestroy()
    {
        SaveSkillToPlayerData();
    }

    void OnDisable()
    {
        SaveSkillToPlayerData();
    }

    // ベース必殺技のロード
    void LoadBaseSkills()
    {
        BaseSkillData[] baseSkills = Resources.LoadAll<BaseSkillData>("BaseSkills");
        baseSkillList.AddRange(baseSkills);

        baseSkillDropdown.ClearOptions();
        List<string> options = baseSkillList.Select(skill => skill.skillName).ToList();
        baseSkillDropdown.AddOptions(options);

        if (baseSkillList.Count > 0)
        {
            baseSkillDropdown.value = 0;
            baseSkillDropdown.RefreshShownValue();
            OnBaseSkillSelected(baseSkillDropdown.value);
        }
    }

    // ベース必殺技が選択されたとき
    void OnBaseSkillSelected(int index)
    {
        if (index >= 0 && index < baseSkillList.Count)
        {
            selectedBaseSkillData = baseSkillList[index];
            Debug.Log("選択されたベースFEO: " + selectedBaseSkillData.skillName);

            maxSyncLevel = selectedBaseSkillData.maxSyncLevel;
            if (syncLevel > maxSyncLevel)
            {
                syncLevel = maxSyncLevel;
            }

            UpdateSyncLevelUI();
            UpdateStatsBasedOnSyncLevel();
            UpdatePreview();
        }
    }

    // 同期レベルの増減
    void IncreaseSyncLevel()
    {
        if (syncLevel < maxSyncLevel)
        {
            syncLevel++;
            OnSyncLevelChanged();
        }
    }

    void DecreaseSyncLevel()
    {
        if (syncLevel > 1)
        {
            syncLevel--;
            OnSyncLevelChanged();
        }
    }

    void OnSyncLevelChanged()
    {
        UpdateSyncLevelUI();
        UpdateStatsBasedOnSyncLevel();
        UpdatePreview();
        ResetOptionSelection();
    }

    void UpdateSyncLevelUI()
    {
        if (syncLevelText != null)
        {
            syncLevelText.text = "同期Lv: " + syncLevel.ToString();
        }

        if (increaseLevelButton != null)
        {
            increaseLevelButton.interactable = syncLevel < maxSyncLevel;
        }
        if (decreaseLevelButton != null)
        {
            decreaseLevelButton.interactable = syncLevel > 1;
        }
    }

    // 属性が選択されたとき
    void OnAttributeSelected(Button btn)
    {
        Debug.Log("属性ボタンがクリックされました: " + btn.name);
        AttributeButton attributeButton = btn.GetComponent<AttributeButton>();
        if (attributeButton == null || attributeButton.attributeData == null)
            return;

        selectedAttributeData = attributeButton.attributeData;

        // 色の更新
        Color attributeColor = attributeColors.ContainsKey(selectedAttributeData.attributeName) ? attributeColors[selectedAttributeData.attributeName] : Color.white;
        SetImagesOutlineColor(attributeColor);

        ResetOptionSelection();
        UpdateOptionToggles();
        UpdatePreview();
    }

    void ResetOptionSelection()
    {

        selectedOptionDataList.Clear();

        foreach (Toggle toggle in optionToggles)
        {
            if (toggle != null)
            {
                toggle.SetIsOnWithoutNotify(false);
            }
        }

        totalPoints = syncLevel - 1;
        remainingPoints = totalPoints;

        UpdateRemainingPointsText();
    }

    // オプショントグルの更新
    void UpdateOptionToggles()
    {
        if (selectedAttributeData == null)
            return;

        string selectedConstellation = selectedAttributeData.attributeName;

        OptionData[] allOptions = Resources.LoadAll<OptionData>("Options");
        List<OptionData> filteredOptions = allOptions.Where(option => option.constellation == selectedConstellation).ToList();

        for (int i = 0; i < optionToggles.Length; i++)
        {
            Toggle toggle = optionToggles[i];
            OptionToggle optionToggle = toggle.GetComponent<OptionToggle>();

            if (i < filteredOptions.Count)
            {
                toggle.gameObject.SetActive(true);
                OptionData optionData = filteredOptions[i];
                optionToggle.optionData = optionData;

                TMP_Text label = toggle.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    label.text = optionData.optionName;
                }

                // リスナーを再設定
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener((isOn) => OnOptionToggleChanged(toggle, isOn));

                // トグルの状態をリセット
                toggle.SetIsOnWithoutNotify(false);
            }
            else
            {
                toggle.gameObject.SetActive(false);
                optionToggle.optionData = null;
                toggle.onValueChanged.RemoveAllListeners();
            }
        }
    }

    // オプションが選択・解除されたとき
    void OnOptionToggleChanged(Toggle toggle, bool isOn)
    {
        OptionToggle optionToggle = toggle.GetComponent<OptionToggle>();
        if (optionToggle == null || optionToggle.optionData == null)
            return;

        OptionData optionData = optionToggle.optionData;
        int optionCost = optionData.cost;

        if (isOn)
        {
            if (remainingPoints >= optionCost)
            {
                remainingPoints -= optionCost;
                selectedOptionDataList.Add(optionData);

                UpdateStatsBasedOnSyncLevel();
            }
            else
            {
                // ポイントが不足している場合はトグルをオフに戻す
                toggle.SetIsOnWithoutNotify(false);
                Debug.Log("ポイントが不足しています");
            }
        }
        else
        {
            if (selectedOptionDataList.Contains(optionData))
            {
                remainingPoints += optionCost;
                selectedOptionDataList.Remove(optionData);

                UpdateStatsBasedOnSyncLevel();
            }
        }

        UpdateRemainingPointsText();
        UpdatePreview();
    }

    // ステータスの再計算
    void UpdateStatsBasedOnSyncLevel()
    {
        if (selectedBaseSkillData == null)
            return;

        totalPoints = syncLevel - 1;
        int usedPoints = selectedOptionDataList.Sum(option => option.cost);
        remainingPoints = totalPoints - usedPoints;

        if(remainingPoints < 0)
        {
            remainingPoints = 0;
            Debug.LogWarning("残りポイントが不足しています");
        }

        int baseDamage = selectedBaseSkillData.baseSkillDamage;
        int baseBonus = selectedBaseSkillData.baseBonus;
        int baseMPCost = selectedBaseSkillData.baseMPCost;

        float syncDamageBonus = 0.8f;
        float sanCostPenaltyRate = 0.25f;
        float mpCostIncreaseRate = 0.2f;

        int syncLevelDamage = Mathf.FloorToInt(syncDamageBonus * Mathf.Pow(syncLevel, 2));

        RecalculateOptionEffects();

        float successDamageMultiplier = 1 + totalSuccessDamageMultiplier;
        float failDamageMultiplier = 1 + totalFailDamageMultiplier;

        totalFailDamage = Mathf.FloorToInt((baseDamage + syncLevelDamage) * failDamageMultiplier);
        totalSuccessDamage = Mathf.FloorToInt((baseDamage + syncLevelDamage + baseBonus) * successDamageMultiplier);

        //san
        int totalOptionSanCost = selectedOptionDataList.Sum(option => option.sanCostIncrease);
        float totalSanCostMultiplier = 1 + (sanCostPenaltyRate * (syncLevel - 1));
        totalSANCos = Mathf.FloorToInt(totalOptionSanCost * totalSanCostMultiplier);

        int totalOptionMPCostIncrease = selectedOptionDataList.Sum(option => option.mpCostIncrease);
        int totalBaseAndOptionMPCost = baseMPCost + totalOptionMPCostIncrease;
        float totalMPCostMultiplier = 1 + (mpCostIncreaseRate * (syncLevel - 1));
        totalMPCost = Mathf.FloorToInt(totalBaseAndOptionMPCost * totalMPCostMultiplier);

        UpdateRemainingPointsText();
    }

    // オプションによるダメージ量の再計算
    void RecalculateOptionEffects()
    {
        totalSuccessDamageMultiplier = selectedOptionDataList.Sum(option => option.successDamageMultiplier);
        totalFailDamageMultiplier = selectedOptionDataList.Sum(option => option.failDamageMultiplier);
    }

    // プレビューの更新
    void UpdatePreview()
    {
        // 名前の表示
        if (selectedBaseSkillData != null)
        {
            nameText.text = selectedBaseSkillData.skillName;
        }
        else
        {
            nameText.text = "ベースが選択されていません";
        }

        // ベース効果＋ダメージ＋各コストの表示
        string baseEffect = "";
        string MaxSyncLevelIs = "";
        string baseSanCost = selectedBaseSkillData.baseSANCos;

        if (selectedBaseSkillData != null)
        {
            baseEffect += selectedBaseSkillData.description + "\n";
            baseEffect += "SANc失敗時ダメージ: " + totalFailDamage + "\n";
            baseEffect += "SANc成功時ダメージ: " + totalSuccessDamage + "\n";
            baseEffect += "MPコスト: " + totalMPCost + "\n";
            baseEffect += "SAN値コスト: " + baseSanCost + "+" + totalSANCos + "\n";
        }
        else
        {
            baseEffect += "ベースが選択されていません\n";
        }

        if (selectedAttributeData != null)
        {
            baseEffect += "属性: " + selectedAttributeData.attributeName;
        }
        else
        {
            baseEffect += "属性が選択されていません" + "\n";
        }

        if (maxSyncLevel != null)
        {
            MaxSyncLevelIs += "最大同期レベル: " + maxSyncLevel;
        }
        else
        {
            MaxSyncLevelIs += "最大動機レベル: " + "null";
        }

        if (syncLevel != null)
        {
            baseEffect += "同期レベル: " + syncLevel;
        }

        baseEffectText.text = baseEffect;
        maxSyncLevelIs.text = MaxSyncLevelIs;

        // オプションの表示
        string optionInfo = "";

        if (selectedOptionDataList.Count > 0)
        {
            optionInfo += "選択した星の光:\n";
            foreach (OptionData optionData in selectedOptionDataList)
            {
                optionInfo += "- " + optionData.optionName + "\n" + optionData.effect + "\n";
                //optionInfo += "  ダメージ倍率: +" + (optionData.damageMultiplier * 100f) + "%\n";
            }
        }
        else
        {
            optionInfo += "星の欠片が選択されていません";
        }

        optionText.text = optionInfo;
    }

    // 残りポイントの更新
    void UpdateRemainingPointsText()
    {
        remainingPointsText.text = "残りポイント：" + remainingPoints.ToString();
    }

    // スキル情報のコピー
    void CopySkillInfoToClipboard()
    {
        string textToCopy = GenerateSkillInfoText();
        GUIUtility.systemCopyBuffer = textToCopy;
        Debug.Log("テキストがクリップボードにコピーされました:\n" + textToCopy);
    }

    // スキル情報の生成
    string GenerateSkillInfoText()
    {
        string skillInfo = "";

        // 必殺技の名前
        if (selectedBaseSkillData != null)
        {
            skillInfo += "【" + selectedBaseSkillData.skillName + "】\n";
        }
        else
        {
            skillInfo += "【ベーススキルが選択されていません】\n";
        }

        // ベース効果＋ダメージ＋各コスト
        if (selectedBaseSkillData != null)
        {
            skillInfo += selectedBaseSkillData.description + "\n";
            skillInfo += "SANc失敗時ダメージ: " + totalFailDamage + "\n";
            skillInfo += "SANc成功時ダメージ: " + totalSuccessDamage + "\n";
            skillInfo += "MPコスト: " + totalMPCost + "\n";
            skillInfo += "SAN値コスト: " + totalSANCos + "\n";
        }
        else
        {
            skillInfo += "ベースが選択されていません\n";
        }

        if (selectedAttributeData != null)
        {
            skillInfo += "属性: " + selectedAttributeData.attributeName+ "\n";
        }
        else
        {
            skillInfo += "属性が選択されていません" + "\n";
        }

        if (syncLevel != null)
        {
            skillInfo += "同期レベル: " + syncLevel;
        }

        // オプションの情報
        if (selectedOptionDataList.Count > 0)
        {
            skillInfo += "選択した星の光:\n";
            foreach (OptionData optionData in selectedOptionDataList)
            {
                skillInfo += "- " + optionData.optionName + ": " + optionData.effect + "\n";
                //skillInfo += "  ダメージ倍率: +" + (optionData.damageMultiplier * 100f) + "%\n";
            }
        }
        else
        {
            skillInfo += "星の欠片が選択されていません";
        }

        return skillInfo;
    }

    // 画像のアウトライン色を設定
    void SetImagesOutlineColor(Color color)
    {
        foreach (Image img in imagesWithOutline)
        {
            if (img != null)
            {
                Outline outline = img.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.effectColor = color;
                }
            }
        }
    }

    public void SaveSkillToPlayerData()
    {
        if(selectedBaseSkillData == null)
        {
            Debug.LogWarning("ベーススキルが選択されていません");
            return;
        }

        if(selectedAttributeData == null)
        {
            Debug.LogWarning("属性が選択されていません");
            return;
        }
        
        FeoData feoData = new FeoData();
        feoData.baseSkillName = selectedBaseSkillData.skillName;
        feoData.attributeName = selectedAttributeData.attributeName;
        feoData.selectedOptionNames = selectedOptionDataList.Select(option => option.optionName).ToList();
        feoData.syncLevel = syncLevel;

        PlayerData playerData = PlayerManager.instance.GetSelectedPlayerData();
        playerData.feos.Add(feoData);

        PlayerManager.instance.SavePlayerData();
        Debug.Log("FEOを保存しました");
    }
}
