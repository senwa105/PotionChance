namespace PotionChanceEstimators;

[System.Runtime.CompilerServices.InlineArray(61)]
internal struct BeliefData
{
    private float _element0;
}

public struct Belief
{
    private BeliefData _data = default;

    private const int Offset = 50;
    public const int MinIndex = -50;
    public const int MaxIndex = 10;
    
    public Belief() 
    { 
    }

    // The game allow for negative potion drop chance, so we need an oversized array.
    // Index 60 maps to +100%
    // Index 50 maps to    0%
    // Index  0 maps to -500%
    public ref float this[int chanceIndex]
    {
        get 
        {
            Span<float> span = System.Runtime.InteropServices.MemoryMarshal.CreateSpan(ref _data[0], 61);
            return ref span[chanceIndex + Offset];
        }
    }

    public static Belief FromKnownChance(float chance)
    {
        Belief belief = default;
        int idx = (int)Math.Round(chance * 10f);
        belief[idx] = 1f;

        return belief;
    }
    
    public override string ToString()
    {
        var sb = new System.Text.StringBuilder("[");
        ReadOnlySpan<float> span = _data;
    
        for (int i = 0; i < span.Length; i++)
        {
            sb.Append(span[i]);
            if (i < span.Length - 1) sb.Append(", ");
        }
    
        sb.Append("]");
        return sb.ToString();
    }

    public string ToStringPositiveOnly()
    {
        var sb = new System.Text.StringBuilder("[");
        ReadOnlySpan<float> slice = ((ReadOnlySpan<float>)_data).Slice(Offset, 11);
    
        for (int i = 0; i < slice.Length; i++)
        {
            sb.Append(slice[i]);
            if (i < slice.Length - 1) sb.Append(", ");
        }
    
        sb.Append("]");
        return sb.ToString();
    }
    
    public void Normalize()
    {
        float sum = 0f;
        Span<float> span = System.Runtime.InteropServices.MemoryMarshal.CreateSpan(ref _data[0], 61);
        
        foreach (float val in span) sum += val;
        if (sum == 0f) return;
        
        for (int i = 0; i < span.Length; i++) span[i] /= sum;
    }
}