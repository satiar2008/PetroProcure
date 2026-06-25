# Contract Templates

Contract templates provide reusable clauses for purchase contracts.

## Current behavior

- Templates are stored in the `contract` schema.
- Each template has:
  - Template code
  - Title
  - Description
  - Contract type
  - Active flag
  - Ordered clauses
- Applying a template copies the template clauses into the contract as editable/required contract clauses.
- Copied clauses are snapshots. Later template edits do not rewrite historical contracts.

## Seeded templates

Initial seed data includes:

- Direct Purchase base template.
- Tender based base template.

These templates include required legal/commercial clauses so contracts can be submitted after items are present.

## Web UI

The Web UI currently supports:

- Listing templates.
- Creating templates with an optional initial clause.
- Editing template title, description, type, and active flag.
- Viewing template clauses.
- Applying templates to editable contracts.

Detailed clause editing inside templates is prepared at API level and can be expanded in the UI later if procurement/legal users need full template authoring from the browser.

## Important rule

Template clauses are copied into contracts. The contract keeps its own clause text after copying, because official contracts must remain historically stable.

