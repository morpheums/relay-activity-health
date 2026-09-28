namespace Relay.Api.Tests.Fixtures;

public sealed class DotEnvDirectory : IDisposable
{
    public const string SaPassword = "Pa$w0rd-Test1";
    public const string DatabasePort = "14999";
    public const string InterpolatedConnectionString =
        "Server=localhost,14999;Database=relay;User Id=sa;Password=Pa$w0rd-Test1;TrustServerCertificate=True";

    private const string DotEnvContent = """
        RELAY_DB_SA_PASSWORD='Pa$w0rd-Test1'
        RELAY_DB_PORT="14999"
        ConnectionStrings__Relay="Server=localhost,${RELAY_DB_PORT};Database=relay;User Id=sa;Password=${RELAY_DB_SA_PASSWORD};TrustServerCertificate=True"

        """;

    private readonly DirectoryInfo contentRootDirectory = Directory.CreateTempSubdirectory("relay-dotenv-");

    public DotEnvDirectory() =>
        File.WriteAllText(Path.Combine(contentRootDirectory.FullName, ".env"), DotEnvContent.ReplaceLineEndings("\n"));

    public string ContentRoot => contentRootDirectory.FullName;

    public void Dispose() => contentRootDirectory.Delete(recursive: true);
}
