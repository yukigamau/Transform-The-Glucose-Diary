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
        public int Ini;
        public int Max;
        private int cur;
        public string Addition;

        public void Initialize()
        {
            cur = Ini;
            apply();
        }
        public int Get() => cur;
        public int Change(int delta)
        {
            cur = Mathf.Clamp(cur + delta, 0, Max);
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
