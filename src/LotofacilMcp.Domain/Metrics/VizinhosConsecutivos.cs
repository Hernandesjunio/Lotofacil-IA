namespace LotofacilMcp.Domain.Metrics;

/// <summary>
/// Vizinhança por diferença 1 entre dezenas consecutivas na ordenação crescente canônica (mesma regra que
/// <see cref="QuantidadeVizinhosPorConcursoMetric"/> e <see cref="SequenciaMaximaVizinhosPorConcursoMetric"/>).
/// </summary>
public static class VizinhosConsecutivos
{
    /// <summary>
    /// Maior comprimento de bloco de dezenas consecutivas (diferença 1 entre adjacentes no vetor ordenado).
    /// Cada dezena isolada conta como run de comprimento 1.
    /// </summary>
    public static int MaxConsecutiveAdjacencyRunLength(IReadOnlyList<int> strictlyIncreasingUniqueNumbers)
    {
        ArgumentNullException.ThrowIfNull(strictlyIncreasingUniqueNumbers);

        var n = strictlyIncreasingUniqueNumbers.Count;
        if (n == 0)
        {
            return 0;
        }

        if (n == 1)
        {
            return 1;
        }

        var currentRun = 1;
        var maxRun = 1;

        for (var i = 1; i < n; i++)
        {
            if (strictlyIncreasingUniqueNumbers[i] - strictlyIncreasingUniqueNumbers[i - 1] == 1)
            {
                currentRun++;
            }
            else
            {
                currentRun = 1;
            }

            if (currentRun > maxRun)
            {
                maxRun = currentRun;
            }
        }

        return maxRun;
    }
}
