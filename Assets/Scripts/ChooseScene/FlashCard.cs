using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FlashCard : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Image image;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Change(CardData card)
    {
        text.text = card.knowledgeText;
    }

    void ChangePhoto(CardData card)
    {
        // 获取当前图片节点上的 RectTransform 组件
        RectTransform rectTrans = image.GetComponent<RectTransform>();

        // 1. 获取宽高
        float width = rectTrans.rect.width;
        float height = rectTrans.rect.height;
        float max = Mathf.Max(width, height);

        // 2. 加载图片（确保你的 Images 文件夹在 Resources 文件夹内部）
        string photoName = $"Images/{card.actionScene}";
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
