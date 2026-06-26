using System.Text;
using Microsoft.Extensions.Options;
using PetroProcure.Application.Documents;
using PetroProcure.Domain.Modules.Legal;
using PetroProcure.Infrastructure.Storage;

namespace PetroProcure.UnitTests.Infrastructure;

public sealed class LegalDocumentStorageServiceTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "PetroProcureLegalTests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task LegalDocumentStoresRelativePathOnlyAndCalculatesHash()
    {
        var service = CreateService();
        var id = Guid.NewGuid();
        var bytes = Encoding.UTF8.GetBytes("قانون مناقصات");

        var stored = await service.SaveAsync(id, "tender-law.txt", new MemoryStream(bytes), "text/plain");

        Assert.False(Path.IsPathRooted(stored.RelativePath));
        Assert.StartsWith($"Legal/Documents/{DateTime.UtcNow.Year}/{id:N}/", stored.RelativePath);
        Assert.Equal("efa4ba81a48e9c74146b05154615ef2c3a1e423793bc12fde2692b600ac7c30f", stored.Hash);
        Assert.True(File.Exists(Path.Combine(_root, stored.RelativePath.Replace('/', Path.DirectorySeparatorChar))));
    }

    [Fact]
    public void LegalDocumentSoftDeleteWorks()
    {
        var document = new LegalDocument(Guid.NewGuid(), "قانون", "law.pdf", "stored.pdf",
            "Legal/Documents/2026/doc/stored.pdf", ".pdf", "application/pdf", 10, "hash", null, Guid.NewGuid());
        var userId = Guid.NewGuid();

        document.SoftDelete(userId);

        Assert.True(document.IsDeleted);
        Assert.Equal(userId, document.DeletedByUserId);
        Assert.NotNull(document.DeletedAt);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, true);
    }

    private LegalDocumentStorageService CreateService() =>
        new(Options.Create(new FileStorageOptions
        {
            RootPath = _root,
            QuarantinePath = Path.Combine(_root, "_quarantine"),
            MaxFileSizeMb = 1,
            AllowedExtensions = [".txt", ".pdf"],
            AllowedMimeTypes = ["text/plain", "application/pdf"]
        }), new NoOpFileScanner());
}
