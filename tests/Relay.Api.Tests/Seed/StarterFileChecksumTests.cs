using System.Security.Cryptography;

namespace Relay.Api.Tests.Seed;

public sealed class StarterFileChecksumTests
{
    [Theory]
    [InlineData("schema.sql", "348912f4fd6dade1728058a4f666780c60b94578f4276135583f502616e51d3d")]
    [InlineData("seed.sql", "40e60ee81d999eb32057b4437bc84e9ec197265d4e58c13c0bfbad150e6eaea2")]
    public async Task StarterFileContentMatchesTheOriginalChecksum(string starterFileName, string expectedSha256)
    {
        var starterFilePath = Path.Combine(FindDatabaseFolder(), starterFileName);

        var starterFileBytes = await File.ReadAllBytesAsync(starterFilePath, TestContext.Current.CancellationToken);

        Convert.ToHexStringLower(SHA256.HashData(starterFileBytes)).ShouldBe(expectedSha256);
    }

    private static string FindDatabaseFolder()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var databaseFolder = Path.Combine(directory.FullName, "db");
            if (File.Exists(Path.Combine(databaseFolder, "seed.sql")))
            {
                return databaseFolder;
            }
        }

        throw new DirectoryNotFoundException($"No db/seed.sql above {AppContext.BaseDirectory}.");
    }
}
