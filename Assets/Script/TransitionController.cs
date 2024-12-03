using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TransitionController : MonoBehaviour
{
    [SerializeField]
    private Material _transitionMaterial; // トランジション用のマテリアル
    [SerializeField]
    private float transitionInTime = 0.5f;  // トランジションインの時間
    [SerializeField]
    private float transitionOutTime = 0.5f; // トランジションアウトの時間

    public static TransitionController instance;

    private void Awake()
    {
        // シングルトンの実装
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // シーン遷移時にオブジェクトを破棄しない
        }
        else
        {
            Destroy(gameObject); // 既にインスタンスが存在する場合は破棄
        }
    }

     /// <summary>
    /// ボタンのOnClickイベントから呼び出すメソッド
    /// </summary>
    /// <param name="sceneName">遷移先のシーン名</param>
    public void OnTransitionButtonClicked(string sceneName)
    {
        StartCoroutine(TransitionAndLoadScene(sceneName));
    }

    /// <summary>
    /// トランジションイン → シーンロード → トランジションアウト の順で実行するコルーチン
    /// </summary>
    /// <param name="sceneName">遷移先のシーン名</param>
    /// <returns></returns>
    public IEnumerator TransitionAndLoadScene(string sceneName)
    {
        // トランジションイン
        Debug.Log("トランジションします");
        yield return Animate(_transitionMaterial, transitionInTime, isFadeIn: true);
        Debug.Log("トランジションしました");


        // シーンを非同期でロード
        Debug.Log("シーンロードします");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // ロードが完了するまで待機
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    /// <summary>
    /// トランジションインまたはアウトを指定した時間で行う
    /// </summary>
    /// <param name="material">使用するマテリアル</param>
    /// <param name="time">トランジションにかける時間</param>
    /// <param name="isFadeIn">インなら true、アウトなら false</param>
    /// <returns></returns>
    public IEnumerator Animate(Material material, float time, bool isFadeIn)
    {
        Image image = GetComponent<Image>();
        if (image == null)
        {
            Debug.LogError("Imageコンポーネントが見つかりません。");
            yield break;
        }

        image.material = material;
        float currentTime = 0f;

        while (currentTime < time)
        {
            float alpha = isFadeIn ? Mathf.Lerp(0f, 1f, currentTime / time) : Mathf.Lerp(1f, 0f, currentTime / time);
            material.SetFloat("_Alpha", alpha);
            yield return null;
            currentTime += Time.deltaTime;
        }

        material.SetFloat("_Alpha", isFadeIn ? 1f : 0f);
    }
}
