using System.ComponentModel;
using LotofacilMcp.Server.Helping;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace LotofacilMcp.Server.Resources;

[McpServerResourceType]
public sealed class HelpResources
{
    private const string MimeMarkdown = "text/markdown";

    [McpServerResource(
        UriTemplate = "lotofacil-ia://help/getting-started@1.0.0",
        Name = "Getting started (onboarding curto)",
        MimeType = MimeMarkdown)]
    [Description("Onboarding curto e agnóstico ao host: fluxo help → index → pipeline mínimo + lembretes normativos.")]
    public static ResourceContents GettingStarted()
        => ReadMarkdownResource(HelpCatalog.GettingStartedUri, HelpCatalog.GettingStartedFileName);

    private static ResourceContents ReadMarkdownResource(string uri, string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "resources", "help", fileName);
        var text = File.ReadAllText(path);

        return new TextResourceContents
        {
            Uri = uri,
            MimeType = MimeMarkdown,
            Text = text
        };
    }
}

