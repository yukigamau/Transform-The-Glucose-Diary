using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Difficulty
{
    public static int Cur = 0;
    public static int Min = 0;
    public static int Max = 10;
    
    public static void Ini()
    {
        Cur = Min;
    }

    public static void Change(int delta)
    {
        int cur = Cur;
        int changed = cur + delta;
        changed = Mathf.Max(changed, Min);
        changed = Mathf.Min(changed, Max);
        Cur = changed;
    }

    public static float HealthDelta()
    {
        // MIN(健康值*(0.01+ 0.002*进阶等级) , 0.2 + 进阶等级* 0.07)
        float health = Barry_Round.Health.Get();
        float delta = Mathf.Min(health * (0.01f + 0.0025f * Cur), 0.2f + Cur * 0.07f);
        return -delta;
    }

    public static float MoodDelta()
    {
        // MAX(0.3+进阶等级*0.07 - (心情值 - 35 - 进阶等级)/(40+ 进阶等级) , 0.3+进阶等级*0.07)
        float mood = Barry_Round.Mood.Get();
        float delta = Mathf.Max(0.3f + Cur * 0.07f - (mood - 35 - Cur) / (40 + Cur), 0.3f + Cur * 0.07f);
        return -delta;
    }
}
