using System.Globalization;
using Microsoft.EntityFrameworkCore;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Legal;
using PetroProcure.Reporting;

namespace PetroProcure.Infrastructure.Persistence.Repositories;

internal sealed class ReportDataProvider(PetroProcureDbContext db) : IReportDataProvider
{
    public async Task<PurchaseFileReportData?> GetPurchaseFileAsync(Guid id, CancellationToken ct)
    {
        var file = await db.PurchaseFiles.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (file is null) return null;
        var department = await db.Departments.Where(x => x.Id == file.CurrentDepartmentId).Select(x => x.Name).SingleOrDefaultAsync(ct) ?? "—";
        var indent = file.SourceIndentId.HasValue ? await db.Indents.Where(x => x.Id == file.SourceIndentId).Select(x => x.IndentNumber).SingleOrDefaultAsync(ct) : null;
        var items = await db.PurchaseFileItems.AsNoTracking().Where(x => x.PurchaseFileId == id).OrderBy(x => x.MescCode).ToListAsync(ct);
        return new(file.Id, file.FileNumber, file.Title, file.Status.ToString(), department, file.CreatedAt, indent, Group(items.Select(x =>
            new ReportItemData(x.MescCode, x.MescGeneralGroupCode, x.GeneralDescription, x.SpecificDescription, Unit(x.UnitOfMeasureId), x.RequestedQuantity))));
    }

    public async Task<LegalComplianceReportData?> GetLegalComplianceAsync(Guid purchaseFileId, CancellationToken ct)
    {
        var file = await db.PurchaseFiles.AsNoTracking()
            .Where(x => x.Id == purchaseFileId)
            .Select(x => new { x.Id, x.FileNumber, x.Title, x.Status, x.CreatedAt })
            .SingleOrDefaultAsync(ct);
        if (file is null) return null;

        var evaluation = await db.LegalProcurementRuleEvaluations.AsNoTracking()
            .Where(x => x.PurchaseFileId == purchaseFileId || x.EntityId == purchaseFileId)
            .OrderByDescending(x => x.EvaluatedAt)
            .FirstOrDefaultAsync(ct);

        var findings = evaluation is null
            ? new List<ProcurementRuleFinding>()
            : await db.LegalProcurementRuleFindings.AsNoTracking()
                .Where(x => x.ProcurementRuleEvaluationId == evaluation.Id)
                .OrderBy(x => x.Severity)
                .ThenBy(x => x.Title)
                .ToListAsync(ct);

        var audits = await db.LegalRuleAuditLogs.AsNoTracking()
            .Where(x => x.PurchaseFileId == purchaseFileId || x.EntityId == purchaseFileId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        var users = await db.Users.AsNoTracking()
            .Where(x => audits.Select(a => a.UserId).Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.UserName ?? x.Email ?? x.Id.ToString(), ct);
        var findingTitles = findings.ToDictionary(x => x.Id, x => x.Title);

        return new LegalComplianceReportData(file.Id, file.FileNumber, file.Title, Text(file.Status), file.CreatedAt,
            evaluation?.EvaluatedAt, evaluation?.Summary ?? "ارزیابی حقوقی ثبت نشده است.",
            new LegalComplianceSummaryData(
                findings.Count(x => x.Result == RuleResult.Pass),
                findings.Count(x => x.Result == RuleResult.Fail),
                findings.Count(x => x.Result == RuleResult.Warning),
                findings.Count(x => x.Result == RuleResult.NotApplicable),
                findings.Count(x => x.Result == RuleResult.NeedHumanReview || x.NeedHumanReview)),
            findings.Where(x => x.Severity == RuleSeverity.Blocking && x.Result == RuleResult.Fail).Select(Finding).ToArray(),
            findings.Where(x => x.Result == RuleResult.Warning || x.Severity == RuleSeverity.Warning).Select(Finding).ToArray(),
            findings.Where(x => x.Result == RuleResult.NeedHumanReview || x.NeedHumanReview).Select(Finding).ToArray(),
            audits.Select(x => new LegalComplianceAuditReportData(
                x.Action,
                x.FindingId.HasValue && findingTitles.TryGetValue(x.FindingId.Value, out var title) ? title : "—",
                x.PreviousResult ?? "—",
                x.NewResult ?? "—",
                users.TryGetValue(x.UserId, out var user) ? user : x.UserId.ToString(),
                x.Reason ?? x.Summary,
                Date(x.CreatedAt))).ToArray());
    }

    public async Task<IndentReportData?> GetIndentAsync(Guid id, CancellationToken ct)
    {
        var indent = await db.Indents.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (indent is null) return null;
        var department = await db.Departments.Where(x => x.Id == indent.RequestingDepartmentId).Select(x => x.Name).SingleOrDefaultAsync(ct) ?? "—";
        var items = await db.IndentItems.AsNoTracking().Where(x => x.IndentId == id).OrderBy(x => x.MescCode).ToListAsync(ct);
        return new(indent.Id, indent.IndentNumber, indent.IndentType.ToString(), department, Group(items.Select(x =>
            new ReportItemData(x.MescCode, x.MescGeneralGroupCode, x.GeneralDescription, x.SpecificDescription, Unit(x.UnitOfMeasureId), x.RequestedQuantity))));
    }

    public async Task<TenderReportData?> GetTenderAsync(Guid id, CancellationToken ct)
    {
        var tender = await db.Tenders.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (tender is null) return null;
        var file = await db.PurchaseFiles.AsNoTracking().Where(x => x.Id == tender.PurchaseFileId)
            .Select(x => new { x.Id, x.FileNumber, x.Title }).SingleAsync(ct);
        var inquiryNumber = tender.SourceInquiryId.HasValue
            ? await db.Inquiries.AsNoTracking().Where(x => x.Id == tender.SourceInquiryId).Select(x => x.InquiryNumber).SingleOrDefaultAsync(ct)
            : null;
        var participants = await db.TenderParticipants.AsNoTracking().Where(x => x.TenderId == id)
            .OrderBy(x => x.SupplierName)
            .Select(x => new TenderParticipantReportData(x.SupplierCode, x.SupplierName, Text(x.Status), Date(x.InvitedAt)))
            .ToListAsync(ct);
        var items = await db.TenderItems.AsNoTracking().Where(x => x.TenderId == id).OrderBy(x => x.MescCode).ToListAsync(ct);

        return new TenderReportData(tender.Id, tender.TenderNumber, file.Id, file.FileNumber, file.Title,
            inquiryNumber, tender.Title, Text(tender.TenderType), Text(tender.Status), Date(tender.IssueDate),
            Date(tender.SubmissionDeadline), Date(tender.OpeningDate), await User(tender.CreatedByUserId, ct),
            tender.PublishedByUserId.HasValue ? await User(tender.PublishedByUserId.Value, ct) : "—",
            participants, Group(items.Select(x => new ReportItemData(x.MescCode, x.MescGeneralGroupCode,
                x.GeneralDescription, x.SpecificDescription, Unit(x.UnitOfMeasureId), x.Quantity))));
    }

    public async Task<TenderComparisonReportData?> GetTenderComparisonAsync(Guid id, CancellationToken ct)
    {
        var tender = await db.Tenders.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (tender is null) return null;
        var file = await db.PurchaseFiles.AsNoTracking().Where(x => x.Id == tender.PurchaseFileId)
            .Select(x => new { x.Id, x.FileNumber, x.Title }).SingleAsync(ct);
        var suppliers = await (from b in db.TenderBids.AsNoTracking()
                               join p in db.TenderParticipants.AsNoTracking() on b.TenderParticipantId equals p.Id
                               where b.TenderId == id
                               orderby p.SupplierName
                               select new TenderComparisonSupplierReportData(p.SupplierName, b.BidNumber ?? "—",
                                   Number(b.TechnicalScore), Number(b.CommercialScore), Number(b.FinalScore),
                                   Money(b.TotalAmount, b.Currency), Money(b.FinalAmount, b.Currency), b.DeliveryTerms ?? "—",
                                   b.PaymentTerms ?? "—", b.Status == TenderBidStatus.Selected)).ToListAsync(ct);

        var bidItems = await (from item in db.TenderBidItems.AsNoTracking()
                              join bid in db.TenderBids.AsNoTracking() on item.TenderBidId equals bid.Id
                              join participant in db.TenderParticipants.AsNoTracking() on bid.TenderParticipantId equals participant.Id
                              where bid.TenderId == id
                              select new
                              {
                                  item.TenderItemId,
                                  item.MescGeneralGroupCode,
                                  item.GeneralDescription,
                                  item.MescCode,
                                  item.SpecificDescription,
                                  item.Quantity,
                                  participant.SupplierName,
                                  item.UnitPrice,
                                  item.TotalPrice,
                                  item.TechnicalComplianceStatus,
                                  item.TechnicalNote
                              }).ToListAsync(ct);

        var groups = bidItems.GroupBy(x => new { x.MescGeneralGroupCode, x.GeneralDescription })
            .OrderBy(x => x.Key.MescGeneralGroupCode)
            .Select(group => new TenderComparisonItemGroupData(group.Key.MescGeneralGroupCode, group.Key.GeneralDescription,
                group.GroupBy(x => new { x.TenderItemId, x.MescCode, x.SpecificDescription, x.Quantity })
                    .OrderBy(x => x.Key.MescCode)
                    .Select(item => new TenderComparisonItemReportData(item.Key.MescCode, item.Key.SpecificDescription,
                        item.Key.Quantity, item.OrderBy(x => x.SupplierName).Select(x => new TenderComparisonBidItemReportData(
                            x.SupplierName, Money(x.UnitPrice, null), Money(x.TotalPrice, null),
                            Text(x.TechnicalComplianceStatus), x.TechnicalNote ?? "—")).ToArray())).ToArray())).ToArray();

        var winner = suppliers.FirstOrDefault(x => x.IsSelected)?.SupplierName ?? "—";
        return new TenderComparisonReportData(tender.Id, tender.TenderNumber, file.Id, file.FileNumber, file.Title,
            suppliers, groups, winner);
    }

    public async Task<TenderWinnerDecisionReportData?> GetTenderWinnerDecisionAsync(Guid id, CancellationToken ct)
    {
        var tender = await db.Tenders.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (tender is null) return null;
        var file = await db.PurchaseFiles.AsNoTracking().Where(x => x.Id == tender.PurchaseFileId)
            .Select(x => new { x.Id, x.FileNumber }).SingleAsync(ct);
        var decision = await db.TenderDecisions.AsNoTracking()
            .Where(x => x.TenderId == id && x.DecisionType == TenderDecisionType.SelectWinner)
            .OrderByDescending(x => x.DecisionDate).FirstOrDefaultAsync(ct);
        var bid = decision?.SelectedTenderBidId is Guid bidId
            ? await db.TenderBids.AsNoTracking().SingleOrDefaultAsync(x => x.Id == bidId, ct)
            : await db.TenderBids.AsNoTracking().Where(x => x.TenderId == id && x.Status == TenderBidStatus.Selected).FirstOrDefaultAsync(ct);
        var supplier = bid is null ? null : await db.TenderParticipants.AsNoTracking()
            .Where(x => x.Id == bid.TenderParticipantId).Select(x => x.SupplierName).SingleOrDefaultAsync(ct);
        var commissionDecision = bid is null ? null : await db.TenderCommissionDecisions.AsNoTracking()
            .Where(x => x.TenderId == id && x.SelectedTenderBidId == bid.Id).OrderByDescending(x => x.CreatedAt)
            .Select(x => new { x.Id, x.SessionId }).FirstOrDefaultAsync(ct);
        var sessionNumber = commissionDecision is null ? null : await db.TenderCommissionSessions.AsNoTracking()
            .Where(x => x.Id == commissionDecision.SessionId).Select(x => x.SessionNumber).SingleOrDefaultAsync(ct);
        var reference = commissionDecision is null ? "—" : $"{sessionNumber ?? "جلسه"} / {commissionDecision.Id.ToString("N")[..8]}";

        return new TenderWinnerDecisionReportData(tender.Id, tender.TenderNumber, file.Id, file.FileNumber,
            supplier ?? "—", bid?.BidNumber ?? "—", Date(decision?.DecisionDate), decision?.Reason ?? "—",
            reference, Money(bid?.FinalAmount, bid?.Currency), decision?.Notes ?? "—");
    }

    public async Task<CommissionSessionReportData?> GetCommissionSessionAsync(Guid id, CancellationToken ct)
    {
        var session = await db.TenderCommissionSessions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (session is null) return null;
        var tenderNumber = await db.Tenders.AsNoTracking().Where(x => x.Id == session.TenderId).Select(x => x.TenderNumber).SingleOrDefaultAsync(ct) ?? "—";
        var fileNumber = await db.PurchaseFiles.AsNoTracking().Where(x => x.Id == session.PurchaseFileId).Select(x => x.FileNumber).SingleOrDefaultAsync(ct) ?? "—";
        var members = await db.TenderCommissionMembers.AsNoTracking().Where(x => x.SessionId == id).OrderBy(x => x.Role)
            .Select(x => new CommissionMemberReportData(x.FullNameSnapshot, Text(x.Role), Text(x.AttendanceStatus),
                x.VoteStatus.HasValue ? Text(x.VoteStatus.Value) : "—")).ToListAsync(ct);
        var agenda = await db.TenderCommissionAgendaItems.AsNoTracking().Where(x => x.SessionId == id).OrderBy(x => x.OrderNo)
            .Select(x => new CommissionAgendaReportData(x.OrderNo, x.Title, x.Description ?? "—", Text(x.Status), x.Notes ?? "—"))
            .ToListAsync(ct);
        var minutesRaw = await db.TenderCommissionMinutes.AsNoTracking().Where(x => x.SessionId == id).OrderBy(x => x.CreatedAt).ToListAsync(ct);
        var agendaLookup = await db.TenderCommissionAgendaItems.AsNoTracking().Where(x => x.SessionId == id)
            .ToDictionaryAsync(x => x.Id, x => x.Title, ct);
        var minutes = minutesRaw.Select(x => new CommissionMinuteReportData(
            x.AgendaItemId.HasValue && agendaLookup.TryGetValue(x.AgendaItemId.Value, out var title) ? title : "عمومی",
            x.Text, Date(x.CreatedAt))).ToArray();
        var decisions = await CommissionDecisions(id, ct);
        var chair = members.FirstOrDefault(x => x.Role == Text(TenderCommissionMemberRole.Chairperson))?.FullName ?? "—";
        var secretary = members.FirstOrDefault(x => x.Role == Text(TenderCommissionMemberRole.Secretary))?.FullName ?? "—";

        return new CommissionSessionReportData(session.Id, session.SessionNumber, session.TenderId, tenderNumber,
            session.PurchaseFileId, fileNumber, session.Title, Date(session.SessionDate), session.Location ?? "—",
            Text(session.Status), chair, secretary, members, agenda, minutes, decisions);
    }

    public async Task<CommissionDecisionReportData?> GetCommissionDecisionAsync(Guid sessionId, Guid decisionId, CancellationToken ct) =>
        (await CommissionDecisions(sessionId, ct)).SingleOrDefault(x => x.Id == decisionId);

    public async Task<ContractReportData?> GetContractAsync(Guid id, CancellationToken ct)
    {
        var contract = await db.PurchaseContracts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (contract is null) return null;
        var fileNumber = await db.PurchaseFiles.AsNoTracking()
            .Where(x => x.Id == contract.PurchaseFileId).Select(x => x.FileNumber).SingleOrDefaultAsync(ct) ?? "—";
        var supplierName = await db.Suppliers.AsNoTracking()
            .Where(x => x.Id == contract.SupplierId).Select(x => x.Name).SingleOrDefaultAsync(ct) ?? "—";
        var tenderNumber = contract.TenderId.HasValue
            ? await db.Tenders.AsNoTracking().Where(x => x.Id == contract.TenderId).Select(x => x.TenderNumber).SingleOrDefaultAsync(ct) ?? "—"
            : "—";
        var commissionReference = "—";
        if (contract.CommissionDecisionId.HasValue)
        {
            var decision = await db.TenderCommissionDecisions.AsNoTracking()
                .Where(x => x.Id == contract.CommissionDecisionId.Value)
                .Select(x => new { x.Id, x.SessionId })
                .SingleOrDefaultAsync(ct);
            if (decision is not null)
            {
                var sessionNumber = await db.TenderCommissionSessions.AsNoTracking()
                    .Where(x => x.Id == decision.SessionId).Select(x => x.SessionNumber).SingleOrDefaultAsync(ct);
                commissionReference = $"{sessionNumber ?? "جلسه"} / {decision.Id.ToString("N")[..8]}";
            }
        }

        var items = await db.PurchaseContractItems.AsNoTracking()
            .Where(x => x.ContractId == id)
            .OrderBy(x => x.MescCode)
            .ToListAsync(ct);
        var clauses = await db.ContractClauses.AsNoTracking()
            .Where(x => x.ContractId == id)
            .OrderBy(x => x.OrderNo)
            .Select(x => new ContractClauseReportData(x.OrderNo, x.Title, x.Body, Text(x.ClauseType), x.IsRequired))
            .ToListAsync(ct);

        return new ContractReportData(contract.Id, contract.ContractNumber, fileNumber, supplierName,
            tenderNumber, commissionReference, contract.Title, contract.Subject, Text(contract.Status),
            Text(contract.ContractType), contract.Currency, Money(contract.TotalAmount, contract.Currency),
            Money(contract.TaxAmount, contract.Currency), Money(contract.FinalAmount, contract.Currency),
            Date(contract.StartDate), Date(contract.EndDate), Date(contract.DeliveryDeadline),
            contract.PaymentTerms ?? "—", contract.DeliveryTerms ?? "—", contract.WarrantyTerms ?? "—",
            contract.PenaltyTerms ?? "—", Group(items.Select(x => new ReportItemData(x.MescCode,
                x.MescGeneralGroupCode, x.GeneralDescription, x.SpecificDescription, Unit(x.UnitOfMeasureId), x.Quantity))),
            clauses);
    }

    public async Task<PurchaseOrderReportData?> GetPurchaseOrderAsync(Guid id, CancellationToken ct)
    {
        var po = await db.PurchaseOrders.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (po is null) return null;
        var fileNumber = await db.PurchaseFiles.AsNoTracking()
            .Where(x => x.Id == po.PurchaseFileId).Select(x => x.FileNumber).SingleOrDefaultAsync(ct) ?? "—";
        var supplier = await db.Suppliers.AsNoTracking()
            .Where(x => x.Id == po.SupplierId)
            .Select(x => new { x.SupplierCode, x.Name })
            .SingleOrDefaultAsync(ct);
        var contractNumber = po.ContractId.HasValue
            ? await db.PurchaseContracts.AsNoTracking().Where(x => x.Id == po.ContractId.Value).Select(x => x.ContractNumber).SingleOrDefaultAsync(ct) ?? "—"
            : "—";
        var items = await db.PurchaseOrderItems.AsNoTracking()
            .Where(x => x.PurchaseOrderId == id)
            .OrderBy(x => x.MescCode)
            .ToListAsync(ct);

        return new PurchaseOrderReportData(po.Id, po.PurchaseOrderNumber, fileNumber, contractNumber,
            supplier?.Name ?? "—", supplier?.SupplierCode ?? "—", po.Title, Text(po.Status),
            Text(po.PurchaseOrderType), po.Currency, Money(po.TotalAmount, po.Currency),
            Money(po.TaxAmount, po.Currency), Money(po.DiscountAmount, po.Currency),
            Money(po.FinalAmount, po.Currency), Date(po.OrderDate), Date(po.ExpectedDeliveryDate),
            po.DeliveryLocation ?? "—", po.DeliveryTerms ?? "—", po.PaymentTerms ?? "—",
            po.WarrantyTerms ?? "—", po.Notes ?? "—", Group(items.Select(x => new ReportItemData(
                x.MescCode, x.MescGeneralGroupCode, x.GeneralDescription, x.SpecificDescription,
                Unit(x.UnitOfMeasureId), x.OrderedQuantity))));
    }

    public async Task<WarehouseReceiptReportData?> GetWarehouseReceiptAsync(Guid id, CancellationToken ct)
    {
        var receipt = await db.WarehouseReceipts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (receipt is null) return null;

        var poNumber = await db.PurchaseOrders.AsNoTracking()
            .Where(x => x.Id == receipt.PurchaseOrderId)
            .Select(x => x.PurchaseOrderNumber)
            .SingleOrDefaultAsync(ct) ?? "—";
        var fileNumber = await db.PurchaseFiles.AsNoTracking()
            .Where(x => x.Id == receipt.PurchaseFileId)
            .Select(x => x.FileNumber)
            .SingleOrDefaultAsync(ct) ?? "—";
        var supplierName = await db.Suppliers.AsNoTracking()
            .Where(x => x.Id == receipt.SupplierId)
            .Select(x => x.Name)
            .SingleOrDefaultAsync(ct) ?? "—";
        var warehouseName = await db.Warehouses.AsNoTracking()
            .Where(x => x.Id == receipt.WarehouseId)
            .Select(x => x.Name)
            .SingleOrDefaultAsync(ct) ?? "—";

        var items = await db.WarehouseReceiptItems.AsNoTracking()
            .Where(x => x.WarehouseReceiptId == id)
            .OrderBy(x => x.MescCode)
            .Select(x => new WarehouseReceiptItemReportData(x.MescCode, x.MescGeneralGroupCode,
                x.GeneralDescription, x.SpecificDescription, Unit(x.UnitOfMeasureId), x.OrderedQuantity,
                x.PreviouslyReceivedQuantity, x.ReceivedQuantity, x.RemainingQuantityAfterReceipt,
                Text(x.QualityStatus)))
            .ToListAsync(ct);

        return new WarehouseReceiptReportData(receipt.Id, receipt.ReceiptNumber, poNumber, fileNumber,
            supplierName, warehouseName, Date(receipt.ReceiptDate), receipt.DeliveryNoteNumber ?? "—",
            receipt.CarrierName ?? "—", receipt.VehicleNumber ?? "—", Text(receipt.Status), items);
    }

    private static IReadOnlyList<ReportItemGroupData> Group(IEnumerable<ReportItemData> items) =>
        items.GroupBy(x => new { x.GeneralGroupCode, x.GeneralDescription }).OrderBy(x => x.Key.GeneralGroupCode)
            .Select(x => new ReportItemGroupData(x.Key.GeneralGroupCode, x.Key.GeneralDescription, x.ToArray())).ToArray();
    private static string Unit(Guid id) => id.ToString()[^1] switch { '1' => "عدد", '2' => "متر", '3' => "کیلوگرم", '4' => "لیتر", '5' => "بسته", '6' => "دستگاه", _ => "واحد" };

    private async Task<IReadOnlyList<CommissionDecisionReportData>> CommissionDecisions(Guid sessionId, CancellationToken ct)
    {
        var session = await db.TenderCommissionSessions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == sessionId, ct);
        if (session is null) return [];
        var tenderNumber = await db.Tenders.AsNoTracking().Where(x => x.Id == session.TenderId).Select(x => x.TenderNumber).SingleOrDefaultAsync(ct) ?? "—";
        var fileNumber = await db.PurchaseFiles.AsNoTracking().Where(x => x.Id == session.PurchaseFileId).Select(x => x.FileNumber).SingleOrDefaultAsync(ct) ?? "—";
        var decisions = await db.TenderCommissionDecisions.AsNoTracking().Where(x => x.SessionId == sessionId).OrderBy(x => x.CreatedAt).ToListAsync(ct);
        var result = new List<CommissionDecisionReportData>();
        foreach (var decision in decisions)
        {
            var supplier = decision.SelectedSupplierId.HasValue
                ? await db.Suppliers.AsNoTracking().Where(x => x.Id == decision.SelectedSupplierId.Value).Select(x => x.Name).SingleOrDefaultAsync(ct) ?? "—"
                : "—";
            var bid = decision.SelectedTenderBidId.HasValue
                ? await db.TenderBids.AsNoTracking().Where(x => x.Id == decision.SelectedTenderBidId.Value).Select(x => x.BidNumber).SingleOrDefaultAsync(ct) ?? "—"
                : "—";
            result.Add(new CommissionDecisionReportData(decision.Id, session.SessionNumber, tenderNumber, fileNumber,
                Text(decision.DecisionType), Text(decision.Status), supplier, bid, decision.DecisionText,
                decision.Reason ?? "—", await User(decision.CreatedByUserId, ct),
                decision.ApprovedByUserId.HasValue ? await User(decision.ApprovedByUserId.Value, ct) : "—",
                Date(decision.ApprovedAt)));
        }
        return result;
    }

    private async Task<string> User(Guid id, CancellationToken ct) => await db.Users.AsNoTracking()
        .Where(x => x.Id == id).Select(x => x.UserName ?? x.Email ?? x.Id.ToString()).SingleOrDefaultAsync(ct) ?? "—";
    private static string Number(decimal? value) => value.HasValue ? value.Value.ToString("0.##") : "—";
    private static string Money(decimal? value, string? currency) => value.HasValue
        ? $"{value.Value:N0} {currency}".Trim()
        : "—";

    private static string Date(DateTime? value)
    {
        if (!value.HasValue) return "—";
        var calendar = new PersianCalendar();
        var date = value.Value.Kind == DateTimeKind.Utc ? value.Value.ToLocalTime() : value.Value;
        return $"{calendar.GetYear(date):0000}/{calendar.GetMonth(date):00}/{calendar.GetDayOfMonth(date):00}";
    }

    private static string Text(TenderType value) => value switch
    {
        TenderType.PublicTender => "مناقصه عمومی",
        TenderType.LimitedTender => "مناقصه محدود",
        TenderType.TwoStageTender => "مناقصه دو مرحله‌ای",
        TenderType.SingleStageTender => "مناقصه یک مرحله‌ای",
        TenderType.NegotiatedTender => "مذاکره‌ای",
        _ => value.ToString()
    };
    private static string Text(PurchaseFileStatus value) => value switch
    {
        PurchaseFileStatus.Draft => "پیش‌نویس",
        PurchaseFileStatus.WaitingForIndentReview => "در انتظار بازبینی درخواست",
        PurchaseFileStatus.WaitingForPurchaseDepartment => "در انتظار واحد خرید",
        PurchaseFileStatus.InPurchaseDepartment => "در واحد خرید",
        PurchaseFileStatus.WaitingForTechnicalReview => "در انتظار بررسی فنی",
        PurchaseFileStatus.WaitingForTenderCommission => "در انتظار کمیسیون مناقصه",
        PurchaseFileStatus.InTender => "در مناقصه",
        PurchaseFileStatus.WaitingForContract => "در انتظار قرارداد",
        PurchaseFileStatus.WaitingForPurchaseOrder => "در انتظار سفارش خرید",
        PurchaseFileStatus.WaitingForWarehouseReceipt => "در انتظار رسید انبار",
        PurchaseFileStatus.Completed => "تکمیل‌شده",
        PurchaseFileStatus.Cancelled => "لغوشده",
        PurchaseFileStatus.Archived => "بایگانی‌شده",
        _ => value.ToString()
    };
    private static string Text(RuleResult value) => value switch
    {
        RuleResult.Pass => "قبول",
        RuleResult.Fail => "رد",
        RuleResult.Warning => "هشدار",
        RuleResult.NotApplicable => "غیرقابل اعمال",
        RuleResult.NeedHumanReview => "نیازمند بازبینی انسانی",
        _ => value.ToString()
    };
    private static string Text(RuleSeverity value) => value switch
    {
        RuleSeverity.Info => "اطلاعی",
        RuleSeverity.Warning => "هشدار",
        RuleSeverity.Critical => "بحرانی",
        RuleSeverity.Blocking => "مسدودکننده",
        _ => value.ToString()
    };

    private static LegalComplianceFindingReportData Finding(ProcurementRuleFinding finding) =>
        new(
            finding.Title,
            Text(finding.Result),
            Text(finding.Severity),
            finding.LegalReference,
            finding.Description,
            finding.Result == RuleResult.Fail
                ? "رفع علت عدم انطباق و اجرای دوباره ارزیابی حقوقی."
                : "پیگیری طبق توضیح و مرجع حقوقی ثبت‌شده.",
            finding.IsAiGenerated,
            finding.NeedHumanReview,
            finding.CitationReferences ?? "—");
    private static string Text(TenderStatus value) => value switch
    {
        TenderStatus.Draft => "پیش‌نویس",
        TenderStatus.ReadyToPublish => "آماده انتشار",
        TenderStatus.Published => "منتشرشده",
        TenderStatus.ReceivingBids => "دریافت پیشنهادها",
        TenderStatus.UnderQualification => "ارزیابی صلاحیت",
        TenderStatus.UnderTechnicalEvaluation => "ارزیابی فنی",
        TenderStatus.UnderCommercialEvaluation => "ارزیابی مالی",
        TenderStatus.UnderFinalReview => "بررسی نهایی",
        TenderStatus.WinnerSelected => "برنده انتخاب‌شده",
        TenderStatus.Closed => "بسته‌شده",
        TenderStatus.Cancelled => "لغوشده",
        _ => value.ToString()
    };
    private static string Text(TenderParticipantStatus value) => value switch
    {
        TenderParticipantStatus.Draft => "پیش‌نویس",
        TenderParticipantStatus.Invited => "دعوت‌شده",
        TenderParticipantStatus.Submitted => "ارسال‌شده",
        TenderParticipantStatus.Declined => "انصراف",
        TenderParticipantStatus.Disqualified => "رد صلاحیت",
        _ => value.ToString()
    };
    private static string Text(TechnicalComplianceStatus value) => value switch
    {
        TechnicalComplianceStatus.NotReviewed => "بررسی‌نشده",
        TechnicalComplianceStatus.Compliant => "مطابق",
        TechnicalComplianceStatus.PartiallyCompliant => "نسبتاً مطابق",
        TechnicalComplianceStatus.NonCompliant => "نامطابق",
        TechnicalComplianceStatus.NeedsClarification => "نیازمند شفاف‌سازی",
        _ => value.ToString()
    };
    private static string Text(TenderCommissionSessionStatus value) => value switch
    {
        TenderCommissionSessionStatus.Draft => "پیش‌نویس",
        TenderCommissionSessionStatus.Scheduled => "زمان‌بندی‌شده",
        TenderCommissionSessionStatus.InProgress => "در حال برگزاری",
        TenderCommissionSessionStatus.Completed => "تکمیل‌شده",
        TenderCommissionSessionStatus.Approved => "تأییدشده",
        TenderCommissionSessionStatus.Cancelled => "لغوشده",
        _ => value.ToString()
    };
    private static string Text(TenderCommissionMemberRole value) => value switch
    {
        TenderCommissionMemberRole.Chairperson => "رئیس کمیسیون",
        TenderCommissionMemberRole.Secretary => "دبیر",
        TenderCommissionMemberRole.Member => "عضو",
        TenderCommissionMemberRole.Observer => "ناظر",
        TenderCommissionMemberRole.TechnicalExpert => "کارشناس فنی",
        TenderCommissionMemberRole.FinancialExpert => "کارشناس مالی",
        _ => value.ToString()
    };
    private static string Text(TenderCommissionAttendanceStatus value) => value switch
    {
        TenderCommissionAttendanceStatus.Invited => "دعوت‌شده",
        TenderCommissionAttendanceStatus.Present => "حاضر",
        TenderCommissionAttendanceStatus.Absent => "غایب",
        TenderCommissionAttendanceStatus.Excused => "غیبت موجه",
        _ => value.ToString()
    };
    private static string Text(TenderCommissionVoteStatus value) => value switch
    {
        TenderCommissionVoteStatus.NotVoted => "ثبت نشده",
        TenderCommissionVoteStatus.Approve => "موافق",
        TenderCommissionVoteStatus.Reject => "مخالف",
        TenderCommissionVoteStatus.Abstain => "ممتنع",
        TenderCommissionVoteStatus.NeedsMoreReview => "نیازمند بررسی بیشتر",
        _ => value.ToString()
    };
    private static string Text(TenderCommissionAgendaStatus value) => value switch
    {
        TenderCommissionAgendaStatus.Pending => "در انتظار طرح",
        TenderCommissionAgendaStatus.Discussed => "بررسی‌شده",
        TenderCommissionAgendaStatus.Deferred => "موکول‌شده",
        TenderCommissionAgendaStatus.Closed => "بسته‌شده",
        _ => value.ToString()
    };
    private static string Text(TenderCommissionDecisionType value) => value switch
    {
        TenderCommissionDecisionType.RecommendWinner => "پیشنهاد برنده",
        TenderCommissionDecisionType.ApproveWinner => "تأیید برنده",
        TenderCommissionDecisionType.RejectAll => "رد همه پیشنهادها",
        TenderCommissionDecisionType.Retender => "تجدید مناقصه",
        TenderCommissionDecisionType.RequestTechnicalReview => "درخواست بررسی فنی",
        TenderCommissionDecisionType.RequestCommercialReview => "درخواست بررسی مالی",
        TenderCommissionDecisionType.CancelTender => "لغو مناقصه",
        TenderCommissionDecisionType.Other => "سایر",
        _ => value.ToString()
    };
    private static string Text(TenderCommissionDecisionStatus value) => value switch
    {
        TenderCommissionDecisionStatus.Draft => "پیش‌نویس",
        TenderCommissionDecisionStatus.Approved => "تأییدشده",
        TenderCommissionDecisionStatus.Rejected => "ردشده",
        TenderCommissionDecisionStatus.Cancelled => "لغوشده",
        _ => value.ToString()
    };
    private static string Text(ContractType value) => value switch
    {
        ContractType.DirectPurchase => "خرید مستقیم",
        ContractType.TenderBased => "مبتنی بر مناقصه",
        ContractType.Service => "خدمات",
        ContractType.Framework => "چارچوب",
        ContractType.Other => "سایر",
        _ => value.ToString()
    };
    private static string Text(ContractStatus value) => value switch
    {
        ContractStatus.Draft => "پیش‌نویس",
        ContractStatus.UnderReview => "در حال بررسی",
        ContractStatus.WaitingForApproval => "در انتظار تأیید",
        ContractStatus.Approved => "تأییدشده",
        ContractStatus.Signed => "امضاشده",
        ContractStatus.Active => "فعال",
        ContractStatus.Completed => "تکمیل‌شده",
        ContractStatus.Cancelled => "لغوشده",
        ContractStatus.Archived => "بایگانی‌شده",
        _ => value.ToString()
    };
    private static string Text(ContractClauseType value) => value switch
    {
        ContractClauseType.General => "عمومی",
        ContractClauseType.Technical => "فنی",
        ContractClauseType.Commercial => "بازرگانی",
        ContractClauseType.Payment => "پرداخت",
        ContractClauseType.Delivery => "تحویل",
        ContractClauseType.Warranty => "گارانتی",
        ContractClauseType.Penalty => "جریمه",
        ContractClauseType.Legal => "حقوقی",
        ContractClauseType.AttachmentReference => "ارجاع پیوست",
        ContractClauseType.Other => "سایر",
        _ => value.ToString()
    };
    private static string Text(PurchaseOrderStatus value) => value switch
    {
        PurchaseOrderStatus.Draft => "پیش‌نویس",
        PurchaseOrderStatus.UnderReview => "در حال بررسی",
        PurchaseOrderStatus.WaitingForApproval => "در انتظار تأیید",
        PurchaseOrderStatus.Approved => "تأییدشده",
        PurchaseOrderStatus.Issued => "صادرشده",
        PurchaseOrderStatus.PartiallyReceived => "دریافت جزئی",
        PurchaseOrderStatus.FullyReceived => "دریافت کامل",
        PurchaseOrderStatus.Completed => "تکمیل‌شده",
        PurchaseOrderStatus.Cancelled => "لغوشده",
        PurchaseOrderStatus.Archived => "بایگانی‌شده",
        _ => value.ToString()
    };
    private static string Text(PurchaseOrderType value) => value switch
    {
        PurchaseOrderType.ContractBased => "مبتنی بر قرارداد",
        PurchaseOrderType.DirectPurchase => "خرید مستقیم",
        PurchaseOrderType.TenderBased => "مبتنی بر مناقصه",
        PurchaseOrderType.Service => "خدمات",
        PurchaseOrderType.Other => "سایر",
        _ => value.ToString()
    };
    private static string Text(WarehouseReceiptStatus value) => value switch
    {
        WarehouseReceiptStatus.Draft => "پیش‌نویس",
        WarehouseReceiptStatus.Submitted => "ارسال‌شده",
        WarehouseReceiptStatus.Approved => "تأییدشده",
        WarehouseReceiptStatus.Cancelled => "لغوشده",
        _ => value.ToString()
    };
    private static string Text(WarehouseReceiptQualityStatus value) => value switch
    {
        WarehouseReceiptQualityStatus.NotInspected => "بازرسی نشده",
        WarehouseReceiptQualityStatus.Accepted => "پذیرفته‌شده",
        WarehouseReceiptQualityStatus.PartiallyAccepted => "پذیرش جزئی",
        WarehouseReceiptQualityStatus.Rejected => "ردشده",
        WarehouseReceiptQualityStatus.NeedsInspection => "نیازمند بازرسی",
        _ => value.ToString()
    };
}
