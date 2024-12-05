namespace ConvolutionalCoding;

public class PathHolder
{
    public readonly DecoderPath[] Paths;
    private Boolean[] IsFirstNode = [true, true];
    
    public PathHolder(int decoderStatesCount)
    {
        var count = decoderStatesCount / 2;
        Paths = new DecoderPath[count];
        for (var i = 0; i < count; i++)
            Paths[i] = new DecoderPath();
    }

    public void AddSurvivorPath(Transition[] survivors)
    {
        if (IsFirstNode[0])
            AddSurvivorPathFirstly(survivors);
        else
        {
            var survivorIndexes = new int[survivors.Length];
            var prevPathIndexes = new List<int>();
            for (var i = 0; i < survivors.Length; i++)
            {
                survivorIndexes[i] = i;
                for (var j = 0; j < Paths.Length; j++)
                {
                    if (Paths[j].DestinationNode == survivors[i].SourceNode)
                        prevPathIndexes.Add(j);
                }
            }
            
            var pathIndexes = survivorIndexes.ToLookup(x => prevPathIndexes[x], x => x);
            var survivorsToRemove = new Queue<int>();
            for (var i = 0; i < survivorIndexes.Length; i++)
            {
                if (!pathIndexes.Contains(i))
                    survivorsToRemove.Enqueue(i);
            }

            foreach (var survIndexes in pathIndexes)
            {
                var pathIndex = survIndexes.Key;
                var indexes = survIndexes.ToList();
                if (indexes.Count > 1)
                {
                    var survivorToRemove = survivorsToRemove.Dequeue();
                    Paths[survivorToRemove].SurvivorPath.Clear();
                    foreach (var node in Paths[pathIndex].SurvivorPath) 
                        Paths[survivorToRemove].AddSurvivorNode(node);
                    
                    Paths[survivorToRemove].AddSurvivorNode(survivors[indexes[1]]);
                }

                if (indexes.Count >= 1)
                    Paths[pathIndex].AddSurvivorNode(survivors[indexes[0]]);
            }
        }
    }

    private void AddSurvivorPathFirstly(Transition[] survivors)
    {
        for (var i = 0; i < survivors.Length; i++)
            Paths[i].AddSurvivorNode(survivors[i]);
        IsFirstNode[0] = false;
    }

    public int GetPathMetricByNode(int node)
    {
        if (Paths[0].SurvivorPath.Count <= 0) 
            return 0;
        
        foreach (var path in Paths)
        {
            if (path.DestinationNode == node)
                return path.Metrics;
        }
    
        return 0;
    }
}