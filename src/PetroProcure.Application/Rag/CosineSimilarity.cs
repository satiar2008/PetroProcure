namespace PetroProcure.Application.Rag;

// AI-RAG-04: pure cosine similarity helper shared by the brute-force index and its tests.
public static class CosineSimilarity
{
    // Returns a value in [-1, 1]. Mismatched dimensions or zero-length vectors yield 0 (no match).
    public static double Compute(IReadOnlyList<float> a, IReadOnlyList<float> b)
    {
        if (a is null || b is null || a.Count == 0 || a.Count != b.Count) return 0d;

        double dot = 0d, magA = 0d, magB = 0d;
        for (var i = 0; i < a.Count; i++)
        {
            double x = a[i], y = b[i];
            dot += x * y;
            magA += x * x;
            magB += y * y;
        }

        if (magA <= 0d || magB <= 0d) return 0d;
        return dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
    }
}
