using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Bubble : MonoBehaviour
{
    public TextMeshProUGUI bubbleText;
    public Image bubbleBorder;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (bubbleText.text == "")
            bubbleBorder.enabled = false;
        else
            bubbleBorder.enabled = true;
    }
}
