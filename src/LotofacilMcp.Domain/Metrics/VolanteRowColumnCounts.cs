using LotofacilMcp.Domain.Models;

namespace LotofacilMcp.Domain.Metrics;

internal static class VolanteRowColumnCounts
{
    public static void FillRowCounts(Draw draw, Span<int> destination)
    {
        destination.Clear();
        foreach (var number in draw.Numbers)
        {
            var rowIndex = (number - 1) / 5;
            destination[rowIndex]++;
        }
    }

    public static void FillColumnCounts(Draw draw, Span<int> destination)
    {
        destination.Clear();
        foreach (var number in draw.Numbers)
        {
            var colIndex = (number - 1) % 5;
            destination[colIndex]++;
        }
    }
}
