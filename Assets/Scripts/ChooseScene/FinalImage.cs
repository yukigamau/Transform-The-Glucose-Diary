using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinalImage : MonoBehaviour
{
    public Image image;

    void Start()
    {

    }

    void Update()
    {

    }

    public void SetImage(string endTitle)
    {
        if (image == null)
        {
            Debug.LogError("Image 组件未绑定！");
            return;
        }

        // 1. 构建路径并从 Resources 加载图片
        string photoPath = $"Images/结局{endTitle}";
        Sprite sprite = Resources.Load<Sprite>(photoPath);

        if (sprite == null)
        {
            Debug.LogError($"未能成功加载结算图片，路径: {photoPath}");
            return;
        }

        // 2. 将加载的图片赋给 Image 组件
        image.sprite = sprite;

        // 3. 获取或添加 AspectRatioFitter 组件
        AspectRatioFitter aspectFitter = image.GetComponent<AspectRatioFitter>();
        if (aspectFitter == null)
        {
            aspectFitter = image.gameObject.AddComponent<AspectRatioFitter>();
        }

        // 4. 设置适配模式
        aspectFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;

        // 5. 计算并设置图片的原生宽高比 ( 宽 / 高 )
        float spriteWidth = sprite.rect.width;
        float spriteHeight = sprite.rect.height;
        aspectFitter.aspectRatio = spriteWidth / spriteHeight;

        // 6. 确保关闭 preserveAspect
        image.preserveAspect = false;
    }
}