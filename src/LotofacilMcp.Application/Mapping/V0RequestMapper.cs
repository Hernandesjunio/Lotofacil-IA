using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Normalization;
using LotofacilMcp.Domain.Windows;
using LotofacilMcp.Application.UseCases;
using LotofacilMcp.Infrastructure.Providers;

namespace LotofacilMcp.Application.Mapping;

public sealed class V0RequestMapper
{
    private readonly DrawNormalizer _drawNormalizer;

    public V0RequestMapper(DrawNormalizer drawNormalizer)
    {
        _drawNormalizer = drawNormalizer;
    }

    public IReadOnlyList<Draw> MapSnapshotToDomainDraws(FixtureSnapshot snapshot)
    {
        if (snapshot is null)
        {
            throw new ArgumentNullException(nameof(snapshot));
        }

        return snapshot.Draws
            .Select(_drawNormalizer.Normalize)
            .OrderBy(draw => draw.ContestId)
            .ToArray();
    }

    public WindowDescriptor MapWindow(DrawWindow window)
    {
        if (window is null)
        {
            throw new ArgumentNullException(nameof(window));
        }

        return new WindowDescriptor(window.Size, window.StartContestId, window.EndContestId);
    }

    public DrawView[] MapWindowDraws(DrawWindow window)
    {
        if (window is null)
        {
            throw new ArgumentNullException(nameof(window));
        }

        return window.Draws
            .Select(draw => new DrawView(
                draw.ContestId,
                draw.DrawDate.ToString("yyyy-MM-dd"),
                draw.Numbers.ToArray()))
            .ToArray();
    }
}
