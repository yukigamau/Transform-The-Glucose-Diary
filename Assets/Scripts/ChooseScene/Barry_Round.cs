static public class Barry_Round
{
    public static int Barry; // 以1为开始
    public static int MaxBarry = 3;
    public static int Round; // 以0为开始

    public static void Ini()
    {
        Barry = 1;
        Round = 0;
    }

    public static void NextBarry()
    {
        Barry++;
        Round = 0;
    }

    public static bool IfFinishBarries()
    {
        // 使用此判断时要注意是当局，还是下一局
        return Barry > MaxBarry;
    }

    public static void NextRound()
    {
        Round++;
    }
}