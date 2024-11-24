using System.Collections;
using ConvolutionalCoding;

var coder = new Encoder([13, 15]);
var encMessage = coder.Encode([true, false, true, false]);
Console.WriteLine($"Encoded \"1010\": {encMessage.Select(x => x ? 1 : 0).Aggregate("", (str, x) => str + x)}");