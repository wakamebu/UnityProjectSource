using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject navigationCanvasPrefab;
    private static UIManager instance;

    void Awake()
    {
        // シングルトンパターンの実装
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // NavigationCanvasプレハブをインスタンス化
            if (navigationCanvasPrefab != null)
            {
                Instantiate(navigationCanvasPrefab);
            }
            else
            {
                Debug.LogError("NavigationCanvasPrefabが割り当てられていません。");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
