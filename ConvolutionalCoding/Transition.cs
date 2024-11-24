namespace ConvolutionalCoding;

public class Transition(
    int registerBitness,
    int coderBitness,
    int sourceNode,
    int destinationNode,
    int coderOut)
{
    public int Metrics;
    public readonly int SourceNode = sourceNode;
    public readonly int DestinationNode = destinationNode;
    public IReadOnlyCollection<bool> CoderOutBits = GetCoderOutBits(coderBitness, coderOut);
    public bool DestinationNodeMsb = GetDestinationNodeMsb(destinationNode, registerBitness);
    private static bool[] GetCoderOutBits(int coderBitness, int coderOut)
    {
        var coderOutBits = new bool[coderBitness];
        for (var i = 0; i < coderOutBits.Length; i++)
        {
            var shift = coderBitness - i;
            coderOutBits[i] = coderOut >> shift == 1;
        }
        
        return coderOutBits;
    }
    private static bool GetDestinationNodeMsb(int destinationNode, int registerBitness)
    {
        var firstBit = destinationNode >> (registerBitness - 2);
        return firstBit > 0;
    }

    private readonly int _coderBitness = coderBitness;
    private readonly int _registerBitness = registerBitness;
    private readonly int _coderOut = coderOut;
    
    public Transition Clone()
    {
        var cloned = (Transition)MemberwiseClone();
        cloned.Metrics = Metrics;
        return cloned;
    }
}