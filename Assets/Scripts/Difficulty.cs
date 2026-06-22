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
        // MIN(健康值*(0.03+ 0.002*进阶等级 , 1 + 进阶等级* 0.1)
        float health = Barry_Round.Health.Get();
        float delta = Mathf.Min(health * 0.03f + 0.002f * Cur, 1f + Cur * 0.1f);
        return -delta;
    }

    public static float MoodDelta()
    {
        // MAX(2+进阶等级*0.1 - (心情值 - 40 - 进阶等级)/(30+ 进阶等级) , 2+进阶等级*0.1)
        float mood = Barry_Round.Mood.Get();
        float delta = Mathf.Max(2 + Cur * 0.1f - (mood - 40 - Cur) / (30 + Cur), 2 + Cur * 0.1f);
        return -delta;
    }
}
