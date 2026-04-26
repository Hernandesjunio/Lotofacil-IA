using System;
using System.IO;

namespace LotofacilMcp.ContractTests;

internal static class ContractTestFixturePaths
{
    internal static string SyntheticMinWindowJson()
    {
        var repositoryRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

        return Path.Combine(repositoryRoot, "tests", "fixtures", "synthetic_min_window.json");
    }
}

