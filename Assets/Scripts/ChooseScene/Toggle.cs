using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Toggle : MonoBehaviour
{
    // 静态单例，方便 2D 游戏对象直接调用
    public static Toggle Instance;

    [Header("把那 3 个可点击的 2D 游戏对象拖到这个列表里")]
    public List<GameObject> toggleObjects;
    private List<string> comments;
    public List<CardData> cards;

    // 当前处于被选中状态的对象索引，-1表示初始状态没有任何对象被选中
    public static int currentIndex = -1;
    public TextMeshProUGUI Comment;
    private List<List<int>> delta;

    private int curRound;
    public int MaxBarry = 12;   // 最大关卡数
    private int barry = 0;  // 当前关卡数

    public ProgressBar progress;

    void Awake()
    {
        Instance = this;
        comments = new List<string>();
        cards = new List<CardData>();
    }

    void Start()
    {
        Comment.text = "";

        curRound = 0;
    }

    // 💡 核心中央控制函数：当某个对象被点击时调用
    public void OnObjectClicked(GameObject clickedObj)
    {
        // 找到当前点击的对象在列表中的索引
        int clickedIndex = toggleObjects.IndexOf(clickedObj);

        // 如果点击的对象不在列表内，直接无视
        if (clickedIndex == -1)
        {
            Debug.LogWarning($"点击的对象 {clickedObj.name} 不在 toggleObjects 列表中！");
            return;
        }

        // 如果点击的是【已经处于选中状态】的对象，你可以选择取消选中它
        if (clickedIndex == currentIndex)
        {
            SetEffectActive(currentIndex, false);
            currentIndex = -1; // 恢复到无选中状态
            Comment.text = "";
            return;
        }

        // 1. 如果之前有别的对象被选过了，先取消它的特效
        if (currentIndex != -1)
        {
            SetEffectActive(currentIndex, false);
        }

        // 2. 开启当前点击对象的特效
        SetEffectActive(clickedIndex, true);
        Comment.text = comments[clickedIndex];

        // 3. 更新当前选中索引
        currentIndex = clickedIndex;
    }

    // 封装一个专门开关物体身上“选中特效子物体”的辅助函数
    private void SetEffectActive(int index, bool isActive)
    {
        if (index < 0 || index >= toggleObjects.Count) return;

        GameObject parentObj = toggleObjects[index];
        if (parentObj != null)
        {
            // 💡 约定：特效子物体的名字叫做 "SelectEffect"
            Transform effectTrans = parentObj.transform.Find("SelectEffect");
            if (effectTrans != null)
            {
                effectTrans.gameObject.SetActive(isActive);
            }
            else
            {
                Debug.LogWarning($"{parentObj.name} 身上找不到名为 'SelectEffect' 的子物体！");
            }
        }
    }

    // 初始化选项
    public bool IniOptions()
    {
        var cardManager = CardManager.Instance;
        cards.Clear();
        cards = cardManager.GetRandomCard(round: curRound, barry: barry,
            energy: progress.Energy.Get());

        if (cards.Count == 0)
            return false;

        comments.Clear();
        for (int i = 0; i < 3; i++)
        {
            var card = cards[i];
            var obj = toggleObjects[i];

            comments.Add(card.bubbleText);

            Debug.Log($"Toggle.IniOptions循环中的card目前的ID为{i}");
            Debug.Log($"当前公式为{card.healthEffect}和{card.moodEffect}");

            obj.SetActive(true);
            Transform effectTrans = obj.transform.Find("SelectEffect");
            effectTrans.gameObject.SetActive(false);

            // 更新选项文本
            Transform titleTrans = obj.transform.Find("Title");
            Transform txtEffectTrans = obj.transform.Find("Effect");

            TextMeshProUGUI titleText = titleTrans.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI effectText = txtEffectTrans.GetComponent<TextMeshProUGUI>();

            titleText.text = card.actionName;
            effectText.text = card.EffectToString(progress.Health.Get());
        }

        foreach (GameObject obj in toggleObjects)
        {
            obj.SetActive(true);
            Transform effectTrans=obj.transform.Find("SelectEffect");
            effectTrans.gameObject.SetActive(false);
        }
        currentIndex = -1;
        Comment.text = "";  // 初始没有文本

        return true;
    }

    // 关闭选项
    public void CloseOptions()
    {
        Settle();

        foreach (GameObject obj in toggleObjects)
        {
            obj.SetActive(false);
        }
        Comment.text = "";  // 关闭时清空文本
        curRound++; // 准备进入下一回合
    }

    // 结算当前回合
    void Settle()
    {
        Transform effectTrans = toggleObjects[currentIndex].transform.Find("Effect");
        TextMeshProUGUI textMeshProUGUI = effectTrans.GetComponent<TextMeshProUGUI>();
        string text = textMeshProUGUI.text;
        var delta = NumberExtractor.GetThreeNumbers(text);
        progress.Energy.Change(delta[0]);
        progress.Health.Change(delta[1]);
        progress.Mood.Change(delta[2]);
    }

    public void SetBarry(int newBarry)
    {
        barry = newBarry;
    }
}