using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NextStep : MonoBehaviour
{
    [Header("需要控制启用的 Flashcard 对象")]
    public GameObject flashCard;

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
    public GameObject finalImage;   // 游戏结束时要显示的图片

    [Header("音效")]
    public AudioSource audioSource;
    public AudioSource turnToNextDayAudio;
    public AudioClip final;

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

        if (flashCard != null) flashCard.SetActive(false);
        if (Transition != null) Transition.SetActive(false);
        if (finalImage != null) finalImage.SetActive(false); // 💡 新增：开局确保结算图关闭

        // 游戏刚启动第一天：初始化选项并直接强制开场黑幕转场
        Toggle.Instance.IniOptions();
        StartDayTransition($"第 {Barry_Round.Barry} 天", isStartImmediate: true);

        Random.InitState((int)System.DateTime.Now.Ticks);
    }

    void OnNextButtonClick()
    {
        audioSource.Play();

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
                string title = Text.FirstLine(text);
                Debug.Log($"{title}被处理Chosen");
                GameOverCheck.Instance.Chosen(title);

                TurnToFlashcard();
            }
        }
        else // 知识卡片
        {
            float currentEnergy = Barry_Round.Energy.Get();
            bool isEnergyExhausted = CheckIfEnergyExhausted(currentEnergy);

            if (!isEnergyExhausted)
            {
                if (ReturnToCurrentDaySelection())  // 当天还没有完成
                    return; // 如果没有成功返回当天，则会继续下面的逻辑
            }

            Debug.Log("准备进入下一天");

            // 当天已经完成，需要前往下一天或结束
            Barry_Round.NextBarry();

            if (GameOverCheck.Instance.IfOver
                (Barry_Round.Health.Get(),
                Barry_Round.Mood.Get()))
                FinalGame();
            else if (Barry_Round.IfFinishBarries())
                FinalGame();    // 完成所有天数
            else
                TurnToNextDay();
        }
    }

    void TurnToFlashcard()
    {
        flashCard.SetActive(true);
        Toggle.Instance.CloseOptions();
        buttonText.text = flashcardText;
        ifChoose = false;

        Transform flashCardTrans = flashCard.transform;
        FlashCard fc = flashCardTrans.GetComponent<FlashCard>();
        int index = Toggle.currentIndex;
        fc.Change(Toggle.Instance.cards[index]);
    }

    bool ReturnToCurrentDaySelection()
    {
        flashCard.SetActive(false);
        buttonText.text = originalText;
        ifChoose = true;
        return Toggle.Instance.IniOptions();
    }

    void TurnToNextDay()
    {
        turnToNextDayAudio.Play();

        StartDayTransition($"第 {Barry_Round.Barry} 天", onScreenBlack: () =>
        {
            flashCard.SetActive(false);
            buttonText.text = originalText;
            ifChoose = true;

            Barry_Round.Energy.Initialize();
            Toggle.Instance.IniOptions();
            if (finalImage != null) finalImage.SetActive(false); // 安全防范
        });
    }

    void PlayFinalSound()
    {
        turnToNextDayAudio.clip = final;
        turnToNextDayAudio.Play();
    }

    /// <summary>
    /// 💡 修正后的游戏结束逻辑
    /// </summary>
    void FinalGame()
    {
        PlayFinalSound();

        GameOverCheck.Instance.AdjustOverTitle(Barry_Round.Health.Get(),
            Barry_Round.Mood.Get());

        string endText = GameOverCheck.Instance.OverTitle;
        Debug.Log($"[游戏结束文本测试]: {endText}");

        // 💡 重点：这里不再直接在这里调用 SetFinalPhoto。
        // 我们把它挪到黑幕完全变黑的瞬间（onScreenBlack 委托里）去执行！
        StartDayTransition($"{endText}", onScreenBlack: () =>
        {
            // 1. 关闭普通游戏界面
            if (flashCard != null) flashCard.SetActive(false);

            // 2. ✨ 当黑幕完全全黑遮挡住原本空洞的场景时，在黑幕内部/前方把图片加载出来！
            string endTitle = GameOverCheck.Instance.GetSpecial();
            SetFinalPhoto(endTitle);

        }, isStartImmediate: false, isGameOver: true);
    }

    void SetFinalPhoto(string endTitle)
    {
        if (finalImage == null) return;
        FinalImage fi = finalImage.GetComponent<FinalImage>();
        finalImage.SetActive(true);
        if (fi != null)
        {
            fi.SetImage(endTitle);
        }
    }

    private bool CheckIfEnergyExhausted(float currentEnergy)
    {
        return currentEnergy <= 0;
    }

    IEnumerator RestoreButtonText()
    {
        yield return new WaitForSeconds(1.5f);
        if (buttonText != null) buttonText.text = originalText;
        restoreTextCoroutine = null;
    }

    // ------------------ 转场核心协程 (修复层级空缺与文本重置) ------------------

    public void StartDayTransition(string dayText, System.Action onScreenBlack = null, bool isStartImmediate = false, bool isGameOver = false)
    {
        if (Transition == null || blackScreenCG == null || textCG == null) return;
        if (transitionText != null) transitionText.text = dayText;
        StartCoroutine(TransitionRoutine(onScreenBlack, isStartImmediate, isGameOver));
    }

    private IEnumerator TransitionRoutine(System.Action onScreenBlack, bool isStartImmediate, bool isGameOver)
    {
        Transition.SetActive(true);

        // 💡 修复：确保每次非游戏结束转场时，把对齐方式重置回正中心
        if (!isGameOver && transitionText != null)
        {
            transitionText.alignment = TextAlignmentOptions.Center;
        }

        var mainImage = Transition.GetComponent<Image>();
        if (mainImage != null) mainImage.raycastTarget = true;

        Transform imgTrans = Transition.transform.Find("Image");
        if (imgTrans != null && imgTrans.GetComponent<Image>() != null)
        {
            imgTrans.GetComponent<Image>().raycastTarget = true;
        }

        blackScreenCG.alpha = isStartImmediate ? 1f : 0f;
        textCG.alpha = 0f;

        // 1. 黑幕淡入阶段
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

        // 2. ✨ 黑幕此时完全变黑。触发委托（隐藏旧卡片，生成/激活 FinalImage）
        if (onScreenBlack != null) onScreenBlack.Invoke();

        // 3. 如果是游戏结束，文字改到底部居中
        if (isGameOver && transitionText != null)
        {
            transitionText.alignment = TextAlignmentOptions.Bottom;
        }

        // 4. 文字（与图片，如果图片挂在Text节点或与其同步）淡入阶段
        float textTimer = 0f;
        while (textTimer < textFadeInTime)
        {
            textTimer += Time.deltaTime;
            textCG.alpha = Mathf.Lerp(0f, 1f, textTimer / textFadeInTime);
            yield return null;
        }
        textCG.alpha = 1f;

        // 5. 游戏结束拦截
        if (isGameOver)
        {
            Button transitionButton = Transition.GetComponent<Button>();
            if (transitionButton == null)
            {
                transitionButton = Transition.AddComponent<Button>();
            }

            transitionButton.onClick.RemoveAllListeners();
            transitionButton.onClick.AddListener(ToGameOverScene);

            yield break; // 彻底停在这里，画面定格
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

    private void ToGameOverScene()
    {
        Debug.Log($"检测到黑幕点击，正在切往 {GameOverScene}...");
        SceneManager.LoadScene(GameOverScene);
    }
}