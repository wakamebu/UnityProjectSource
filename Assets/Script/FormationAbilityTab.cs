// このクラスは非推奨です。FormationCharacterLevelUPに統合されました。
#if false
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FormationAbilityTab : MonoBehaviour
{
    [Header("UI Elements")] 
    public GameObject abilityButtonPrefab;
    // 各カテゴリーのScroll View Content
    public Transform searchAndActionParent;
    public Transform battleAndNegotiationParent;
    public Transform knowledgeAndLanguageParent;

    [Header("Ability Master Set")]
    // AbilityDataSet には、探索・行動、戦闘・交渉、知識・言語の各Ability配列が格納されている
    // プレイヤーの所持状況にかかわらず、全てのアビリティを表示するために使用
    public AbilityDataSet abilityDataSet;

    // 内部データ
    private PlayerData nowPlayerData;
    private PlayerData tempPlayerData;
    private PlayerManager playerManager;
    private FormationSelectScript formationSelectScript;
    private FormationCharacterLevelUP formationCharacterLevelUP;

    public static FormationAbilityTab instance;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        playerManager = PlayerManager.instance;
        formationCharacterLevelUP = FormationCharacterLevelUP.instance;
    }

    // キャラクター選択時に呼ばれるメソッド
    public void SetPlayerData(PlayerData nowData, PlayerData tempData)
    {
        nowPlayerData = nowData;
        tempPlayerData = new PlayerData(); // 新しいインスタンスを作成

        // 必要なデータをコピー
        tempPlayerData.level = tempData.level;
        tempPlayerData.experience = tempData.experience;
        tempPlayerData.usedSoul = tempData.usedSoul;
        tempPlayerData.usedAbilityBonus = tempData.usedAbilityBonus;

        // スキル状態をディープコピー
        foreach (var kvp in tempData.skillStates)
        {
            SkillState newState = new SkillState
            {
                isLearned = kvp.Value.isLearned,
                growValue = kvp.Value.growValue
            };
            tempPlayerData.skillStates[kvp.Key] = newState;
        }
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

        foreach (var abilityDataSO in abilities)
        {
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
                    valueColor
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

        UpdateUIAndCheckChanges();
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
        formationCharacterLevelUP.RecalcTempUI();
        RefreshAbilityList();
        CheckAndShowConfirmPanel();
    }

    // 確認パネルの表示判定
    private void CheckAndShowConfirmPanel()
    {
        if (HasAbilityChanges())
        {
            FormationCharacterLevelUP.instance.ShowActionConfirmPanel(FormationCharacterLevelUP.PendingActionType.AcquireAbility);
        }
        else
        {
            FormationCharacterLevelUP.instance.HideActionConfirmPanel();
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
    public void ConfirmChanges()
    {
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

        // UIを更新
        RefreshAbilityList();
        CheckAndShowConfirmPanel();
    }

    // 変更をキャンセルするメソッド
    public void CancelChanges()
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
        CheckAndShowConfirmPanel();
    }
}
#endif
