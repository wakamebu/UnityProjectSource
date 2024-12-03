using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "CharacterData/PlayerData", order = 1)]
public class PlayerDataScriptableObject : ScriptableObject
{
    public PlayerData playerData;
}