using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 定义动画状态枚举（根据你代码末尾的 StepState 补全）
public enum StepState
{
    Playing,
    Finished
}

public class Animation : MonoBehaviour
{
    [Header("动画配置")]
    [SerializeField] private float textMoveOffset = -50f;     // 文字移动偏移量（负数表示从下往上飘）
    [SerializeField] private float textFadeDuration = 0.5f;   // 渐显持续时间
    [SerializeField] private float delayBetweenLines = 0.2f;   // 每行文字之间的延迟时间
    [SerializeField] public List<GameObject> gameObjects;

    [HideInInspector]
    public StepState currentState; // 当前动画状态

    // 用于缓存初始位置的列表
    private List<Vector3> originalPositions = new List<Vector3>();

    private void Start()
    {
        foreach (var gameObject in gameObjects)
            gameObject.SetActive(false);
        StartPPTAnimation(gameObjects);
    }

    /// <summary>
    /// 外部播放动画的入口
    /// </summary>
    public void StartPPTAnimation(List<GameObject> gameObjects)
    {
        // 1. 初始化并记录所有物体的原本初始位置
        originalPositions.Clear();
        foreach (var obj in gameObjects)
        {
            if (obj != null)
            {
                originalPositions.Add(obj.transform.localPosition);
            }
            else
            {
                originalPositions.Add(Vector3.zero); // 占位，保持索引一致
            }
        }

        // 2. 修改状态并启动协程
        currentState = StepState.Playing;
        StartCoroutine(PlayPPTTextAnimation(gameObjects));
    }

    private IEnumerator PlayPPTTextAnimation(List<GameObject> gameObjects)
    {
        for (int i = 0; i < gameObjects.Count; i++)
        {
            GameObject gameObject = gameObjects[i];
            if (gameObject == null) continue;

            gameObject.SetActive(true);

            // 🛠️ 修复 1：CanvasGroup 如果没有，自动添加一个，防止空指针报错
            CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // 🛠️ 修复 2：修改了错误的 GetComponents 获取方式
            RectTransform textRect = gameObject.GetComponent<RectTransform>();
            if (textRect == null) continue; // 如果不是UI元素则跳过

            // 🛠️ 修复 3：确保索引安全，防止 originalPositions 越界
            Vector3 originalLocalPos = (i < originalPositions.Count) ? originalPositions[i] : textRect.localPosition;
            Vector3 startLocalPos = originalLocalPos + new Vector3(0, textMoveOffset, 0);

            float timer = 0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 0f; // 动画开始前先隐形

            while (timer < textFadeDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / textFadeDuration;
                float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

                textRect.localPosition = Vector3.Lerp(startLocalPos, originalLocalPos, smoothProgress);
                canvasGroup.alpha = smoothProgress;

                yield return null;
            }

            textRect.localPosition = originalLocalPos;
            canvasGroup.alpha = 1f;

            // 如果是最后一行，就不需要再等待延迟了
            if (i < gameObjects.Count - 1)
            {
                yield return new WaitForSeconds(delayBetweenLines);
            }
        }

        // 💡 只有当协程顺利把所有字都数完了，才会自动走到这里
        currentState = StepState.Finished;
    }
}