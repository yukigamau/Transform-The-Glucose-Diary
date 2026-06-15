using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverCheck : MonoBehaviour
{
    public static GameOverCheck Instance;

    [System.Serializable]
    public class Condition
    {
        public int Cnt; // 次数
        public int Level;   // 达成条件
        public string Name; // 名字
        public string Event;    // 事件
        public string Achieve;  // 成就
        public bool Got;    // 是否获得
        public string Comment;  // 评论

        public string Updater;  // 达成结局用的升级事件的名称
        public int UpLevel;  // 达成结局需要触发升级事件的次数
        public int CurCnt;   // 当前升级事件的触发次数
        public bool Finish;  // 是否完成升级事件对应的结局

        public bool Check(string EventName)
        {
            if (EventName == Event)
            {
                Cnt++;
                if (Cnt >= Level)
                {
                    Got = true;
                    Debug.Log($"{Achieve}已达成");
                    return true;
                }
            }

            return false;
        }

        public bool UpCheck(string EventName)
        {
            if(EventName == Updater)
            {
                CurCnt++;
                if (CurCnt >= UpLevel)
                    Finish = true;
            }

            return Finish;
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
        public float HealthCondition;
        public float MoodCondition;
        public string Name;
        [TextArea(3, 5)] public string Epilogue;

        // 是否达成
        public bool Comein(float health, float mood)
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
        public float MinHealth;
        public float MaxHealth;
        public float MinMood;
        public float MaxMood;
        public string Name;

        public bool Comein(float health, float mood)
        {
            if (health > MinHealth && health <= MaxHealth
                || mood > MinMood && mood <= MaxMood)
                return true;
            else
                return false;
        }

        public string GetOverTitle()
        {
            string s = $"{Name}\n";
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
        fianls.Add(HelloWorld); // HelloWorld权重在Normal之前
        fianls.Add(Normal);

        OverTitle = "";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 选卡后对结局的判断
    public bool Chosen(string title)
    {
        foreach (Condition condition in conditions)
        {
            condition.Check(title);
        }

        // 判断特殊结局
        foreach (Condition condition in conditions)
        {
            if(condition.UpCheck(title))
            {
                OverTitle = condition.Achieve + "\n";
                OverTitle += $"你完成了{condition.Level}次{condition.Event}\n";
                OverTitle += $"达成了{condition.Name}，并完成了{title}";
            }

        }

        return OverTitle != "";
    }

    // 是否结束
    public bool IfOver(float health, float mood)
    {
        // 基于数值的普通结果
        for (int i = 0; i < attributes.Count; i++)
        {
            if (attributes[i].Comein(health, mood))
            {
                OverTitle = attributes[i].GetOverTitle();
                return true;
            }
        }

        // 至少要完成10局
        // 在判定IfOver时，已经完成了Barry的增加，所以Barry是下一天的
        if (Barry_Round.Barry <= 10)
            return false;

        if (OverTitle != "")
            return true;

        return false;
    }

    // 如果没有OverTitle，则寻找常规结果
    public void AdjustOverTitle(float health, float mood)
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
