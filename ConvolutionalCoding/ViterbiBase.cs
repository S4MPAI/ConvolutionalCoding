using System.Collections;

namespace ConvolutionalCoding;

public abstract class ViterbiBase(IEnumerable<int> polynoms)
{
    public readonly List<int> Polynoms = polynoms.ToList();
    public int CoderBitness => Polynoms.Count;
    
    public int RegisterLength =>
        _registerLength == -1 ? _registerLength = Helpers.GetBitCount(Polynoms.Max()) : _registerLength;
    private int _registerLength = -1;

    public Dictionary<int, int> CoderStates => _coderStates ??= CalculateCoderStates();
    private Dictionary<int, int>? _coderStates;
    
    public Lookup<int, Transition> Transitions => _transitions ??= CalculateTransitionsGrid();
    private Lookup<int, Transition>? _transitions;
    
    public int StatesCount => _statesCount == 0 ? _statesCount = (int)Math.Pow(2, RegisterLength) : _statesCount;
    private int _statesCount;
    
    protected int TransitionsCount => 
        _transitionsCount == 0 ? _transitionsCount = (int)Math.Pow(2, RegisterLength - 1) : _transitionsCount;
    private int _transitionsCount;


    private Dictionary<int, int> CalculateCoderStates()
    {
        var states = new Dictionary<int, int> { { 0, 0 } };
        for (var stateNumber = 1; stateNumber < StatesCount; stateNumber++)
        {
            var adderOuts = new List<bool>(Polynoms.Count);
            var register = new BitArray(stateNumber);
            foreach (var polynom in Polynoms)
            {
                var adderOut = false;

                for (var i = 0; i < RegisterLength; i++)
                {
                    var polynomBit = ((polynom >> i) & 1) > 0;
                    adderOut ^= polynomBit & register[i];
                }
                
                adderOuts.Add(adderOut);
            }

            var state = adderOuts
                .Select(adderOut => adderOut ? 1 : 0)
                .Aggregate(0, (current, currentBit) => (current << 1) | currentBit);
            states.Add(stateNumber, state);
        }
        
        return states;
    }
    
    private Lookup<int, Transition> CalculateTransitionsGrid()
    {
        var transitions = new List<Transition>(TransitionsCount);

        for (var node = 0; node < TransitionsCount; node++)
        {
            for (int mostSignificantBit = 0; mostSignificantBit <= 1; mostSignificantBit++)
            {
                var reg = node | (mostSignificantBit << (RegisterLength - 1));
                var coderOut = CoderStates[reg];
                var destinationNode = reg >> 1;
                transitions.Add(new Transition(RegisterLength, Polynoms.Count, node, destinationNode, coderOut));
            }
        }
        
        return (Lookup<int, Transition>)transitions.ToLookup(x => x.SourceNode, x => x);
    }

    protected static Transition GetNextTransition(bool bitToEncode, IEnumerable<Transition> currentTransitions)
    {
        foreach (var transition in currentTransitions)
        {
            if (bitToEncode)
            {
                if (transition.DestinationNodeMsb)
                    return transition;
            }
            else
            {
                if (!transition.DestinationNodeMsb)
                    return transition;
            }
        }
        
        return null;
    }
}