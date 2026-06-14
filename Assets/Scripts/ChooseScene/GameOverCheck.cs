using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverCheck : MonoBehaviour
{
    public static Toggle Instance;

    [System.Serializable]
    public struct Condition
    {
        public int Cnt; // 次数
        public int Level;   // 达成条件
        public string Name; // 名字
        public string Event;    // 事件
        public string Achieve;  // 成就
        public bool Got;    // 获得
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

    public Condition LongRuner;
    public Condition Cooker;
    public Condition Guitar;
    private List<Condition> conditions;

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
    }

    public Attribute EatMuch;
    public Attribute Groomy;
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
            if (health > MinHealth && health < MaxHealth
                && mood > MinMood && mood < MaxMood)
                return true;
            else
                return false;
        }
    }

    public Final Normal;
    public Final HelloWorld;

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
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Chosen(string title)
    {
        for(int i = 0; i< conditions.Count; i++)
        {
            if (conditions[i].Check(title))
                break;
        }
    }

    public bool IfOver(int health, int mood)
    {
        for(int i=0;i<attributes.Count;i++)
        {
            if (attributes[i].Comein(health, mood))
            {
                return true;
            }
        }

        return false;
    }
}
