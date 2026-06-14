using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onClick()
    {
#if UNITY_EDITOR
        // 在Unity编辑器中停止运行（仅用于测试）
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // 在打包后的游戏中退出
            Application.Quit();
#endif
    }
}
