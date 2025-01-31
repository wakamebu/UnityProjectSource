using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterImageSetter : MonoBehaviour
{
    [SerializeField] private Image characterImageUI; 
    [SerializeField] public PlayerDataScriptableObject[] playerDataCIS;

    public void SetCharacterImage(int index)
    {
        // 配列範囲チェック
        if (playerDataCIS == null || playerDataCIS.Length == 0)
        {
            UnityEngine.Debug.LogWarning("playerDataCIS が空です");
            return;
        }
        if (index < 0 || index >= playerDataCIS.Length)
        {
            UnityEngine.Debug.LogWarning($"インデックス {index} は配列の範囲外です");
            return;
        }

        // 指定されたScriptableObjectを取得
        PlayerDataScriptableObject so = playerDataCIS[index];
        if (so != null && so.characterSprite != null)
        {
            characterImageUI.sprite = so.characterSprite;
        }
        else
        {
            UnityEngine.Debug.LogWarning($"playerDataCIS[{index}] または characterSprite がnullです");
        }
    }
}