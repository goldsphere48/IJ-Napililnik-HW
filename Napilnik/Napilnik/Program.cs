static class Program
{
    public static int Clamp(int a, int b, int c)
    {
        if (a < b)
            return b;
        else if (a > c)
            return c;
        else
            return a;
    }

    public static void Main()
	{

	}
}