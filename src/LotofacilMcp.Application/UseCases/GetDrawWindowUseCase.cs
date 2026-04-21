using LotofacilMcp.Application.Mapping;
using LotofacilMcp.Application.Validation;
using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;
using LotofacilMcp.Infrastructure.DatasetVersioning;
using LotofacilMcp.Infrastructure.Providers;

namespace LotofacilMcp.Application.UseCases;

public sealed record GetDrawWindowInput(
    int WindowSize,
    int? EndContestId,
    string FixturePath);

public sealed record GetDrawWindowDeterministicHashInput(
    int WindowSize,
    int? EndContestId);

public sealed record GetDrawWindowResult(
    string DatasetVersion,
    string ToolVersion,
    GetDrawWindowDeterministicHashInput DeterministicHashInput,
    WindowDescriptor Window,
    IReadOnlyList<DrawView> Draws);

public sealed class GetDrawWindowUseCase
{
    public const string ToolVersion = "1.0.0";

    private readonly SyntheticFixtureProvider _fixtureProvider;
    private readonly DatasetVersionService _datasetVersionService;
    private readonly WindowResolver _windowResolver;
    private readonly V0CrossFieldValidator _validator;
    private readonly V0RequestMapper _mapper;

    public GetDrawWindowUseCase(
        SyntheticFixtureProvider fixtureProvider,
        DatasetVersionService datasetVersionService,
        WindowResolver windowResolver,
        V0CrossFieldValidator validator,
        V0RequestMapper mapper)
    {
        _fixtureProvider = fixtureProvider;
        _datasetVersionService = datasetVersionService;
        _windowResolver = windowResolver;
        _validator = validator;
        _mapper = mapper;
    }

    public GetDrawWindowResult Execute(GetDrawWindowInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        _validator.ValidateGetDrawWindow(input);

        var snapshot = _fixtureProvider.LoadSnapshot(input.FixturePath);
        var normalizedDraws = _mapper.MapSnapshotToDomainDraws(snapshot);

        try
        {
            var window = _windowResolver.Resolve(normalizedDraws, input.WindowSize, input.EndContestId);
            return new GetDrawWindowResult(
                DatasetVersion: _datasetVersionService.CreateFromSnapshot(snapshot),
                ToolVersion: ToolVersion,
                DeterministicHashInput: new GetDrawWindowDeterministicHashInput(input.WindowSize, input.EndContestId),
                Window: _mapper.MapWindow(window),
                Draws: _mapper.MapWindowDraws(window));
        }
        catch (DomainInvariantViolationException ex)
        {
            throw MapDomainError(ex);
        }
    }

    private static ApplicationValidationException MapDomainError(DomainInvariantViolationException ex)
    {
        if (ex.Message.Contains("requested end_contest_id", StringComparison.Ordinal))
        {
            return new ApplicationValidationException(
                code: "INVALID_CONTEST_ID",
                message: ex.Message,
                details: new Dictionary<string, object?>());
        }

        if (ex.Message.Contains("insufficient history", StringComparison.Ordinal))
        {
            return new ApplicationValidationException(
                code: "INSUFFICIENT_HISTORY",
                message: ex.Message,
                details: new Dictionary<string, object?>());
        }

        return new ApplicationValidationException(
            code: "INVALID_REQUEST",
            message: ex.Message,
            details: new Dictionary<string, object?>());
    }
}
