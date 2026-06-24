# Supplier Management

Supplier Management is the first master-data business module after the stabilized Purchase File foundation. Suppliers are independent master data, but they are prepared for future integration with Inquiry/RFQ, Tender, Contract, Purchase Order, and Purchase File processes.

## Business purpose

Suppliers represent organizations that may provide goods, services, engineering work, chemicals, instruments, electrical equipment, mechanical equipment, or contracting services. Blacklisted suppliers must not be selected by default in future purchasing processes.

## Entities

- `Supplier`
- `SupplierContact`
- `SupplierCategory`
- `SupplierCategoryAssignment`
- `SupplierDocument`
- `SupplierEvaluation`

Supplier descriptions, contacts, categories, blacklist state, and evaluations are kept as master data. Supplier documents currently use metadata placeholders and are prepared for later integration with the document repository.

## Permissions

- `Supplier.View`
- `Supplier.Create`
- `Supplier.Edit`
- `Supplier.ActivateDeactivate`
- `Supplier.Blacklist`
- `Supplier.ManageContacts`
- `Supplier.ManageCategories`
- `Supplier.Evaluate`
- `Supplier.ManageDocuments`

System administrators receive all permissions. Purchase managers receive full supplier permissions. Purchase experts receive operational supplier permissions. Tender commission managers receive view/evaluation permissions.

## API endpoints

- `GET /api/suppliers`
- `GET /api/suppliers/{id}`
- `GET /api/suppliers/by-code/{supplierCode}`
- `GET /api/suppliers/lookup`
- `POST /api/suppliers`
- `PUT /api/suppliers/{id}`
- `POST /api/suppliers/{id}/activate`
- `POST /api/suppliers/{id}/deactivate`
- `POST /api/suppliers/{id}/blacklist`
- `POST /api/suppliers/{id}/remove-from-blacklist`
- `GET /api/suppliers/{id}/contacts`
- `POST /api/suppliers/{id}/contacts`
- `PUT /api/suppliers/{id}/contacts/{contactId}`
- `POST /api/suppliers/{id}/contacts/{contactId}/deactivate`
- `GET /api/suppliers/categories`
- `POST /api/suppliers/{id}/categories`
- `DELETE /api/suppliers/{id}/categories/{categoryId}`
- `GET /api/suppliers/{id}/evaluations`
- `POST /api/suppliers/{id}/evaluations`

All endpoints use permission-based authorization. User identity is read from authenticated claims; the API does not accept client-supplied user IDs.

## Web UI pages

- `/purchase/suppliers`
- `/purchase/suppliers/create`
- `/purchase/suppliers/{id}`
- `/purchase/suppliers/{id}/edit`
- `/purchase/suppliers/categories`

The Web UI is Persian RTL and uses MudBlazor. Dates use the shared Persian `DateDisplay` component.

## Future integration

Future modules should reuse `SupplierSelector.razor` and the `SupplierLookupDto` contract. Inquiry, Tender, Contract, Purchase Order, and AI risk analysis should exclude inactive and blacklisted suppliers by default unless the user explicitly requests otherwise and has the required permission.

Planned extensions:

- supplier qualification report
- supplier evaluation report
- supplier document upload through the root document repository
- AI-assisted supplier risk analysis
- supplier performance scoring from purchase order and warehouse receipt history
