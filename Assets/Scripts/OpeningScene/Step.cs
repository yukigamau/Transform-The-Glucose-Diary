using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Step : MonoBehaviour
{
    public List<Sprite> photos;
    public List<string> texts;
    public Image image;
    public TextMeshProUGUI tip;
    public string fullTipScene;

    [Header("把场景中铺满全屏的黑色 Image 的 CanvasGroup 拖进来")]
    public CanvasGroup blackCanvasGroup;

    [Header("转场速度设置")]
    public float fadeDuration = 0.5f; // 黑布变黑或变透明需要的时间（秒）

    private int curID = 0;
    private bool isTransitioning = false; // 锁：防止玩家在黑布转场时疯狂点击导致逻辑错乱

    public AudioSource audioSource;

    void Start()
    {
        // 游戏一开始，确保黑布是完全不透明的（全黑）
        if (blackCanvasGroup != null)
        {
            blackCanvasGroup.alpha = 1f;
            blackCanvasGroup.blocksRaycasts = true; // 遮挡点击
        }

        // 启动时直接进入第一次展现（从全黑淡出显现第一张）
        StartCoroutine(TransitionToNextStep(true));
    }

    void Update()
    {
        // 监听鼠标左键点击，且当前没有在转场中
        if (Input.GetMouseButtonDown(0) && !isTransitioning)
        {
            StartCoroutine(TransitionToNextStep(false));
        }
    }

    // 核心协程：处理黑布的淡入淡出与内容更换
    IEnumerator TransitionToNextStep(bool isFirstStart)
    {
        audioSource.Play();

        isTransitioning = true;

        // --- 1. 如果不是第一次启动，需要先让黑布【淡入】（画面变黑） ---
        if (!isFirstStart)
        {
            yield return StartCoroutine(Fade(0f, 1f));
        }

        // --- 2. 此时画面处于全黑状态，在幕后安全更换内容 ---
        if (curID < photos.Count)
        {
            image.sprite = photos[curID];

            // 安全检查：防止文本列表和图片列表长度不一致导致报错
            if (curID < texts.Count)
            {
                tip.text = texts[curID];
            }
            else
            {
                tip.text = ""; // 如果没有对应文本则清空
            }

            curID++;

            // --- 3. 更换完毕，让黑布【淡出】（画面变亮，露出新内容） ---
            yield return StartCoroutine(Fade(1f, 0f));
            isTransitioning = false; // 转场结束，重新允许玩家点击
        }
        else
        {
            // --- 4. 如果图片已经放完了，直接跳转场景 ---
            Debug.Log("内容播放完毕，正在跳转场景...");
            SceneManager.LoadScene(fullTipScene);
        }
    }

    // 专职负责控制透明度变化的数学平滑协程
    IEnumerator Fade(float startAlpha, float endAlpha)
    {
        if (blackCanvasGroup == null) yield break;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;

            // 使用 Mathf.SmoothStep 让淡入淡出更柔和
            blackCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, Mathf.SmoothStep(0f, 1f, progress));
            yield return null;
        }

        // 确保数值精准对齐
        blackCanvasGroup.alpha = endAlpha;

        // 性能与交互小优化：如果全黑，挡住所有点击；如果变透明，允许点击穿透黑布
        blackCanvasGroup.blocksRaycasts = (endAlpha == 1f);
    }
}