using PetroProcure.Application.Documents;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Documents;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Infrastructure.Storage;
using PetroProcure.Reporting;

namespace PetroProcure.UnitTests.Reporting;

public sealed class ReportGeneratorTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "PetroProcureReportTests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task ReportGeneratorCreatesPdfBytes()
    {
        var generator = new ReportGenerator(new FakeDataProvider(), new CapturingStorage(),
            new TestCurrentUser(Guid.NewGuid()));

        var bytes = await generator.GeneratePdfAsync(ReportNames.PurchaseFileSummary,
            new Dictionary<string, object?> { ["PurchaseFileId"] = FakeDataProvider.PurchaseFileId });

        Assert.True(bytes.Length > 100);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
    }

    [Fact]
    public async Task GeneratedReportCanBeSavedIntoRootFolderRepository()
    {
        var purchaseFile = new PurchaseFile(
            FakeDataProvider.PurchaseFileId, "PF-2026-000001", "Test", null, PurchaseFilePriority.Normal,
            null, Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid());
        var repository = new FakeFileRepository { PurchaseFile = purchaseFile };
        var storage = new FileStorageService(repository,
            Microsoft.Extensions.Options.Options.Create(new FileStorageOptions
            {
                RootPath = _root,
                QuarantinePath = Path.Combine(_root, ".quarantine"),
                AllowedExtensions = [".pdf"],
                AllowedMimeTypes = ["application/pdf"]
            }), new NoOpFileScanner());
        var generator = new ReportGenerator(new FakeDataProvider(), storage,
            new TestCurrentUser(Guid.NewGuid()));

        var document = await generator.SaveGeneratedReportToPurchaseFileAsync(
            purchaseFile.Id, ReportNames.PurchaseFileSummary,
            new Dictionary<string, object?> { ["PurchaseFileId"] = purchaseFile.Id });

        Assert.False(Path.IsPathRooted(document.RelativePath));
        Assert.Contains("/10-Final/", document.RelativePath);
        Assert.True(File.Exists(Path.Combine(_root, document.RelativePath.Replace('/', Path.DirectorySeparatorChar))));
    }

    [Fact]
    public async Task PurchaseFileReportDataContainsGroupedMescItems()
    {
        var data = await new FakeDataProvider().GetPurchaseFileAsync(FakeDataProvider.PurchaseFileId, default);

        var group = Assert.Single(data!.Groups);
        Assert.Equal("123456", group.GeneralGroupCode);
        Assert.Equal(2, group.Items.Count);
    }

    [Fact]
    public async Task TenderSummaryReportCreatesPdfBytes()
    {
        var generator = new ReportGenerator(new FakeDataProvider(), new CapturingStorage(),
            new TestCurrentUser(Guid.NewGuid()));

        var bytes = await generator.GeneratePdfAsync(ReportNames.TenderSummary,
            new Dictionary<string, object?> { ["TenderId"] = FakeDataProvider.TenderId });

        Assert.True(bytes.Length > 100);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
    }

    [Fact]
    public async Task TenderComparisonReportDataContainsSupplierBids()
    {
        var data = await new FakeDataProvider().GetTenderComparisonAsync(FakeDataProvider.TenderId, default);

        Assert.Equal(2, data!.Suppliers.Count);
        Assert.Contains(data.Groups.SelectMany(x => x.Items), x => x.SupplierItems.Count == 2);
    }

    [Fact]
    public async Task CommissionMinutesReportDataContainsMembersAgendaAndDecisions()
    {
        var data = await new FakeDataProvider().GetCommissionSessionAsync(FakeDataProvider.CommissionSessionId, default);

        Assert.Single(data!.Members);
        Assert.Single(data.AgendaItems);
        Assert.Single(data.Minutes);
        Assert.Single(data.Decisions);
    }

    [Fact]
    public async Task TenderGeneratedReportUsesTenderDocumentFolderAndSafeFileName()
    {
        var storage = new CapturingStorage();
        var generator = new ReportGenerator(new FakeDataProvider(), storage,
            new TestCurrentUser(Guid.NewGuid()));

        await generator.SaveGeneratedReportToPurchaseFileAsync(FakeDataProvider.PurchaseFileId,
            ReportNames.TenderSummary,
            new Dictionary<string, object?> { ["TenderId"] = FakeDataProvider.TenderId, ["TenderNumber"] = "TND/2026:000001" });

        Assert.Equal(DocumentType.TenderDocument, storage.LastDocumentType);
        Assert.DoesNotContain('/', storage.LastOriginalFileName);
        Assert.DoesNotContain(':', storage.LastOriginalFileName);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, true);
    }

    private sealed class FakeDataProvider : IReportDataProvider
    {
        public static readonly Guid PurchaseFileId = Guid.NewGuid();
        public static readonly Guid TenderId = Guid.NewGuid();
        public static readonly Guid CommissionSessionId = Guid.NewGuid();
        public static readonly Guid DecisionId = Guid.NewGuid();
        public Task<PurchaseFileReportData?> GetPurchaseFileAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<PurchaseFileReportData?>(new(
                PurchaseFileId, "PF-2026-000001", "خرید لوله", "در واحد خرید", "واحد خرید",
                DateTime.UtcNow, "2630001",
                [new("123456", "لوله و اتصالات",
                    [new("1234560001","123456","لوله و اتصالات","لوله فولادی","متر",10),
                     new("1234560002","123456","لوله و اتصالات","زانو فولادی","عدد",4)])]));
        public Task<IndentReportData?> GetIndentAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<IndentReportData?>(new(id, "2630001", "دستی", "متقاضی", []));
        public Task<TenderReportData?> GetTenderAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<TenderReportData?>(new(TenderId, "TND-2026-000001", PurchaseFileId, "PF-2026-000001",
                "خرید لوله", "INQ-2026-000001", "مناقصه خرید لوله", "عمومی", "منتشرشده",
                "1405/01/10", "1405/01/20", "1405/01/21", "admin", "manager",
                [new("SUP-001", "شرکت الف", "ارسال‌شده", "1405/01/11")],
                [new("123456", "لوله و اتصالات",
                    [new("1234560001", "123456", "لوله و اتصالات", "لوله فولادی", "متر", 10)])]));
        public Task<TenderComparisonReportData?> GetTenderComparisonAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<TenderComparisonReportData?>(new(TenderId, "TND-2026-000001", PurchaseFileId, "PF-2026-000001",
                "خرید لوله",
                [new("شرکت الف", "BID-001", "90", "95", "93", "1,000", "950", "تحویل فوری", "نقدی", true),
                 new("شرکت ب", "BID-002", "85", "88", "86", "1,100", "1,050", "30 روزه", "اعتباری", false)],
                [new("123456", "لوله و اتصالات",
                    [new("1234560001", "لوله فولادی", 10,
                        [new("شرکت الف", "95", "950", "مطابق", "—"),
                         new("شرکت ب", "105", "1,050", "نسبتاً مطابق", "نیازمند بررسی")])])],
                "شرکت الف"));
        public Task<TenderWinnerDecisionReportData?> GetTenderWinnerDecisionAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<TenderWinnerDecisionReportData?>(new(TenderId, "TND-2026-000001", PurchaseFileId,
                "PF-2026-000001", "شرکت الف", "BID-001", "1405/01/22", "بهترین امتیاز فنی و مالی",
                "صورتجلسه 1", "950", "تصمیم نهایی با کمیسیون است."));
        public Task<CommissionSessionReportData?> GetCommissionSessionAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<CommissionSessionReportData?>(new(CommissionSessionId, "TC-2026-000001", TenderId,
                "TND-2026-000001", PurchaseFileId, "PF-2026-000001", "جلسه کمیسیون مناقصه",
                "1405/01/22", "اتاق جلسات", "تکمیل‌شده", "رئیس کمیسیون", "دبیر کمیسیون",
                [new("عضو اول", "رئیس", "حاضر", "موافق")],
                [new(1, "بررسی پیشنهادها", "ارزیابی پیشنهادهای دریافت‌شده", "تکمیل‌شده", "—")],
                [new("بررسی پیشنهادها", "پیشنهاد شرکت الف به‌عنوان گزینه مناسب مطرح شد.", "1405/01/22")],
                [new(DecisionId, "TC-2026-000001", "TND-2026-000001", "PF-2026-000001",
                    "انتخاب برنده", "تصویب‌شده", "شرکت الف", "BID-001",
                    "شرکت الف انتخاب شد.", "امتیاز بالاتر", "admin", "manager", "1405/01/22")]));
        public Task<CommissionDecisionReportData?> GetCommissionDecisionAsync(Guid sessionId, Guid decisionId, CancellationToken cancellationToken) =>
            Task.FromResult<CommissionDecisionReportData?>(new(DecisionId, "TC-2026-000001", "TND-2026-000001",
                "PF-2026-000001", "انتخاب برنده", "تصویب‌شده", "شرکت الف", "BID-001",
                "شرکت الف انتخاب شد.", "امتیاز بالاتر", "admin", "manager", "1405/01/22"));
    }

    private sealed class CapturingStorage : IFileStorageService
    {
        public string LastOriginalFileName { get; private set; } = "";
        public DocumentType LastDocumentType { get; private set; }
        public Task EnsurePurchaseFileFoldersAsync(PurchaseFile purchaseFile, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<FileDocumentDto> SaveFileAsync(Guid purchaseFileId, DocumentType documentType, string originalFileName, Stream stream, Guid uploadedByUserId, Guid? departmentId = null, string? mimeType = null, string? description = null, CancellationToken cancellationToken = default)
        {
            LastOriginalFileName = originalFileName;
            LastDocumentType = documentType;
            return Task.FromResult(new FileDocumentDto(Guid.NewGuid(), purchaseFileId, null, documentType, originalFileName, "stored.pdf", "PurchaseFiles/test.pdf", ".pdf", "application/pdf", stream.Length, "hash", 1, uploadedByUserId, DateTime.UtcNow, false, description));
        }
        public Task<StoredFileContent> OpenFileAsync(Guid fileDocumentId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteFileAsync(Guid fileDocumentId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<FileDocumentDto> CreateNewVersionAsync(Guid fileDocumentId, Stream stream, Guid createdByUserId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public string GetRelativePath(PurchaseFile purchaseFile, DocumentType documentType, string storedFileName) => "";
        public Task<IReadOnlyList<FileDocumentDto>> GetPurchaseFileDocumentsAsync(Guid purchaseFileId, bool includeDeleted = false, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<FileDocumentDto>>([]);
    }

    private sealed class FakeFileRepository : IFileDocumentRepository
    {
        public PurchaseFile? PurchaseFile { get; set; }
        public FileDocument? Document { get; private set; }
        public Task<PurchaseFile?> FindPurchaseFileAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(PurchaseFile);
        public Task<FileDocument?> FindDocumentAsync(Guid id, bool includeVersions, CancellationToken cancellationToken) => Task.FromResult(Document);
        public Task AddDocumentAsync(FileDocument document, CancellationToken cancellationToken) { Document = document; return Task.CompletedTask; }
        public Task<IReadOnlyList<FileDocumentDto>> GetDocumentsAsync(Guid purchaseFileId, bool includeDeleted, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<FileDocumentDto>>([]);
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
