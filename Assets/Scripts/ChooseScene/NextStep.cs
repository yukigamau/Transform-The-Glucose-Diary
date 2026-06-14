using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    public float textStayTime = 1f;
    [Tooltip("黑幕和文本同时淡出所需的时间（秒）")]
    public float fadeOutTime = 0.5f;

    private CanvasGroup blackScreenCG;
    private CanvasGroup textCG;
    private TextMeshProUGUI transitionText;

    private int barry = 0; // 当前关卡数/天数
    private Coroutine restoreTextCoroutine;

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
        barry = 1;
        Toggle.Instance.IniOptions();
        StartDayTransition($"第 {barry} 天", isStartImmediate: true);
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
            // 💡 阶段一：当前在选择界面，必须先选择一张卡牌才能去看卡片
            if (Toggle.currentIndex == -1)
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
                TurnToFlashcard(); // 顺利去看知识卡片
            }
        }
        else
        {
            // 💡 阶段二：当前在知识卡片界面，点击按钮准备返回
            // 我们需要检查剩余精力是否还能支撑下一轮卡牌
            int currentEnergy = Toggle.Instance.progress.Energy.Get();

            // 问一下 Toggle 里的回合数（如果你的 Toggle 脚本里是用 curRound，记得对齐）
            // 我们通过一个临时逻辑判断当前精力是不是“连最省精力的牌都用不起了”
            bool isEnergyExhausted = CheckIfEnergyExhausted(currentEnergy);

            if (isEnergyExhausted)
            {
                // ➔ A. 精力确实用完了：触发黑幕转场，并推进天数进入下一天
                TurnToNextDay();
            }
            else
            {
                // ➔ B. 精力还有剩：不转场跳天，直接掀开黑幕回当前这一天继续选！
                ReturnToCurrentDaySelection();
            }
        }
    }

    // 转换到知识卡片页面
    void TurnToFlashcard()
    {
        Flashcard.SetActive(true);
        Toggle.Instance.CloseOptions(); // 注意：这里面已经跑了 Settle 扣了精力，且 curRound++ 了
        buttonText.text = flashcardText;
        ifChoose = false;

        ChangeFlashcard();
    }

    /// <summary>
    /// 💡 新增逻辑：精力还有剩，不跨天，直接回当前天继续选择
    /// </summary>
    void ReturnToCurrentDaySelection()
    {
        Flashcard.SetActive(false);
        buttonText.text = originalText;
        ifChoose = true;

        // 刷新卡池，由于还在当天，barry 没有自增，会继续留在原地消耗精力
        if (!Toggle.Instance.IniOptions())
            TurnToNextDay();    // 已经没有选项了
    }

    /// <summary>
    /// 💡 改名整合：精力用完了，平滑过渡到全新的一天（带黑幕转场）
    /// </summary>
    void TurnToNextDay()
    {
        barry++; // 推进天数

        StartDayTransition($"第 {barry} 天", onScreenBlack: () =>
        {
            Flashcard.SetActive(false);
            buttonText.text = originalText;
            ifChoose = true;

            Toggle.Instance.progress.Energy.Initialize();

            // 💡 核心：通知 Toggle 同步更新它的当前关卡数
            Toggle.Instance.SetBarry(barry);

            // 在全黑的掩护下，重新刷出一池满精力的全新卡牌
            Toggle.Instance.IniOptions();
        });
    }

    /// <summary>
    /// 💡 核心检查器：判断当前剩余精力是不是什么牌都用不起了
    /// </summary>
    private bool CheckIfEnergyExhausted(int currentEnergy)
    {
        // 理论上点外卖消耗是0，如果卡池里永远有0消耗的低级垃圾食品卡，单纯判断 currentEnergy <= 0 可能会卡死。
        // 所以最稳妥的做法是：如果剩余精力小于等于0，或者已经无法支持卡池里的任何卡牌，就判定为耗尽。
        // 这里可以直接采用对齐你需求的极简规则：当前精力 <= 0 即视为可以过天
        return currentEnergy <= 0;
    }

    // 修改知识卡片的文本
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

    // ------------------ 转场核心协程 (保持原样不动) ------------------
    public void StartDayTransition(string dayText, System.Action onScreenBlack = null, bool isStartImmediate = false)
    {
        if (Transition == null || blackScreenCG == null || textCG == null) return;
        if (transitionText != null) transitionText.text = dayText;
        StartCoroutine(TransitionRoutine(onScreenBlack, isStartImmediate));
    }

    private IEnumerator TransitionRoutine(System.Action onScreenBlack, bool isStartImmediate)
    {
        Transition.SetActive(true);
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
}