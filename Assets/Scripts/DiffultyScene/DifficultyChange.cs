using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyChange : MonoBehaviour
{
    public int difficultyChangeStep = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClick()
    {
        Difficulty.Change(difficultyChangeStep);
    }
}
