using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PalyerDataGetAndView : MonoBehaviour
{
    // UI 要素
    [Header("UI Elements")]
    public TMP_Text yourName;
    public TMP_Text yourCharacterLevel;
    public Image yourCharacterSprite;

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

    public void GetSprite(){
        //yourCharacterSprite.sprite = selectedPlayerData.characterSprite;
    }

    public void GetLevel(){
        string Leveltext = "Level : " + selectedPlayerData.level.ToString();
        yourCharacterLevel.text = Leveltext;
    }

    public void RefreshView()
    {
        if (PlayerManager.instance == null) return;

        selectedPlayerData = PlayerManager.instance.GetSelectedPlayerData();
        UpdateView();
    }

    private void UpdateView(){
        GetName();
        GetLevel();
        //GetSprite();
    }
}
