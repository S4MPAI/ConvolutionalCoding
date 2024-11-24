namespace ConvolutionalCoding;

public static class Helpers
{
    public static int GetBitCount(int value)
    {
        return (int)Math.Log2(value) + 1;
    }
}