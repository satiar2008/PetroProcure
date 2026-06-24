# Inquiry / RFQ

Inquiry / RFQ is the purchasing module used by the Purchase Department to request price, technical, or commercial offers from selected suppliers for one Purchase File.

## Relationship to PurchaseFile

Every inquiry must be linked to a Purchase File. Inquiry items are normally copied from Purchase File items and keep MESC, description, unit, quantity, and technical-description snapshots.

## Relationship to Supplier

Inquiry suppliers are copied from Supplier master data and keep supplier code/name/contact snapshots. Inactive or blacklisted suppliers are excluded by default.

## Entities

- `Inquiry`
- `InquiryItem`
- `InquirySupplier`
- `SupplierQuote`
- `SupplierQuoteItem`
- `InquiryDocument`
- `InquirySequence`

## Status lifecycle

Typical lifecycle:

`Draft -> Sent -> PartiallyResponded/FullyResponded -> SupplierSelected -> Closed`

An inquiry can also be cancelled before close. Closed and cancelled inquiries are read-only.

## Permissions

- `Inquiry.View`
- `Inquiry.Create`
- `Inquiry.Edit`
- `Inquiry.Send`
- `Inquiry.Cancel`
- `Inquiry.ManageSuppliers`
- `Inquiry.ReceiveQuote`
- `Inquiry.CompareQuotes`
- `Inquiry.SelectSupplier`
- `Inquiry.ManageDocuments`

## API endpoints

- `GET /api/inquiries`
- `GET /api/inquiries/{id}`
- `GET /api/inquiries/by-number/{inquiryNumber}`
- `GET /api/purchase-files/{purchaseFileId}/inquiries`
- `POST /api/inquiries`
- `POST /api/inquiries/from-purchase-file/{purchaseFileId}`
- `PUT /api/inquiries/{id}`
- `POST /api/inquiries/{id}/send`
- `POST /api/inquiries/{id}/cancel`
- `POST /api/inquiries/{id}/close`
- `GET /api/inquiries/{id}/items`
- `GET /api/inquiries/{id}/items/grouped`
- `POST /api/inquiries/{id}/items`
- `DELETE /api/inquiries/{id}/items/{itemId}`
- `GET /api/inquiries/{id}/suppliers`
- `POST /api/inquiries/{id}/suppliers`
- `DELETE /api/inquiries/{id}/suppliers/{inquirySupplierId}`
- `GET /api/inquiries/{id}/quotes`
- `GET /api/inquiries/{id}/comparison`
- `POST /api/inquiries/{id}/quotes`
- `POST /api/inquiries/{id}/quotes/{quoteId}/items`
- `POST /api/inquiries/{id}/quotes/{quoteId}/select`
- `POST /api/inquiries/{id}/quotes/{quoteId}/reject`

## Web pages

- `/purchase/inquiries`
- `/purchase/inquiries/create`
- `/purchase/inquiries/from-purchase-file/{purchaseFileId}`
- `/purchase/inquiries/{id}`

## Future relation to Tender, Contract, PurchaseOrder

Inquiry comparison prepares the data needed for Tender, Contract, and Purchase Order modules. Later phases can add official inquiry reports, AI quote comparison, supplier risk analysis, and missing quote document checks.
