using PetroProcure.Domain.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Domain.Modules.Warehouse;

public sealed class Warehouse : Entity<Guid>
{
    private Warehouse() : base(Guid.Empty) { Code = Name = string.Empty; }
    public Warehouse(Guid id, string code, string name, string? location = null, string? description = null) : base(id)
    {
        Code = Required(code, nameof(code)).ToUpperInvariant();
        Name = Required(name, nameof(name));
        Location = Trim(location);
        Description = Trim(description);
        IsActive = true;
    }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string? Location { get; private set; }
    public bool IsActive { get; private set; }
    public string? Description { get; private set; }
    public void Update(string name, string? location, bool isActive, string? description)
    {
        Name = Required(name, nameof(name));
        Location = Trim(location);
        IsActive = isActive;
        Description = Trim(description);
    }
    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class WarehouseReceipt : Entity<Guid>
{
    private readonly List<WarehouseReceiptItem> _items = [];
    private readonly List<WarehouseReceiptDocument> _documents = [];
    private WarehouseReceipt() : base(Guid.Empty) { ReceiptNumber = string.Empty; }
    public WarehouseReceipt(Guid id, string receiptNumber, Guid purchaseOrderId, Guid purchaseFileId,
        Guid warehouseId, Guid supplierId, DateTime receiptDate, Guid receivedByUserId, Guid createdByUserId,
        string? deliveryNoteNumber = null, string? carrierName = null, string? vehicleNumber = null,
        string? description = null) : base(id)
    {
        ReceiptNumber = Required(receiptNumber, nameof(receiptNumber));
        PurchaseOrderId = purchaseOrderId == Guid.Empty ? throw new ArgumentException("Purchase order is required.", nameof(purchaseOrderId)) : purchaseOrderId;
        PurchaseFileId = purchaseFileId == Guid.Empty ? throw new ArgumentException("Purchase file is required.", nameof(purchaseFileId)) : purchaseFileId;
        WarehouseId = warehouseId == Guid.Empty ? throw new ArgumentException("Warehouse is required.", nameof(warehouseId)) : warehouseId;
        SupplierId = supplierId == Guid.Empty ? throw new ArgumentException("Supplier is required.", nameof(supplierId)) : supplierId;
        Status = WarehouseReceiptStatus.Draft;
        ReceiptDate = receiptDate;
        DeliveryNoteNumber = Trim(deliveryNoteNumber);
        CarrierName = Trim(carrierName);
        VehicleNumber = Trim(vehicleNumber);
        ReceivedByUserId = receivedByUserId;
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        Description = Trim(description);
    }

    public string ReceiptNumber { get; private set; }
    public Guid PurchaseOrderId { get; private set; }
    public Guid PurchaseFileId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid SupplierId { get; private set; }
    public WarehouseReceiptStatus Status { get; private set; }
    public DateTime ReceiptDate { get; private set; }
    public string? DeliveryNoteNumber { get; private set; }
    public string? CarrierName { get; private set; }
    public string? VehicleNumber { get; private set; }
    public Guid ReceivedByUserId { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public Guid? CancelledByUserId { get; private set; }
    public string? CancellationReason { get; private set; }
    public IReadOnlyCollection<WarehouseReceiptItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<WarehouseReceiptDocument> Documents => _documents.AsReadOnly();

    public void Update(DateTime receiptDate, string? deliveryNoteNumber, string? carrierName, string? vehicleNumber, string? description)
    {
        EnsureEditable();
        ReceiptDate = receiptDate;
        DeliveryNoteNumber = Trim(deliveryNoteNumber);
        CarrierName = Trim(carrierName);
        VehicleNumber = Trim(vehicleNumber);
        Description = Trim(description);
    }
    public void AddItem(WarehouseReceiptItem item)
    {
        EnsureEditable();
        if (item.WarehouseReceiptId != Id) throw new InvalidOperationException("Receipt item belongs to another receipt.");
        _items.Add(item);
    }
    public void RemoveItem(Guid itemId)
    {
        EnsureEditable();
        var item = _items.SingleOrDefault(x => x.Id == itemId) ?? throw new InvalidOperationException("Receipt item was not found.");
        _items.Remove(item);
    }
    public void AddDocument(WarehouseReceiptDocument document)
    {
        if (document.WarehouseReceiptId != Id) throw new InvalidOperationException("Document belongs to another receipt.");
        _documents.Add(document);
    }
    public void Submit()
    {
        EnsureEditable();
        if (_items.Count == 0) throw new InvalidOperationException("Warehouse receipt cannot be submitted without items.");
        Status = WarehouseReceiptStatus.Submitted;
    }
    public void Approve(Guid userId)
    {
        if (Status != WarehouseReceiptStatus.Submitted) throw new InvalidOperationException("Only submitted receipts can be approved.");
        if (_items.Count == 0) throw new InvalidOperationException("Warehouse receipt cannot be approved without items.");
        Status = WarehouseReceiptStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        ApprovedByUserId = userId;
    }
    public void Cancel(string reason, Guid userId)
    {
        EnsureEditable();
        Status = WarehouseReceiptStatus.Cancelled;
        CancellationReason = Required(reason, nameof(reason));
        CancelledAt = DateTime.UtcNow;
        CancelledByUserId = userId;
    }
    public void EnsureEditable()
    {
        if (Status is WarehouseReceiptStatus.Approved or WarehouseReceiptStatus.Cancelled)
            throw new InvalidOperationException("Approved or cancelled warehouse receipts are read-only.");
    }
    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class WarehouseReceiptItem : Entity<Guid>
{
    private WarehouseReceiptItem() : base(Guid.Empty) { MescCode = MescGeneralGroupCode = GeneralDescription = SpecificDescription = string.Empty; }
    public WarehouseReceiptItem(Guid id, Guid warehouseReceiptId, Guid purchaseOrderItemId, Guid mescItemId,
        string mescCode, string mescGeneralGroupCode, string generalDescription, string specificDescription,
        Guid unitOfMeasureId, decimal orderedQuantity, decimal previouslyReceivedQuantity, decimal receivedQuantity,
        decimal remainingQuantityAfterReceipt, WarehouseReceiptQualityStatus qualityStatus,
        decimal? acceptedQuantity = null, decimal? rejectedQuantity = null, string? batchNumber = null,
        string? serialNumber = null, DateTime? expiryDate = null, string? notes = null) : base(id)
    {
        if (receivedQuantity <= 0) throw new ArgumentOutOfRangeException(nameof(receivedQuantity));
        if (remainingQuantityAfterReceipt < 0) throw new ArgumentOutOfRangeException(nameof(remainingQuantityAfterReceipt));
        WarehouseReceiptId = warehouseReceiptId;
        PurchaseOrderItemId = purchaseOrderItemId;
        MescItemId = mescItemId;
        MescCode = Required(mescCode, nameof(mescCode));
        MescGeneralGroupCode = Required(mescGeneralGroupCode, nameof(mescGeneralGroupCode));
        GeneralDescription = Required(generalDescription, nameof(generalDescription));
        SpecificDescription = Required(specificDescription, nameof(specificDescription));
        UnitOfMeasureId = unitOfMeasureId;
        OrderedQuantity = orderedQuantity;
        PreviouslyReceivedQuantity = previouslyReceivedQuantity;
        ReceivedQuantity = receivedQuantity;
        AcceptedQuantity = acceptedQuantity;
        RejectedQuantity = rejectedQuantity;
        RemainingQuantityAfterReceipt = remainingQuantityAfterReceipt;
        QualityStatus = qualityStatus;
        BatchNumber = Trim(batchNumber);
        SerialNumber = Trim(serialNumber);
        ExpiryDate = expiryDate;
        Notes = Trim(notes);
    }
    public Guid WarehouseReceiptId { get; private set; }
    public Guid PurchaseOrderItemId { get; private set; }
    public Guid MescItemId { get; private set; }
    public string MescCode { get; private set; }
    public string MescGeneralGroupCode { get; private set; }
    public string GeneralDescription { get; private set; }
    public string SpecificDescription { get; private set; }
    public Guid UnitOfMeasureId { get; private set; }
    public decimal OrderedQuantity { get; private set; }
    public decimal PreviouslyReceivedQuantity { get; private set; }
    public decimal ReceivedQuantity { get; private set; }
    public decimal? AcceptedQuantity { get; private set; }
    public decimal? RejectedQuantity { get; private set; }
    public decimal RemainingQuantityAfterReceipt { get; private set; }
    public string? BatchNumber { get; private set; }
    public string? SerialNumber { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public WarehouseReceiptQualityStatus QualityStatus { get; private set; }
    public string? Notes { get; private set; }
    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class WarehouseReceiptDocument : Entity<Guid>
{
    private WarehouseReceiptDocument() : base(Guid.Empty) { DocumentType = string.Empty; }
    public WarehouseReceiptDocument(Guid id, Guid warehouseReceiptId, Guid? fileDocumentId, string documentType,
        string? originalFileName, string? description, Guid uploadedByUserId) : base(id)
    {
        WarehouseReceiptId = warehouseReceiptId;
        FileDocumentId = fileDocumentId;
        DocumentType = string.IsNullOrWhiteSpace(documentType) ? "WarehouseReceiptDocument" : documentType.Trim();
        OriginalFileName = Trim(originalFileName);
        Description = Trim(description);
        UploadedAt = DateTime.UtcNow;
        UploadedByUserId = uploadedByUserId;
    }
    public Guid WarehouseReceiptId { get; private set; }
    public Guid? FileDocumentId { get; private set; }
    public string DocumentType { get; private set; }
    public string? OriginalFileName { get; private set; }
    public string? Description { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class InventoryTransaction : Entity<Guid>
{
    private InventoryTransaction() : base(Guid.Empty) { TransactionNumber = string.Empty; }
    public InventoryTransaction(Guid id, string transactionNumber, Guid mescItemId, Guid warehouseId,
        InventoryTransactionType transactionType, InventoryTransactionReferenceType referenceType,
        Guid referenceId, decimal quantity, Guid unitOfMeasureId, DateTime transactionDate,
        Guid createdByUserId, string? description = null) : base(id)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        TransactionNumber = Required(transactionNumber, nameof(transactionNumber));
        MescItemId = mescItemId;
        WarehouseId = warehouseId;
        TransactionType = transactionType;
        ReferenceType = referenceType;
        ReferenceId = referenceId;
        Quantity = quantity;
        UnitOfMeasureId = unitOfMeasureId;
        TransactionDate = transactionDate;
        CreatedAt = DateTime.UtcNow;
        CreatedByUserId = createdByUserId;
        Description = Trim(description);
    }
    public string TransactionNumber { get; private set; }
    public Guid MescItemId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public InventoryTransactionType TransactionType { get; private set; }
    public InventoryTransactionReferenceType ReferenceType { get; private set; }
    public Guid ReferenceId { get; private set; }
    public decimal Quantity { get; private set; }
    public Guid UnitOfMeasureId { get; private set; }
    public DateTime TransactionDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public string? Description { get; private set; }
    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class WarehouseReceiptSequence : Entity<Guid>
{
    private WarehouseReceiptSequence() : base(Guid.Empty) { }
    public WarehouseReceiptSequence(Guid id, int year, int lastNumber) : base(id) { Year = year; LastNumber = lastNumber; }
    public int Year { get; private set; }
    public int LastNumber { get; private set; }
    public int Next() => ++LastNumber;
}

public sealed class InventoryTransactionSequence : Entity<Guid>
{
    private InventoryTransactionSequence() : base(Guid.Empty) { }
    public InventoryTransactionSequence(Guid id, int year, int lastNumber) : base(id) { Year = year; LastNumber = lastNumber; }
    public int Year { get; private set; }
    public int LastNumber { get; private set; }
    public int Next() => ++LastNumber;
}
