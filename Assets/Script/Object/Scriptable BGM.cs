using UnityEngine;

[CreateAssetMenu(fileName = "New BGM Data", menuName = "BGM Data", order = 51)]
public class BGMData : ScriptableObject
{
    public string bgmName;
    public AudioClip bgmClip;
    public string bgmSource;
    public string bgmUseScene;
    [TextArea(1,6)]
    public string bgmComment;
}