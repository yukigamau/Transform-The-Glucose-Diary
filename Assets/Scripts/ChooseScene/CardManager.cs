using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [Header("所有从配置表加载进来的卡牌资产")]
    // 修改为普通的 List，既然是单例，建议通过 Instance.cardDataList 访问，非必要不全部用 static
    public List<CardData> cardDataList = new List<CardData>();

    // 初始化所有容器，防止 NullReferenceException
    private Dictionary<int, List<int>> stableRound_CardID = new Dictionary<int, List<int>>();
    private Dictionary<int, List<int>> onlyBarry_CardID = new Dictionary<int, List<int>>();
    private List<int> normalCard = new List<int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 确保全局唯一且跨场景不销毁（视需求而定）
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadAllCardAssets();
    }

    // 获取存在稳定卡牌的回合
    private void GetStableRounds()
    {
        for (int i = 0; i < cardDataList.Count; i++)
        {
            if (cardDataList[i].stable == null || cardDataList[i].stable.Count == 0)
                continue;

            foreach (var round in cardDataList[i].stable)
            {
                if (!stableRound_CardID.ContainsKey(round))
                    stableRound_CardID[round] = new List<int>();

                // 根据稀有度（权重）加入队列
                int rarity = Mathf.Max(1, cardDataList[i].GetRarity()); // 兜底防止稀有度配错成0导致死循环
                for (int j = 0; j < rarity; j++)
                {
                    stableRound_CardID[round].Add(i);
                }
            }
        }
    }

    // 获取仅在特定回合出现的卡牌
    private void GetOnlyBarries()
    {
        for (int i = 0; i < cardDataList.Count; i++)
        {
            if (cardDataList[i].only == null || cardDataList[i].only.Count == 0)
                continue;

            foreach (var round in cardDataList[i].only)
            {
                if (!onlyBarry_CardID.ContainsKey(round))
                    onlyBarry_CardID[round] = new List<int>();

                int rarity = Mathf.Max(1, cardDataList[i].GetRarity());
                for (int j = 0; j < rarity; j++)
                {
                    onlyBarry_CardID[round].Add(i);
                }
            }
        }
    }

    // 获取普通卡牌
    private void GetNormalRound()
    {
        for (int i = 0; i < cardDataList.Count; i++)
        {
            // 既不是稳定，也不是仅出现，那就是普通卡
            if ((cardDataList[i].stable == null || cardDataList[i].stable.Count == 0) &&
                (cardDataList[i].only == null || cardDataList[i].only.Count == 0))
            {
                int rarity = Mathf.Max(1, cardDataList[i].GetRarity());
                for (int j = 0; j < rarity; j++)
                {
                    normalCard.Add(i);
                }
            }
        }
    }

    private void LoadAllCardAssets()
    {
        // 清空旧数据（清空前确保已实例化）
        cardDataList.Clear();
        stableRound_CardID.Clear();
        onlyBarry_CardID.Clear();
        normalCard.Clear();

        CardData[] loadedCards = Resources.LoadAll<CardData>("Cards");

        if (loadedCards != null && loadedCards.Length > 0)
        {
            cardDataList.AddRange(loadedCards);
            Debug.Log($"🎉 成功动态加载了 {loadedCards.Length} 张卡牌资产！");
        }
        else
        {
            Debug.LogError("🚨 无法加载卡牌资产！请检查 Assets/Resources/Cards 目录。");
            return;
        }

        GetStableRounds();
        GetOnlyBarries();
        GetNormalRound();
    }

    public bool IfRoundStable(int round) => stableRound_CardID.ContainsKey(round);
    public bool IfBarryOnly(int barry) => onlyBarry_CardID.ContainsKey(barry);

    /// <summary>
    /// 获取指定回合的随机卡牌列表（共3张，且精力消耗在 energy 之内，不用去重）
    /// </summary>
    public List<CardData> GetRandomCard(int round, int barry, int energy)
    {
        List<CardData> result = new List<CardData>();

        // 1. 构建当前回合的“卡池 ID 快照”
        List<int> rawPool = new List<int>();

        if (IfRoundStable(round))
        {
            rawPool.AddRange(stableRound_CardID[round]);
        }
        else
        {
            rawPool.AddRange(normalCard);
        }

        // 💡 纠个小错：这里传进来的参数是 barry，所以原先代码的 onlyBarry_CardID[round] 应该改成 [barry]
        if (IfBarryOnly(barry))
        {
            rawPool.AddRange(onlyBarry_CardID[barry]);
        }

        // 2. 【新增逻辑】精力过滤：从原始卡池中挑选出消耗在当前精力范围内的卡牌 ID 组成新卡池
        List<int> filteredPool = new List<int>();
        foreach (int cardId in rawPool)
        {
            // 如果卡牌的精力消耗（注意：由于配置表里带负号，如 -2、-4，所以是“绝对值 <= 当前精力”
            // 或者是直接判断 “卡牌消耗的绝对值 <= energy” 或者是 “-cardDataList[cardId].energyCost <= energy”）
            // 那么卡牌消耗的精力就是 Mathf.Abs(energyCost) 
            int cost = Mathf.Abs(cardDataList[cardId].energyCost);

            if (cost <= energy)
            {
                filteredPool.Add(cardId);
            }
        }

        // 防御性代码：如果过滤后的卡池为空，说明玩家当前精力不够用任何一张牌，直接返回空列表
        if (filteredPool.Count == 0)
        {
            Debug.LogWarning($"⚠️ 回合 {round} 精力为 {energy} 时，卡池中没有符合精力消耗条件的卡牌！");
            return result;
        }

        // 3. 随机抽取 3 张卡牌（不用去重，纯随机）
        int cardsToDraw = 3;

        for (int i = 0; i < cardsToDraw; i++)
        {
            // 从符合精力条件的卡池里随机选一个
            int randomIndex = Random.Range(0, filteredPool.Count);
            int selectedCardId = filteredPool[randomIndex];

            result.Add(cardDataList[selectedCardId]);

            // 💡 提示：因为你明确要求“不用去重”，所以这里【不需要】从 filteredPool 中 Remove
            // 这样同一张卡牌如果权重高，或者运气好，是有可能在 result 里出现 2 次甚至 3 次的。
        }

        return result;
    }
}