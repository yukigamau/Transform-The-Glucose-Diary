using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverCheck : MonoBehaviour
{
    public static GameOverCheck Instance;

    [System.Serializable]
    public struct Condition
    {
        public int Cnt; // 次数
        public int Level;   // 达成条件
        public string Name; // 名字
        public string Event;    // 事件
        public string Achieve;  // 成就
        public bool Got;    // 是否获得
        public string Comment;  // 评论

        public bool Check(string EventName)
        {
            if (EventName == Event)
            {
                Cnt++;
                if (Cnt >= Level)
                {
                    Got = true;
                    return true;
                }
            }

            return false;
        }
    }

    // 在第10天结束后才能触发的结果，有比较麻烦的处理
    public Condition LongRuner;
    public Condition Cooker;
    public Condition Guitar;
    [Header("动态处理")]
    public List<Condition> conditions;

    [System.Serializable]
    public struct Attribute
    {
        public int HealthCondition;
        public int MoodCondition;
        public string Name;
        [TextArea(3, 5)] public string Epilogue;

        // 是否达成
        public bool Comein(int health, int mood)
        {
            if (health < HealthCondition && mood < MoodCondition)
                return true;
            else
                return false;
        }

        public string GetOverTitle()
        {
            return Name + "\n" + Epilogue;
        }
    }

    public Attribute EatMuch;
    public Attribute Groomy;
    // 用于根据属性判断普通的结果，得不到特殊结果，但是有提前结束的能力
    private List<Attribute> attributes; 

    [System.Serializable]
    public struct Final
    {
        public int MinHealth;
        public int MaxHealth;
        public int MinMood;
        public int MaxMood;
        public string Name;

        public bool Comein(int health, int mood)
        {
            if (health > MinHealth && health <= MaxHealth
                && mood > MinMood && mood <= MaxMood)
                return true;
            else
                return false;
        }

        public string GetOverTitle()
        {
            string s = $"{Name}\n";
            s += $"你的健康值≥{MinHealth}并≤{MaxHealth}\n";
            s += $"你的心情值≥{MinMood}并≤{MaxMood}\n";
            return s;
        }
    }

    public Final Normal;
    public Final HelloWorld;
    private List<Final> fianls;

    public string OverTitle;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        conditions = new List<Condition>();
        conditions.Add(LongRuner);
        conditions.Add(Cooker);
        conditions.Add(Guitar);

        attributes = new List<Attribute>();
        attributes.Add(EatMuch);
        attributes.Add(Groomy);

        fianls = new List<Final>();
        fianls.Add(Normal);
        fianls.Add(HelloWorld);

        OverTitle = "";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool Chosen(string title)
    {
        for(int i = 0; i< conditions.Count; i++)
        {
            // 达成特殊结局
            if (conditions[i].Check(title) && conditions[i].Got)
            {
                OverTitle = conditions[i].Achieve + "\n";
                OverTitle += $"你完成了{conditions[i].Level}次{conditions[i].Event}\n";
                OverTitle += $"达成了{conditions[i].Name}，并完成了{title}";

                return true;
            }
        }

        return false;
    }

    // 是否结束
    public bool IfOver(int health, int mood)
    {
        // 至少要完成10局
        // 在判定IfOver时，已经完成了Barry的增加，所以Barry是下一天的
        if (Barry_Round.Barry <= 10)
            return false;

        if (OverTitle != "")
            return true;

        // 基于数值的普通结果
        for (int i = 0; i < attributes.Count; i++)
        {
            if (attributes[i].Comein(health, mood))
            {
                OverTitle = attributes[i].GetOverTitle();
                return true;
            }
        }

        return false;
    }

    // 如果没有OverTitle，则寻找常规结果
    public void AdjustOverTitle(int health, int mood)
    {
        if (OverTitle == "")
            foreach (Final f in fianls)
                if (f.Comein(health, mood))
                    OverTitle = f.GetOverTitle();
    }

    public string GetSpecial()
    {
        /// OverTitle的第一行就是称号

        // 寻找第一个换行符的位置（同时兼容 \n 和 \r）
        int index = OverTitle.IndexOf('\n');

        string firstLine;
        if (index >= 0)
        {
            // 截取从 0 开始到换行符之前的字符串
            firstLine = OverTitle.Substring(0, index);
        }
        else
        {
            // 如果找不到换行符，说明整段文字本身就只有一行
            firstLine = OverTitle;
        }

        // 💡 额外安全处理：去除 Windows 换行符残留下来的 \r 
        firstLine = firstLine.TrimEnd('\r');

        return firstLine;
    }

    // 获取最终的称号
    public string GetSpecial_Destory()
    {
        string firstLine = GetSpecial();
        Destroy(gameObject);

        return firstLine;
    }
}
