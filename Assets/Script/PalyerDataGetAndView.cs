using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PalyerDataGetAndView : MonoBehaviour
{
    // UI 要素
    [Header("UI Elements")]
    public TMP_Text yourName;
    public TMP_Text yourCharacterLevel;

    private PlayerManager playerManager;
    private PlayerData selectedPlayerData;

    void Start(){
        StartCoroutine(InitializeAfterDataLoad());
    }

    IEnumerator InitializeAfterDataLoad(){
        while(PlayerManager.instance == null || PlayerManager.instance.GetSelectedPlayerData() == null)
        {
            yield return null; 
        }

        playerManager = PlayerManager.instance;
        selectedPlayerData = playerManager.GetSelectedPlayerData();
        UpdateView();
    }

    public void GetName(){
        yourName.text = selectedPlayerData.playerName;
    }

    public void GetLevel(){
        string Leveltext = "Level : " + selectedPlayerData.level.ToString();
        yourCharacterLevel.text = Leveltext;
    }

    private void UpdateView(){
        GetName();
        GetLevel();
    }
}
