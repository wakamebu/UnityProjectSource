using System.Collections;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class TextAnimator : MonoBehaviour
{
    public float waitPerChar = 0.01f;

    public static TextAnimator instance;


    void Awake()
    {
        // シングルトンの設定
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    //*/

    /// <summary>
    /// 指定されたTMP_Textコンポーネントに対してテキストアニメーションを実行します。
    /// </summary>
    /// <param name="text">アニメーションを適用するTMP_Textコンポーネント</param>
    /// <param name="message">表示するメッセージ</param>

    /*
    void Start()
    {
        StartCoroutine(Simple());
    }
    //*/

    public IEnumerator SimpleAnimateText(TMP_Text text,string message)
    {
        if(text == null)
        {
            Debug.LogWarning("TMPコンポーネントがnull");
            yield break;
        }

        text.text = message;
        // 文字の表示数を0に(テキストが表示されなくなる)
        text.maxVisibleCharacters = 0;

        // テキストの文字数分ループ
        for (int i = 0; i <= message.Length; i++)
        {
            // 一文字ごとにn秒待機
            // 文字の表示数を増やしていく
            text.maxVisibleCharacters = i;
            yield return new WaitForSeconds(waitPerChar);
        }
    }
}