using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void LoadSkillCreationScene()
    {
        SceneManager.LoadScene("SkillAcquisitionScene");
    }

    public void LoadSoundtrackScene()
    {
        SceneManager.LoadScene("SoundtrackScene");
    }
    
}
