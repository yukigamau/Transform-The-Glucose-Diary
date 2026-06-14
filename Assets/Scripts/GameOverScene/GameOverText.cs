using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverText : MonoBehaviour
{
    private int finalHealth;
    private int finalMood;
    private string finalSpecial;

    public TextMeshProUGUI textMeshProUGUI;

    // Start is called before the first frame update
    void Start()
    {
        ReadFinalData();
        textMeshProUGUI.text = finalSpecial;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ReadFinalData()
    {
        ReadFinalHealth_Mood();
        ReadFinalSpecial();
    }

    void ReadFinalHealth_Mood()
    {
        // 1.通过类型主动去新场景里寻找那个被保留下来的组件
        ProgressBar dataHolder = FindObjectOfType<ProgressBar>();

        if (dataHolder != null)
        {
            // 2. 读取里面存储的东西
            finalHealth = dataHolder.Health.Get();
            finalMood = dataHolder.Mood.Get();

            // 3. 核心：读取完了，现在可以安全地把它删掉了
            Destroy(dataHolder.gameObject);
            Debug.Log("缓存对象已彻底从内存中销毁。");
        }
    }

    void ReadFinalSpecial()
    {
        GameOverCheck dataHolder = FindObjectOfType<GameOverCheck>();

        if(dataHolder != null)
        {
            finalSpecial = dataHolder.GetSpecial_Destory();
        }
    }
}
