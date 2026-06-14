#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CardImporter
{
    [MenuItem("工具/一键导入新版CSV卡牌")]
    public static void ImportNewCSV()
    {
        // 1. 获取在 Project 窗口选中的 CSV 文件路径
        string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);

        if (string.IsNullOrEmpty(assetPath) || !assetPath.EndsWith(".csv"))
        {
            EditorUtility.DisplayDialog("提示", "请先在 Project 窗口选中你的 [CardData.csv] 文件！", "知道了");
            return;
        }

        // 2. 按行读取 CSV 内容
        string[] lines = File.ReadAllLines(assetPath);
        if (lines.Length <= 1)
        {
            Debug.LogError("CSV 文件内容为空或只有表头！");
            return;
        }

        // 创建或确认保存 ScriptableObject 的目标文件夹
        string targetFolder = "Assets/Resources/Cards";
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        int successCount = 0;

        // 3. 遍历数据行（从第 1 行开始，跳过第 0 行表头）
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            // 💡 严格通过 Tab (\t) 切分列
            string[] rawColumns = line.Split('\t');

            string[] columns = new string[16];
            for (int c = 0; c < 16; c++)
            {
                if (c < rawColumns.Length)
                    columns[c] = rawColumns[c];
                else
                    columns[c] = ""; // 超出范围的直接当作空文本处理
            }

            // 4. 创建 CardData 资产实例并赋值
            CardData card = ScriptableObject.CreateInstance<CardData>();

            int index = 0;

            // 【基本信息】 (第 0 - 3 列)
            card.actionName = columns[index++].Trim();
            card.level = columns[index++].Trim();
            card.actionType = columns[index++].Trim();
            card.actionScene = columns[index++].Trim();

            // 如果连名字都没有，说明是表格底部的纯空行，过滤掉
            if (string.IsNullOrEmpty(card.actionName)) continue;

            // 【数值影响 - 消耗与主要属性】 (第 4 - 7 列)
            int.TryParse(columns[index++].Trim(), out int energy);
            card.energyCost = energy;

            card.healthEffect = columns[index++].Trim();
            card.moodEffect = columns[index++].Trim();
            card.cardDescription = columns[index++].Trim(); // 对应表格中的“卡面描述”列

            // 【数值影响 - 专长文本描述】 (第 8 - 10 列)
            card.marathon = ParseStringSafe(columns[index++]);
            card.music = ParseStringSafe(columns[index++]);
            card.cook = ParseStringSafe(columns[index++]);

            // 顺延一列“结局导向”数据 (第 11 列)，资产类里没定义，这里直接略过
            index++;

            // 【文本与描述 - 气泡与知识卡片】 (第 12 - 13 列)
            card.bubbleText = columns[index++].Trim();
            card.knowledgeText = columns[index++].Trim();

            // 【限制 - 固定与仅出现时机】 (第 14 - 15 列)
            card.stable = ParseSpaceSeparatedList(columns[index++]);
            card.only = ParseSpaceSeparatedList(columns[index++]);

            // 5. 强行覆盖老数据逻辑
            string safeName = card.actionName.Replace("/", "_").Replace("\\", "_").Replace(" ", "");
            if (string.IsNullOrEmpty(safeName)) continue;

            string savePath = $"{targetFolder}/{safeName}.asset";

            // 如果这个路径下已经有同名的老资产文件了，先强制把它干掉
            if (AssetDatabase.LoadAssetAtPath<CardData>(savePath) != null)
            {
                AssetDatabase.DeleteAsset(savePath);
            }

            // 重新创建全新的资产
            AssetDatabase.CreateAsset(card, savePath);
            successCount++;
        }

        // 6. 刷新 Unity 数据库
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("成功", $"🎉 完美导入！成功【强行覆盖】并生成了 {successCount} 张卡牌数据！\n无视了末尾空白列的残缺，全量数据已同步。", "太棒了");
    }

    // 辅助工具：安全清洗字符串（如果留空或写了"无"，则自动统一返回默认的"0"字符串）
    private static string ParseStringSafe(string value)
    {
        value = value.Trim();
        if (string.IsNullOrEmpty(value) || value == "无") return "0";
        return value;
    }

    // 辅助工具：专门解析由【空格】隔开的纯净时机列表（例如 "1 6" 或单数字 "1" 或者是空的）
    private static List<int> ParseSpaceSeparatedList(string value)
    {
        List<int> resultList = new List<int>();
        value = value.Trim();

        if (string.IsNullOrEmpty(value) || value == "无") return resultList;

        // 根据新表特征，通过空格切分出多个数字
        string[] elements = value.Split(' ');

        foreach (var element in elements)
        {
            if (string.IsNullOrWhiteSpace(element)) continue;
            if (int.TryParse(element.Trim(), out int number))
            {
                resultList.Add(number);
            }
        }

        return resultList;
    }
}
#endif