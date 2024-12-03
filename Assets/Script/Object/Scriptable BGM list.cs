using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Chapter BGM Data", menuName = "Chapter BGM Data", order = 52)]
public class ChapterBGMData : ScriptableObject
{
    public string chapterName;
    public List<BGMData> bgmList;
}