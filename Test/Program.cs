using System.Collections;
using System.Text;
using Decoder = ConvolutionalCoding.Decoder;
using Encoder = ConvolutionalCoding.Encoder;

var polys = new[]{13, 15};
var coder = new Encoder(polys);
var decoder = new Decoder(polys);
//var message = "HELLO WORLD";
var message = "Кодирование и шифрование";

var encoded = coder.Encode(message);
Console.WriteLine(encoded.Aggregate("", (a, b) => a + (b ? 1 : 0)));

while (true)
{
    Console.WriteLine("Введите процент ошибок:");
    var errorPercent = int.Parse(Console.ReadLine());
    var errorsIndexes = GetErrorsIndexes(encoded.Count, errorPercent);
    var errEncoded = GetMessageWithErrors(encoded, errorsIndexes);
    WriteToConsoleWithErrors(errEncoded, errorsIndexes);
    
    var restored = decoder.Decode(errEncoded);
    Console.WriteLine(ToUtf8String(restored));
}

return;

HashSet<int> GetErrorsIndexes(int messageLength, double errorsPercent)
{
    var numErrors = Math.Truncate(messageLength * errorsPercent / 100);
    var errorsIndexes = new HashSet<int>();
    var random = new Random();
    
    for (var i = 0; i < numErrors; i++)
    {
        var r = random.Next(0, messageLength);
        errorsIndexes.Add(r);
    }
    
    return errorsIndexes;
}

List<bool> GetMessageWithErrors(List<bool> message, HashSet<int> errorsIndexes)
{
    var messageWithErrors = new List<bool>();
    for (var i = 0; i < message.Count; i++)
    {
        if (errorsIndexes.Contains(i))
            messageWithErrors.Add(!message[i]);
        else
            messageWithErrors.Add(message[i]);
    }
    
    return messageWithErrors;
}

void WriteToConsoleWithErrors(List<bool> message, HashSet<int> errorsIndexes)
{
    for (var i = 0; i < message.Count; i++)
    {
        if (errorsIndexes.Contains(i))
            Console.ForegroundColor = ConsoleColor.DarkRed;
        
        Console.Write(message[i] ? '1' : '0');
        Console.ResetColor();
    }
    Console.WriteLine();
}

string ToUtf8String(IReadOnlyList<bool> bits)
{
    var bytes = new List<byte>();
    for (var i = 0; i < bits.Count; i += 8)
    {
        var b = 0;
        for (var j = 0; j < 8; j++)
        {
            b |= (bits[i + j] ? 1 : 0) << j;
        }
        bytes.Add((byte)b);
    }
    
    var s = Encoding.UTF8.GetString(bytes.ToArray());
    return s;
}