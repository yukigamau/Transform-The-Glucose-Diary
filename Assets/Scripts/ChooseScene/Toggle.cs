using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    private List<List<float>> delta;

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

        Barry_Round.Ini();
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
        cards = cardManager.GetRandomCard(round: Barry_Round.Round, barry: Barry_Round.Barry,
            energy: progress.Energy.Get());

        if (cards.Count == 0)
        {
            Debug.Log($"没有可以抽取的卡。精力值：{progress.Energy.Get()}");
            return false;
        }

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

            // 改动卡片
            ChangeCard(obj, card);
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
        Barry_Round.NextRound(); // 准备进入下一回合
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

    void ChangeCard(GameObject obj, CardData card)
    {
        ChangeTextColor(obj, card);
        ChangeBackPhoto(obj, card);
    }

    // 修改卡片标题颜色
    void ChangeTextColor(GameObject obj, CardData card)
    {
        string name = "Title";
        Transform textTrans = obj.transform.Find(name);
        if (textTrans == null)
        {
            Debug.LogError($"{obj.name} 找不到名为 '{name}' 的子物体！");
            return;
        }

        TextMeshProUGUI text = textTrans.GetComponent<TextMeshProUGUI>();
        Color black = new Color(0f, 0f, 0f);
        text.color = card.level switch
        {
            "白" => black,
            "蓝" => new Color32(81, 163, 206, 255),
            "金" => new Color32(255, 208, 0, 255),
            _ => black
        };
    }

    // 修改图片背景
    void ChangeBackPhoto(GameObject obj, CardData card)
    {
        string panel = "Panel";
        string name = "Image";
        Transform imageTrans = obj.transform.Find(panel).transform.Find(name);
        if (imageTrans == null) return;

        Image image = imageTrans.GetComponent<Image>();
        // 获取当前图片节点上的 RectTransform 组件
        RectTransform rectTrans = image.GetComponent<RectTransform>();

        // 1. 获取传入的卡牌面板的 RectTransform 宽高
        RectTransform objRectTrans = toggleObjects[0].GetComponent<RectTransform>();
        float width = objRectTrans.rect.width;
        float height = objRectTrans.rect.height;
        float max = Mathf.Max(width, height);

        // 2. 加载图片（确保你的 Images 文件夹在 Resources 文件夹内部）
        string photoName = $"Images/卡片{card.actionType}（扣图）";
        Sprite sprite = Resources.Load<Sprite>(photoName);

        if (sprite == null)
        {
            Debug.LogError($"未能加载图片: {photoName}");
            return;
        }

        // 3. 更换图片
        image.sprite = sprite;

        // 4. 正确修改 UI 的长宽为 max (使用 sizeDelta 赋值)
        rectTrans.sizeDelta = new Vector2(max, max);

        // 💡 额外建议：由于你把长宽都强行设为了 max（变成了正方形），
        // 如果原图不是正方形，建议开启保持比例，防止图片被拉伸变形
        image.preserveAspect = true;
    }
}