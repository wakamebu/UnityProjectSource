using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BGMInfo
{
    public string bgmName;        // BGM の名前
    public string source;         // BGM のソース情報（作曲者、出典など）
    public string scene;          // BGM が使用されているシーン
    [TextArea(1,6)]
    public string comment;        // コメントや説明
    public AudioClip bgmClip;     // 実際の AudioClip
}