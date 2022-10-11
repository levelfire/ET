using Unity.Mathematics;

public static class MethHelper
{
    //public static bool CheckBuff(ref BuffComponent comBuff, int buffId)
    //{
    //    if (comBuff != null)
    //    {
    //        return comBuff.ContainsBuff(buffId);
    //    }
    //    return false;
    //}

    public static int getGridKey(int row, int col)
    {
        return (row << 4) | col;
    }

    public static (int, int) getGirdRC(int key)
    {
        int row = key >> 4;
        int col = key & 0b1111;
        return (row, col);
    }

    public static void SubCompareZero(ref int val, int sub)
    {
        val = sub > val ? 0 : val - sub;
    }

    public static (int x, int y) GetPosXY(int pos)
    {
        int x = pos & 0xff;
        int y = (pos & 0xff00) >> 8;
        return (x, y);
    }

    public static int SetPos(int x, int y)
    {
        return math.abs(x) | (math.abs(y) << 8);
    }
}