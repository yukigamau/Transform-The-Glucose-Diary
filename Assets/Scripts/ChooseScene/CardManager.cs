using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
            UnityEngine.Debug.Log($"🎉 成功动态加载了 {loadedCards.Length} 张卡牌资产！");
        }
        else
        {
            UnityEngine.Debug.LogError("🚨 无法加载卡牌资产！请检查 Assets/Resources/Cards 目录。");
            return;
        }

        GetStableRounds();
        GetOnlyBarries();
        GetNormalRound();
    }

    public bool IfRoundStable(int round) => stableRound_CardID.ContainsKey(round);
    public bool IfBarryOnly(int barry) => onlyBarry_CardID.ContainsKey(barry);

    // 检查卡牌的前置条件
    bool CheckCondition(CardData cardData)
    {
        if (cardData.condition == "" || cardData.condition == "0")
            return true;

        foreach(GameOverCheck.Condition condition in GameOverCheck.Instance.conditions)
        {
            if (condition.Name == cardData.condition && condition.Got)
            {
                UnityEngine.Debug.Log($"{cardData.name}可以使用");
                return true;
            }
        }

        UnityEngine.Debug.Log($"{cardData.name}不可使用");
        return false;
    }

    /// <summary>
    /// 获取指定回合的随机卡牌列表（共3张，且精力消耗在 energy 之内，且必须满足前置条件，不用去重）
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

        if (IfBarryOnly(barry))
        {
            rawPool.AddRange(onlyBarry_CardID[barry]);
        }

        // 2. 【核心修改】双重过滤：同时筛选 精力消耗 与 前置条件
        List<int> filteredPool = new List<int>();
        foreach (int cardId in rawPool)
        {
            CardData card = cardDataList[cardId];

            // 检查一：精力是否足够 (绝对值 <= 当前可用精力)
            int cost = Mathf.Abs(card.energyCost);
            bool isEnergyEnough = cost <= energy;

            // 检查二：前置条件是否满足
            bool isConditionMet = CheckCondition(card);

            // 只有两个条件同时满足，这张卡牌才算解锁并允许被抽到
            if (isEnergyEnough && isConditionMet)
            {
                filteredPool.Add(cardId);
            }
        }

        // 🛑 防御性兜底：如果过滤后的卡池为空，说明没有任何一张卡满足条件
        if (filteredPool.Count == 0)
        {
            UnityEngine.Debug.LogWarning($"礼貌警告 ⚠️: 天数 {barry} | 回合 {round} | 精力 {energy} 时，" +
                                         $"过滤出的合法卡池为空！(可能是精力耗尽或前置条件把卡滤光了)");

            // 💡 脱困策略：如果完全没有合法卡牌，直接返回一个空列表，
            // 这样你的上层控制器（如 Toggle）收到空列表后，就能立刻识别并触发“过天(TurnToNextDay)”或“结算(FianlGame)”
            return result;
        }

        // 3. 随机抽取 3 张卡牌（不用去重，纯随机）
        int cardsToDraw = 3;

        for (int i = 0; i < cardsToDraw; i++)
        {
            // 从完全合法的卡池里随机选一个
            int randomIndex = UnityEngine.Random.Range(0, filteredPool.Count);
            int selectedCardId = filteredPool[randomIndex];

            result.Add(cardDataList[selectedCardId]);

            // 由于你明确要求不用去重，这里不需要 Remove，同一张牌可能重复出现
        }

        return result;
    }
}