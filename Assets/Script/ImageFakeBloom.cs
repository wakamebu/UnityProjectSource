using UnityEngine;
using UnityEngine.UI;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

[ExecuteInEditMode, RequireComponent(typeof(Image))]
public class ImageFakeBloom : MonoBehaviour
{
    [SerializeField]
    private Image _image;

    [SerializeField]
    private Image _glowImage;

    /// <summary> 発光色 </summary>
    [SerializeField]
    private Color _glowColor = Color.white;

    /// <summary> 上乗せ画像のぼかし距離 </summary>
    [SerializeField]
    private float _blurSig = 5f;

    // ぼかし画像の再生成判定用
    private float _preBlurSig;
    private Sprite _preOrifinSprite;

    /// <summary>
    /// 起動時
    /// </summary>
    private void Awake()
    {
        UpdateGlow();
    }

    /// <summary>
    /// Inspector上の値が変更されたときに呼び出し
    /// </summary>
    private void OnValidate()
    {
        UpdateGlow();
    }

    /// <summary>
    /// 初期化
    /// </summary>
    private void Initialize()
    {
        _image = GetComponent<Image>();

        // 上乗せする発光表現用のImageの生成
        _glowImage = new GameObject("Glow", typeof(Image)).GetComponent<Image>();
        // 加算合成マテリアルを設定
        _glowImage.material = Resources.Load<Material>("UI-Additive");
        _glowImage.transform.SetParent(_image.transform, false);
        _glowImage.gameObject.layer = _image.gameObject.layer;
        _glowImage.rectTransform.sizeDelta = _image.rectTransform.sizeDelta;

        _preBlurSig = _blurSig;
        _preOrifinSprite = _image.sprite;
    }

    /// <summary>
    /// 発光表現用のぼかし画像の更新
    /// </summary>
    private void UpdateGlow()
    {
        if (_image == null)
        {
            // ベースのImageを取得していなかったら初期化
            Initialize();
        }

        if (_image.sprite == null)
        {
            // ベースのImageの画像が設定されていなかったら何もしない
            _glowImage.sprite = null;
            return;
        }

        _glowImage.color = _glowColor;

        if (_glowImage.sprite != null && _preBlurSig == _blurSig && _preOrifinSprite == _image.sprite)
        {
            // ぼかし距離とベースImageに変更がなければぼかし画像の再生成をしない
            return;
        }

        Sprite preGlowSprite = _glowImage.sprite;

        Texture2D blurTex = CreateBlurTexture(_image.sprite.texture, _blurSig);
        Sprite blurSprite = Sprite.Create(blurTex, _image.sprite.rect, _image.rectTransform.pivot);
        _glowImage.sprite = blurSprite;
        _glowImage.rectTransform.sizeDelta = _image.rectTransform.sizeDelta;

#if UNITY_EDITOR
        // エディターでのみ分かりやすいようにスプライト名をつける
        blurSprite.name = _image.sprite.name + " blur";
#endif

        // 使わなくなったスプライトを破棄
        DestroySprite(preGlowSprite);

        _preBlurSig = _blurSig;
        _preOrifinSprite = _image.sprite;
    }

    /// <summary>
    /// 更新
    /// </summary>
    private void Update()
    {
        _glowImage.rectTransform.sizeDelta = _image.rectTransform.sizeDelta;
        _glowImage.color = _glowColor;
    }

    /// <summary>
    /// 削除時に呼び出し
    /// </summary>
    private void OnDestroy()
    {
        DestroySprite(_glowImage.sprite);
    }

    /// <summary>
    /// スプライトの破棄
    /// </summary>
    private void DestroySprite(Sprite sprite)
    {
        if (sprite == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(sprite.texture);
            Destroy(sprite);
        }
        else
        {
            // エディターモードでは即時破棄
            DestroyImmediate(sprite.texture);
            DestroyImmediate(sprite);
        }
    }

    /// <summary>
    /// ぼかし画像を生成
    /// https://qiita.com/divideby_zero/items/4c02177a56f7d500d4c0
    /// </summary>
    /// <param name="sig">ぼかし距離</param>
    private static Texture2D CreateBlurTexture(Texture2D tex, float sig)
    {
        sig = Mathf.Max(sig, 0f);
        int W = tex.width;
        int H = tex.height;
        int Wm = (int)(Mathf.Ceil(3.0f * sig) * 2 + 1);
        int Rm = (Wm - 1) / 2;

        //フィルタ
        float[] msk = new float[Wm];

        sig = 2 * sig * sig;
        float div = Mathf.Sqrt(sig * Mathf.PI);

        //フィルタの作成
        for (int x = 0; x < Wm; x++)
        {
            int p = (x - Rm) * (x - Rm);
            msk[x] = Mathf.Exp(-p / sig) / div;
        }

        var src = tex.GetPixels(0).Select(x => x.a).ToArray();
        var tmp = new float[src.Length];
        var dst = new Color[src.Length];

        //垂直方向
        for (int x = 0; x < W; x++)
        {
            for (int y = 0; y < H; y++)
            {
                float sum = 0;
                for (int i = 0; i < Wm; i++)
                {
                    int p = y + i - Rm;
                    if (p < 0 || p >= H) continue;
                    sum += msk[i] * src[x + p * W];
                }
                tmp[x + y * W] = sum;
            }
        }
        //水平方向
        for (int x = 0; x < W; x++)
        {
            for (int y = 0; y < H; y++)
            {
                float sum = 0;
                for (int i = 0; i < Wm; i++)
                {
                    int p = x + i - Rm;
                    if (p < 0 || p >= W) continue;
                    sum += msk[i] * tmp[p + y * W];
                }
                dst[x + y * W] = new Color(1, 1, 1, sum);
            }
        }

        var createTexture = new Texture2D(W, H);
        createTexture.SetPixels(dst);
        createTexture.Apply();

        return createTexture;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ImageFakeBloom))]
    public class ImageFakeBloomEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ImageFakeBloom target = this.target as ImageFakeBloom;
            if(target._glowImage == null || target._glowImage.sprite == null)
            {
                return;
            }

            // 発光表現用に生成された画像をプロジェクトに保存するボタン
            if (GUILayout.Button("発光画像を保存"))
            {
                SaveBlurSprite(target._glowImage.sprite);
            }
        }

        /// <summary>
        /// 渡されたSpriteをプロジェクトに保存
        /// </summary>
        private void SaveBlurSprite(Sprite sprite)
        {
            string path = EditorUtility.SaveFilePanelInProject("発光画像を保存", sprite.name, "png", "保存する画像名を入力してください");
            if (path.Length == 0)
            {
                // 保存キャンセル
                return;
            }

            byte[] pngData = sprite.texture.EncodeToPNG();
            if (pngData == null)
            {
                Debug.LogError("画像データの取得に失敗しました");
                return;
            }

            File.WriteAllBytes(path, pngData);
            AssetDatabase.Refresh();

            // テクスチャタイプをSpriteに変更
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (textureImporter != null)
            {
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.SaveAndReimport();
            }
        }
    }
#endif
}
