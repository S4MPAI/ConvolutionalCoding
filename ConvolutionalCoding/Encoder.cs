using System.Collections;
using System.Net.Mime;
using System.Text;

namespace ConvolutionalCoding;

public class Encoder(IEnumerable<int> polynoms) : ViterbiBase(polynoms)
{
    public List<bool> Encode(BitArray message)
    {
        var encodedMessage = new List<bool>();
        var currentNode = 0;

        for (int i = 0; i < message.Count; i++)
        {
            var nextTransition = GetNextTransition(message[i], Transitions[currentNode]);
            currentNode = nextTransition.DestinationNode;
            encodedMessage.AddRange(nextTransition.CoderOutBits);
        }

        return encodedMessage;
    }

    public List<bool> Encode(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        var ba = new BitArray(messageBytes);
        var encoded = Encode(ba);

        return encoded;
    }

    public void Encode(BinaryReader streamToEncode, StreamWriter encodedOutputStream)
    {
        var b = streamToEncode.ReadByte();
        var ba = new BitArray(b);
        var encodedBits = Encode(ba);
        foreach (var bit in encodedBits)
        {
            encodedOutputStream.Write((bit ? 1 : 0) & 1);
        }
    }

    public List<bool> Encode(IEnumerable<bool> message)
    {
        var b = new BitArray(message.ToArray());
        
        return Encode(b);
    }
}