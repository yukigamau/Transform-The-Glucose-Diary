using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class NumberExtractor
{
    /// <summary>
    /// 从字符串中快速提取前3个整数（支持 +5、-3 等正负号）
    /// </summary>
    public static List<int> GetThreeNumbers(string input)
    {
        List<int> results = new List<int>();

        if (string.IsNullOrEmpty(input)) return results;

        // 💡 [+-]? 表示可选的正负号，\d+ 表示连续的数字
        string pattern = @"[+-]?\d+";

        // 获取所有匹配项
        MatchCollection matches = Regex.Matches(input, pattern);

        // 只要前 3 个
        for (int i = 0; i < Math.Min(3, matches.Count); i++)
        {
            if (int.TryParse(matches[i].Value, out int number))
            {
                results.Add(number);
            }
        }

        return results;
    }
}