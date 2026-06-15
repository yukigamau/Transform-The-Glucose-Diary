using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class NumberExtractor
{
    /// <summary>
    /// 从字符串中快速提取前3个数（支持 +5、-3 等正负号）
    /// </summary>
    public static List<float> GetThreeNumbers(string input)
    {
        List<float> results = new List<float>();

        if (string.IsNullOrEmpty(input)) return results;

        // 💡 支持正负号、整数、以及可选的小数点和后续数字
        string pattern = @"[+-]?\d+(?:\.\d+)?";

        // 获取所有匹配项
        MatchCollection matches = Regex.Matches(input, pattern);

        // 只要前 3 个
        for (int i = 0; i < Math.Min(3, matches.Count); i++)
        {
            if (float.TryParse(matches[i].Value, out float number))
            {
                results.Add(number);
            }
        }

        return results;
    }
}