using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class FormulaCalculator
{
    /// <summary>
    /// 强力公式解析与计算方法
    /// 支持：中文变量名、MIN/MAX函数、负数开头表达式
    /// 修复：Unicode规范化解决来自TSV/Excel配置表的"同字不同码"问题
    /// </summary>
    public static float EvaluateFormula(string formula, string varName, float varValue)
    {
        if (string.IsNullOrEmpty(formula)) return 0f;

        try
        {
            // 1. 清除所有空白字符（空格、全角空格、Tab等）
            formula = Regex.Replace(formula, @"\s+", "");
            varName = Regex.Replace(varName, @"\s+", "");

            // 2. Unicode NFKC 规范化
            //    消除全角/半角、不同编码方式造成的"看起来一样但字节不同"问题
            //    对处理来自 Excel/CSV/TSV 配置表的中文字符串至关重要
            formula = formula.Normalize(NormalizationForm.FormKC);
            varName = varName.Normalize(NormalizationForm.FormKC);

            // 3. 将 MIN/MAX 转换为 DataTable 支持的 IIF 条件表达式
            formula = ConvertMinMaxToIif(formula);

            // 4. 替换变量名为具体数值
            string valueStr = varValue.ToString(CultureInfo.InvariantCulture);
            formula = formula.Replace(varName, valueStr);

            // 5. 替换后检查变量是否还留在公式里（说明替换彻底失败）
            if (formula.Contains(varName))
            {
                Debug.LogError(
                    $"🚨 变量替换失败！varName字节: [{ByteDump(varName)}]\n" +
                    $"公式内容字节: [{ByteDump(formula)}]\n" +
                    $"请检查配置表中该变量名的字符编码是否与传入varName一致。"
                );
                return 0f;
            }

            // 6. 用 DataTable.Compute 计算最终结果
            DataTable dt = new DataTable();
            object result = dt.Compute(formula, "");

            return Convert.ToSingle(result);
        }
        catch (Exception e)
        {
            Debug.LogError($"🚨 公式 [{formula}] 最终解析失败! 错误信息: {e.Message}");
            return 0f;
        }
    }

    /// <summary>
    /// 将 MIN(a,b) / MAX(a,b) 递归转换为 DataTable 支持的 IIF 表达式
    /// </summary>
    private static string ConvertMinMaxToIif(string expression)
    {
        string minPattern = @"MIN\(([^,)]+),([^,)]+)\)";
        string maxPattern = @"MAX\(([^,)]+),([^,)]+)\)";

        // 循环替换以支持嵌套 MIN/MAX
        while (Regex.IsMatch(expression, minPattern, RegexOptions.IgnoreCase))
        {
            expression = Regex.Replace(expression, minPattern, match =>
            {
                string a = WrapNegative(match.Groups[1].Value);
                string b = WrapNegative(match.Groups[2].Value);
                return $"IIF(({a})<({b}),({a}),({b}))";
            }, RegexOptions.IgnoreCase);
        }

        while (Regex.IsMatch(expression, maxPattern, RegexOptions.IgnoreCase))
        {
            expression = Regex.Replace(expression, maxPattern, match =>
            {
                string a = WrapNegative(match.Groups[1].Value);
                string b = WrapNegative(match.Groups[2].Value);
                return $"IIF(({a})>({b}),({a}),({b}))";
            }, RegexOptions.IgnoreCase);
        }

        return expression;
    }

    /// <summary>
    /// DataTable.Compute 无法识别括号内直接以负号开头的表达式（如 (-2+x) 会报错）
    /// 在前面补 0 变成 (0-2+x) 即可绕过此限制
    /// </summary>
    private static string WrapNegative(string expr)
    {
        if (expr.StartsWith("-"))
            return "0" + expr;
        return expr;
    }

    /// <summary>
    /// 调试工具：打印字符串中每个字符的 Unicode 码点
    /// 用于排查"看起来相同但替换失败"的隐藏字符问题
    /// </summary>
    private static string ByteDump(string s)
    {
        if (s == null) return "null";
        var sb = new StringBuilder();
        foreach (char c in s)
            sb.Append($"U+{(int)c:X4}({c}) ");
        return sb.ToString().TrimEnd();
    }
}