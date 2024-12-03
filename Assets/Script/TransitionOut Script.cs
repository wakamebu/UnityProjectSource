using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class TransitionInOut : MonoBehaviour
{
    [SerializeField]
    private Material _transitionMaterial; // トランジション用のマテリアル
    [SerializeField]
    private float transitionInTime = 0.5f;  // トランジションインの時間
    [SerializeField]
    private float transitionOutTime = 0.5f; // トランジションアウトの時間

    void Start()
    {
        StartCoroutine(BeginTransition());
    }

    IEnumerator BeginTransition()
    {
        yield return Animate(_transitionMaterial, transitionInTime, transitionOutTime);
    }

    /// <summary>
    /// トランジションインとアウトを指定した時間で行う
    /// </summary>
    /// <param name="material">使用するマテリアル</param>
    /// <param name="inTime">トランジションインの時間</param>
    /// <param name="outTime">トランジションアウトの時間</param>
    /// <returns></returns>
    IEnumerator Animate(Material material, float inTime, float outTime)
        {
            Image image = GetComponent<Image>();
            if (image == null)
            {
                Debug.LogError("Imageコンポーネントが見つかりません。");
                yield break;
            }

            image.material = material;
            float currentTime = 0f;

            // トランジションイン
            while (currentTime < inTime)
            {
                float alpha = Mathf.Lerp(0f, 1f, currentTime / inTime);
                material.SetFloat("_Alpha", alpha);
                yield return null;
                currentTime += Time.deltaTime;
            }
            material.SetFloat("_Alpha", 1f);

            // トランジションアウト
            currentTime = 0f;
            while (currentTime < outTime)
            {
                float alpha = Mathf.Lerp(1f, 0f, currentTime / outTime);
                material.SetFloat("_Alpha", alpha);
                yield return null;
                currentTime += Time.deltaTime;
            }
            material.SetFloat("_Alpha", 0f);
        }
    }
