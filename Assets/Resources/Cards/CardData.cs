using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCardData", menuName = "游戏配置/卡牌数据")]
public class CardData : ScriptableObject
{
    [Header("基本信息")]
    public string actionName;         // 行动名称
    public string level;              // 卡牌等级
    public string actionType;         // 行动类型
    public string actionScene;        // 行动场景

    [Header("数值影响")]
    public int energyCost;            // 精力值
    public string healthEffect;       // 健康值影响 (CSV里带公式或加减号，用string存最稳妥)
    public string moodEffect;         // 心情值影响 (同上，方便后续解析)
    public string condition;          // 前置条件

    [Header("文本与描述")]
    [TextArea(3, 5)] public string cardDescription; // 卡面描述
    [TextArea(2, 4)] public string bubbleText;      // 文本气泡
    [TextArea(3, 5)] public string knowledgeText;   // 知识卡片内容

    [Header("限制")]
    public List<int> stable; // 固定时机
    public List<int> only;   // 仅出现时机

    public int GetRarity()
    {
        return level switch
        {
            "白" => 10,
            "蓝" => 4,
            "金" => 1,
            _ => 0
        };
    }

    public string EffectToString(int healthValue)
    {
        string effectString;
        if (cardDescription == "normal")
            effectString = $"精力值{energyCost:+#;-#;0}\n";
        else
            effectString = $"精力值{-10}（结束今天）\n";

        int healthCost = (int)FormulaCalculator.EvaluateFormula(healthEffect, "健康值", healthValue);
        effectString += $"健康值{healthCost:+#;-#;0}\n";

        int moodCost = (int)FormulaCalculator.EvaluateFormula(moodEffect, "健康值", healthValue);
        effectString += $"心情值{moodCost:+#;-#;0}\n";

        return effectString;
    }
}