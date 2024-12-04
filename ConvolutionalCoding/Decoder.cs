using System.Collections;

namespace ConvolutionalCoding;

public class Decoder(IEnumerable<int> polynoms) : ViterbiBase(polynoms)
{
    public bool[] Decode(IEnumerable<bool> encodedMessage)
    {
        var encodedBits = encodedMessage.ToList();
        if (encodedBits.Count % CoderBitness != 0)
            throw new ArgumentException($"Число бит в сообщении ({encodedBits.Count}) не согласуется с параметрами декодера.");

        var paths = new PathHolder(StatesCount);
        for (var symbol = 0; symbol < encodedBits.Count; symbol += CoderBitness)
        {
            var encoderOutput = new bool[CoderBitness];
            for (var bit = 0; bit < CoderBitness; bit++)
                encoderOutput[bit] = encodedBits[symbol + bit];
            UpdatePaths(encoderOutput, paths);
        }   

        var decodedMessage = ChooseFinalMessage(paths);
        return decodedMessage;
    }

    public bool[] Decode(BitArray encodedMessage)
    {
        var bools = encodedMessage.Cast<bool>().ToArray();
        
        return Decode(bools);
    }

    public List<bool> Decode(string binaryString)
    {
        var resBits = new List<bool>();
        for (var i = 0; i < binaryString.Length; i += CoderBitness)
        {
            var symbol = new bool[CoderBitness];
            for (var j = 0; j < CoderBitness; j++)
                symbol[j] = Convert.ToInt32(binaryString[i + j]) > 0;
            
            var decoded = Decode(symbol);
            resBits.AddRange(decoded);
        }
        return resBits;
    }

    public void Decode(TextReader streamToDecode, BinaryWriter decodedOutputStream)
    {
        var byteSymbolLength = CoderBitness * 8;
        var ar = new byte[1];
        while (streamToDecode.Peek() > 0)
        {
            var encodedByte = new bool[byteSymbolLength];
            for (var i = 0; i < byteSymbolLength; i++)
            {
                var ch = streamToDecode.Read();
                encodedByte[i] = ch > 0;
            }

            var decodedByte = Decode(encodedByte);
            var ba = new BitArray(decodedByte);
            ba.CopyTo(ar, 0); 
            var b = ar[0];
            decodedOutputStream.Write(b);
        }
    }
    
    private static bool[] ChooseFinalMessage(PathHolder encoderPaths)
    {
        var paths = encoderPaths.Paths
            .Where(p => p.SurvivorPath.Count > 0)
            .OrderBy(p => p.Metrics);

        foreach (var path in paths)
        {
            var decoded = new bool[path.Count];
            for (var i = 0; i < path.Count; i++)
            {
                decoded[i] = path.SurvivorPath[i].DestinationNodeMsb;
            }
            return decoded;
        }
        return [];
    }

    private void UpdatePaths(IEnumerable<bool> encoderOutput, PathHolder paths)
    {
        var currentPaths = CalculatePathMetrics(encoderOutput, paths);

        var survivedPaths = SelectSurvivedPaths(currentPaths);
        paths.AddSurvivorPath(survivedPaths);
    }
    
    private Transition[] CalculatePathMetrics(IEnumerable<bool> encoderOutput, PathHolder paths)
    {
        var transitionsWithMetrics = new Transition[StatesCount];
        for (var i = 0; i < StatesCount; i++)
        {
            var transCount = Transitions[i].Count();
            for (var j = 0; j < transCount; j++)
            {
                var trans = Transitions[i].ElementAt(j);
                var branchMetric = CalculateHammingDistance(trans.CoderOutBits, encoderOutput);
                var pathMetric = paths.GetPathMetricByNode(trans.SourceNode);
                var t = trans.Clone();
                t.Metrics = branchMetric + pathMetric;
                transitionsWithMetrics[i * transCount + j] = t;
            }
        }
        
        return transitionsWithMetrics;
    }

    private Transition[] SelectSurvivedPaths(Transition[] currentPaths)
    {
        var survivors = new Transition[TransitionsCount];
        for (var i = 0; i < TransitionsCount; i++)
        {
            var prevDestNode = i;
            var sameDestPaths = currentPaths
                .Where(t => t.DestinationNode == prevDestNode)
                .ToArray();
            var minMetricTransition = sameDestPaths.First();
            foreach (var t in sameDestPaths)
                if (t.Metrics < minMetricTransition.Metrics)
                    minMetricTransition = t;
            survivors[i] = minMetricTransition;
        }
        
        return survivors;
    }
    
    private int CalculateHammingDistance(params IEnumerable<bool>[] bitCombination)
    {
        var sumOfOnes = 0;
        for (var bit = 0; bit < bitCombination[0].Count(); bit++)
        {
            var resultBit = bitCombination
                .Aggregate(false, (current, t) => current ^ t.ElementAt(bit));
            if (resultBit)
                sumOfOnes++;
        }
        
        return sumOfOnes;
    }
}