using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [System.Serializable]
    public struct BarValue
    {
        public TextMeshProUGUI BarText;
        public float Ini;
        public float Max;
        private float cur;
        public string Addition;

        public void Initialize()
        {
            cur = Ini;
            apply();
        }
        public float Get() => cur;
        public float Change(float delta)
        {
            float rawCur = Mathf.Clamp(cur + delta, 0, Max);
            cur = (float)Math.Round(rawCur, 2, MidpointRounding.AwayFromZero);
            apply();
            return cur;
        }
        public void apply()
        {
            BarText.text = $"{Addition}{cur}/{Max}";
        }
    }

    public BarValue Health;
    public BarValue Mood;
    public BarValue Energy;

    // Start is called before the first frame update
    void Start()
    {
        Ini();

        // 游戏结束还要用
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Ini()
    {
        Health.Initialize();
        Mood.Initialize();
        Energy.Initialize();
    }
}
