using System;
using TMPro;
using UnityEngine;

static public class Barry_Round
{
    public static int Barry; // 以1为开始
    public static int MaxBarry = 12;
    public static int Round; // 以0为开始

    [System.Serializable]
    public struct BarValue
    {
        public TextMeshProUGUI BarText;
        public float Ini;
        public float Max;
        private float cur;
        public string Addition;

        public void Initialize(TextMeshProUGUI text)
        {
            BarText = text;
            cur = Ini;
            apply();
        }
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

    public static BarValue Health;
    public static BarValue Mood;
    public static BarValue Energy;

    public static void Ini(TextMeshProUGUI health,TextMeshProUGUI mood,TextMeshProUGUI energy)
    {
        Barry = 1;
        Round = 0;

        Health.Ini = 40;
        Health.Max = 100;
        Health.Initialize(health);

        Mood.Ini = 50;
        Mood.Max = 100;
        Mood.Initialize(mood);

        Energy.Ini = 15;
        Energy.Max = 15;
        Energy.Initialize(energy);
    }

    public static void Ini()
    {
        Barry = 1;
        Round = 0;
        Health.Initialize();
        Mood.Initialize();
        Energy.Initialize();
    }

    public static void NextBarry()
    {
        Barry++;
        Round = 0;
    }

    public static bool IfFinishBarries()
    {
        // 使用此判断时要注意是当局，还是下一局
        return Barry > MaxBarry;
    }

    public static void NextRound()
    {
        Round++;
    }
}