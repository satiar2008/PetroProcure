# Legal Document Storage

Legal source documents are stored under the managed procurement root folder. The database stores only relative paths and immutable metadata snapshots.

Default folder layout:

```text
ProcurementRoot/
  Legal/
    Documents/
      YYYY/
        {LegalDocumentId}/
          {safe-stored-file-name}
```

Rules:

- Never store absolute physical paths in the database.
- Keep original file name for display.
- Generate safe stored file names.
- Store extension, MIME type, size, SHA-256 hash, uploader, and upload time.
- Use the shared file upload validation settings from `PetroProcure:FileStorage`.
- Antivirus scanning uses the existing `IFileScanner` abstraction when enabled.
- Delete is soft delete; the physical file is kept for audit and future retention policy.
- Download requires `LegalDocument.View`; upload/delete requires `LegalDocument.Manage`.

Current implementation intentionally does not parse PDF contents automatically. Articles and clauses are entered by authorized admins after upload.
