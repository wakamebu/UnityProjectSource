using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NavigationController : MonoBehaviour
{
    private TransitionController transitionController;

    void Start()
    {
        transitionController = TransitionController.instance;
        if (transitionController == null)
        {
            Debug.LogError("TransitionController のインスタンスが見つかりません。シーンに TransitionController が存在し、正しく設定されていることを確認してください。");
        }
        else
        {
            Debug.Log("TransitionController instance found in NavigationController.");
        }
    }

    public void LoadTopScene()
    {
        if(transitionController != null)
        {
        Debug.Log("シーンロードします");
        StartCoroutine(transitionController.TransitionAndLoadScene("TopPageScene"));
        }
    }

    public void LoadGallery()
    {
        StartCoroutine(transitionController.TransitionAndLoadScene("GalleryScene"));
    }

    public void LoadFEO()
    {
        // FEOシーンをロード
        StartCoroutine(transitionController.TransitionAndLoadScene("SkillAcquisitionScene"));
    }

    public void LoadFormation()
    {
        StartCoroutine(transitionController.TransitionAndLoadScene("FormationScene"));
    }

    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}