using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement; // 💡 必须引入场景管理命名空间

public class UIMover : MonoBehaviour
{
    public List<RectTransform> uiObjects;
    public List<GameObject> uiSetNotActive;
    [Header("需要显示的文本列表（按顺序放）")]
    public List<TextMeshProUGUI> identifyText;

    public float initialSpeed = 40f;
    public float acceleration = 200f;

    [Header("文本动画设置")]
    public float textFadeDuration = 0.5f;
    public float textMoveOffset = 30f;
    public float delayBetweenLines = 0.3f;

    [Header("移动到该 X 坐标后消失")]
    public float disappearX = 600f;

    [Header("最后要跳转的场景名字")]
    public string openingScene = "OpeningScene";

    private float currentSpeed;

    // 💡 状态机枚举，用来标记当前进行到哪一步了
    private enum StepState { MovingUI, PlayingTextAnim, TextFinished }
    private StepState currentState = StepState.MovingUI;

    private List<Vector3> originalPositions = new List<Vector3>();
    private Coroutine textAnimCoroutine; // 记录协程引用，方便随时掐断

    public bool ifMove = false;

    void Start()
    {
        currentSpeed = initialSpeed;

        // 游戏一开始，通过 CanvasGroup 让文字隐形
        for (int i = 0; i < identifyText.Count; i++)
        {
            TextMeshProUGUI text = identifyText[i];
            if (text != null)
            {
                originalPositions.Add(text.rectTransform.localPosition);

                CanvasGroup canvasGroup = text.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = text.gameObject.AddComponent<CanvasGroup>();
                }

                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;

                text.gameObject.SetActive(true);
            }
            else
            {
                originalPositions.Add(Vector3.zero);
            }
        }
    }

    void Update()
    {
        if (!ifMove)
            return;

        // 核心：每帧检测鼠标左键点击
        if (Input.GetMouseButtonDown(0))
        {
            HandleOnClicked();
            return; // 点击帧不处理常规物理移动，防止逻辑冲突
        }

        foreach (var ui in uiSetNotActive)
            ui.SetActive(false);

        // 常规状态机逻辑
        switch (currentState)
        {
            case StepState.MovingUI:
                if (uiObjects.Count > 0)
                {
                    currentSpeed += acceleration * Time.deltaTime;

                    for (int i = uiObjects.Count - 1; i >= 0; i--)
                    {
                        RectTransform rect = uiObjects[i];
                        if (rect != null)
                        {
                            rect.localPosition += new Vector3(currentSpeed * Time.deltaTime, 0, 0);

                            if (rect.localPosition.x > disappearX)
                            {
                                rect.gameObject.SetActive(false);
                                uiObjects.RemoveAt(i);
                            }
                        }
                    }
                }
                else
                {
                    // UI 控件自然移动结束，进入播文本动画阶段
                    currentState = StepState.PlayingTextAnim;
                    textAnimCoroutine = StartCoroutine(PlayPPTTextAnimation());
                }
                break;

            case StepState.PlayingTextAnim:
                // 协程正在后台播放，这里不需要做任何事
                break;

            case StepState.TextFinished:
                // 文本已经全部显示完毕，等待玩家点击
                break;
        }
    }

    // 💡 处理点击的中央控制中心
    void HandleOnClicked()
    {
        if (currentState == StepState.MovingUI)
        {
            Debug.Log("【玩家点击】UI控件未移动完 -> 直接处理掉控件，开始播文本动画");

            // 1. 直接强行隐藏所有还没走完的UI控件，并清空列表
            for (int i = 0; i < uiObjects.Count; i++)
            {
                if (uiObjects[i] != null) uiObjects[i].gameObject.SetActive(false);
            }
            uiObjects.Clear();

            // 2. 切换状态并开始播动画
            currentState = StepState.PlayingTextAnim;
            textAnimCoroutine = StartCoroutine(PlayPPTTextAnimation());
        }
        else if (currentState == StepState.PlayingTextAnim)
        {
            Debug.Log("【玩家点击】文本动画未结束 -> 强行掐断动画，让文本瞬间直接全部显示");

            // 1. 强行杀死正在播放的协程
            if (textAnimCoroutine != null)
            {
                StopCoroutine(textAnimCoroutine);
            }

            // 2. 瞬间把所有文本复位到最终目标状态
            for (int i = 0; i < identifyText.Count; i++)
            {
                TextMeshProUGUI text = identifyText[i];
                if (text == null) continue;

                CanvasGroup canvasGroup = text.GetComponent<CanvasGroup>();
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                text.rectTransform.localPosition = originalPositions[i];
            }

            // 3. 状态切换为结束
            currentState = StepState.TextFinished;
        }
        else if (currentState == StepState.TextFinished)
        {
            Debug.Log("【玩家点击】文本已完全显示 -> 跳转场景到：" + openingScene);

            // 跳转到目标场景
            SceneManager.LoadScene(openingScene);
        }
    }

    // 协程：模拟 PPT 一行行从上往下“推入”的动画
    IEnumerator PlayPPTTextAnimation()
    {
        for (int i = 0; i < identifyText.Count; i++)
        {
            TextMeshProUGUI text = identifyText[i];
            if (text == null) continue;

            CanvasGroup canvasGroup = text.GetComponent<CanvasGroup>();
            RectTransform textRect = text.rectTransform;

            Vector3 originalLocalPos = originalPositions[i];
            Vector3 startLocalPos = originalLocalPos + new Vector3(0, textMoveOffset, 0);

            float timer = 0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

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

            yield return new WaitForSeconds(delayBetweenLines);
        }

        // 💡 只有当协程顺利把所有字都数完了，才会自动走到这里
        currentState = StepState.TextFinished;
    }
}