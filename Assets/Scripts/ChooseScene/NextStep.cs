using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 💡 新增：用于场景跳转

public class NextStep : MonoBehaviour
{
    [Header("需要控制启用的 Flashcard 对象")]
    public GameObject Flashcard;

    private Button myButton;
    private TextMeshProUGUI buttonText;
    private string originalText = "查看结果";
    private string flashcardText = "下一次选择";
    private string chooseFirstText = "请先选择";
    bool ifChoose = true;

    [Header("用于处理每天的转场")]
    public GameObject Transition;

    [Header("渐变速度配置")]
    [Tooltip("黑幕淡入全屏所需的时间（秒）")]
    public float blackFadeInTime = 0.5f;
    [Tooltip("文本淡入显现所需的时间（秒）")]
    public float textFadeInTime = 0.25f;
    [Tooltip("文本完全显现后，在屏幕上的停留时间（秒）")]
    public float textStayTime = 0.5f;
    [Tooltip("黑幕和文本同时淡出所需的时间（秒）")]
    public float fadeOutTime = 0.5f;

    private CanvasGroup blackScreenCG;
    private CanvasGroup textCG;
    private TextMeshProUGUI transitionText;

    private Coroutine restoreTextCoroutine;

    public bool over = false;   // 游戏结束
    public string GameOverScene;    // 游戏结束后的场景

    private void Awake()
    {
        if (Transition != null)
        {
            Transform imageTrans = Transition.transform.Find("Image");
            Transform textTrans = Transition.transform.Find("Text");

            if (imageTrans != null) blackScreenCG = imageTrans.GetComponent<CanvasGroup>();
            if (textTrans != null)
            {
                textCG = textTrans.GetComponent<CanvasGroup>();
                transitionText = textTrans.GetComponent<TextMeshProUGUI>();
            }
        }
    }

    void Start()
    {
        myButton = GetComponent<Button>();
        if (myButton != null)
        {
            myButton.onClick.AddListener(OnNextButtonClick);
        }

        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = originalText;

        if (Flashcard != null) Flashcard.SetActive(false);
        if (Transition != null) Transition.SetActive(false);

        // 游戏刚启动第一天：初始化选项并直接强制开场黑幕转场
        Barry_Round.Ini();
        Toggle.Instance.IniOptions();
        StartDayTransition($"第 {Barry_Round.Barry} 天", isStartImmediate: true);
    }

    void OnNextButtonClick()
    {
        if (Toggle.Instance == null || Toggle.Instance.toggleObjects == null || Toggle.Instance.toggleObjects.Count == 0)
        {
            Debug.LogError("请先在 Inspector 中将控制器（Toggle）拖入 Toggle Controller 格子！");
            return;
        }

        if (ifChoose)
        {
            int index = Toggle.currentIndex;

            if (index == -1)
            {
                if (buttonText != null)
                {
                    buttonText.text = chooseFirstText;
                    if (restoreTextCoroutine != null) StopCoroutine(restoreTextCoroutine);
                    restoreTextCoroutine = StartCoroutine(RestoreButtonText());
                }
            }
            else
            {
                var obj = Toggle.Instance.toggleObjects[index];
                var titleTransform = obj.GetComponent<Transform>().Find("Title");
                var textMesh = titleTransform.GetComponent<TextMeshProUGUI>();
                string text = textMesh.text;
                GameOverCheck.Instance.Chosen(text);

                TurnToFlashcard();
            }
        }
        else // 知识卡片
        {
            int currentEnergy = Toggle.Instance.progress.Energy.Get();
            bool isEnergyExhausted = CheckIfEnergyExhausted(currentEnergy);

            if (!isEnergyExhausted)
            {
                if (ReturnToCurrentDaySelection())  // 当天还没有完成
                    return; // 如果没有成功返回当天，则会继续下面的逻辑
            }

            // 当天已经完成，需要前往下一天或结束
            Barry_Round.NextBarry();

            if (GameOverCheck.Instance.IfOver
                (Toggle.Instance.progress.Health.Get(),
                Toggle.Instance.progress.Mood.Get()))
                FinalGame();
            else if (Barry_Round.IfFinishBarries())
                FinalGame();    // 完成所有天数
            else
                TurnToNextDay();
        }
    }

    void TurnToFlashcard()
    {
        Flashcard.SetActive(true);
        Toggle.Instance.CloseOptions();
        buttonText.text = flashcardText;
        ifChoose = false;

        ChangeFlashcard();
    }

    bool ReturnToCurrentDaySelection()
    {
        Flashcard.SetActive(false);
        buttonText.text = originalText;
        ifChoose = true;

        // 说明没有足够的选项以供使用，但是直接这样做很容易出现重复或多关的问题
        // 用一个布尔表示是否真的有更多东西
        //if (!Toggle.Instance.IniOptions())
        //    TurnToNextDay();
        return Toggle.Instance.IniOptions();
    }

    void TurnToNextDay()
    {
        StartDayTransition($"第 {Barry_Round.Barry} 天", onScreenBlack: () =>
        {
            Flashcard.SetActive(false);
            buttonText.text = originalText;
            ifChoose = true;

            Toggle.Instance.progress.Energy.Initialize();
            Toggle.Instance.IniOptions();
        });
    }

    /// <summary>
    /// 💡 游戏结束了（已修改：传入 isGameOver = true）
    /// </summary>
    void FinalGame()
    {
        GameOverCheck.Instance.AdjustOverTitle(Toggle.Instance.progress.Health.Get(),
            Toggle.Instance.progress.Mood.Get());

        string endTitle = GameOverCheck.Instance.OverTitle;
        Debug.Log($"[游戏结束文本测试]: {endTitle}");

        // 💡 重点：这里给第三个参数传了 true，代表这是游戏结束转场
        StartDayTransition($"{endTitle}", onScreenBlack: () =>
        {
            if (Flashcard != null)
                Flashcard.SetActive(false);
        }, isStartImmediate: false, isGameOver: true);
    }

    private bool CheckIfEnergyExhausted(int currentEnergy)
    {
        return currentEnergy <= 0;
    }

    void ChangeFlashcard()
    {
        Transform txtTrans = Flashcard.transform.Find("Text");
        if (txtTrans != null)
        {
            TextMeshProUGUI text = txtTrans.GetComponent<TextMeshProUGUI>();
            int index = Toggle.currentIndex;
            text.text = Toggle.Instance.cards[index].knowledgeText;
        }
    }

    IEnumerator RestoreButtonText()
    {
        yield return new WaitForSeconds(1.5f);
        if (buttonText != null) buttonText.text = originalText;
        restoreTextCoroutine = null;
    }

    // ------------------ 转场核心协程 (已调整支持游戏结束不消失) ------------------

    // 重载方法，为了兼容 Start() 里的旧调用，给 isGameOver 加了默认值 false
    public void StartDayTransition(string dayText, System.Action onScreenBlack = null, bool isStartImmediate = false, bool isGameOver = false)
    {
        if (Transition == null || blackScreenCG == null || textCG == null) return;
        if (transitionText != null) transitionText.text = dayText;
        StartCoroutine(TransitionRoutine(onScreenBlack, isStartImmediate, isGameOver));
    }

    private IEnumerator TransitionRoutine(System.Action onScreenBlack, bool isStartImmediate, bool isGameOver)
    {
        Transition.SetActive(true);

        // 确保 Transition 及其子物体的 Image 能够阻挡并接收点击
        var mainImage = Transition.GetComponent<Image>();
        if (mainImage != null) mainImage.raycastTarget = true;

        Transform imgTrans = Transition.transform.Find("Image");
        if (imgTrans != null && imgTrans.GetComponent<Image>() != null)
        {
            imgTrans.GetComponent<Image>().raycastTarget = true;
        }

        blackScreenCG.alpha = isStartImmediate ? 1f : 0f;
        textCG.alpha = 0f;

        if (!isStartImmediate)
        {
            float timer = 0f;
            while (timer < blackFadeInTime)
            {
                timer += Time.deltaTime;
                blackScreenCG.alpha = Mathf.Lerp(0f, 1f, timer / blackFadeInTime);
                yield return null;
            }
        }
        blackScreenCG.alpha = 1f;

        if (onScreenBlack != null) onScreenBlack.Invoke();

        float textTimer = 0f;
        while (textTimer < textFadeInTime)
        {
            textTimer += Time.deltaTime;
            textCG.alpha = Mathf.Lerp(0f, 1f, textTimer / textFadeInTime);
            yield return null;
        }
        textCG.alpha = 1f;

        // 核心改动位置
        if (isGameOver)
        {
            // 如果是游戏结束，不再走后面的淡出逻辑，而是就地挂载/启用点击事件
            Button transitionButton = Transition.GetComponent<Button>();
            if (transitionButton == null)
            {
                // 如果没有 Button 组件，代码动态赋予一个
                transitionButton = Transition.AddComponent<Button>();
            }

            // 清除可能残存的监听，并绑定跳转场景的方法
            transitionButton.onClick.RemoveAllListeners();
            transitionButton.onClick.AddListener(ToGameOverScene);

            yield break; // 彻底斩断协程，让画面永远停在这一帧，等待玩家点击
        }

        // --- 以下是原本的正常过天淡出逻辑 ---
        yield return new WaitForSeconds(textStayTime);

        float fadeOutTimer = 0f;
        while (fadeOutTimer < fadeOutTime)
        {
            fadeOutTimer += Time.deltaTime;
            float progress = fadeOutTimer / fadeOutTime;
            blackScreenCG.alpha = Mathf.Lerp(1f, 0f, progress);
            textCG.alpha = Mathf.Lerp(1f, 0f, progress);
            yield return null;
        }
        blackScreenCG.alpha = 0f;
        textCG.alpha = 0f;
        Transition.SetActive(false);
    }

    /// <summary>
    /// 💡 点击黑幕后触发此函数跳转场景
    /// </summary>
    private void ToGameOverScene()
    {
        Debug.Log("检测到黑幕点击，正在切往 GameOverScence...");
        SceneManager.LoadScene(GameOverScene);
    }
}