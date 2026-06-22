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
}
