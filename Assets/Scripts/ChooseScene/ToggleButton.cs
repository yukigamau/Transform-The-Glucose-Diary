using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class ToggleButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        // 1. 当这个物体被点击时，直接去寻找之前重命名为 Toggle 的那个全局单例
        // 2. 将当前游戏对象 (gameObject) 作为参数传进去，告诉它我被点击了
        if (Toggle.Instance != null)
        {
            Toggle.Instance.OnObjectClicked(gameObject);
        }
        else
        {
            Debug.LogError("场景中找不到名为 Toggle 的单例控制器！请检查旧脚本是否成功创建了 Instance 变量。");
        }
    }
}
