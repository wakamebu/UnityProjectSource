using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int dataVersion = 1; //本体のバージョンではなく、セーブデータの中身を増やすごとに1増やすデータバージョン変数
    public List<PlayerData> playerDatas;
    public int selectedPlayerIndex; //どのキャラクターを選択したプレイヤーかを管理するインデックス
    public SettingData settingData;

    // 他の必要なデータ（進行状況など）を追加可能
}