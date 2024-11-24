namespace ConvolutionalCoding;

public class DecoderPath
{
    public readonly List<Transition> SurvivorPath = [];
    public int Metrics => SurvivorPath[^1].Metrics;
    public int DestinationNode => SurvivorPath[^1].DestinationNode;
    public int Count => SurvivorPath.Count;

    public void AddSurvivorNode(Transition node) =>
        SurvivorPath.Add(node);
}