using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CurBarry : MonoBehaviour
{
    public TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        text.text = $"当前：第{Barry_Round.Barry}天";
    }
}
