# Authentication and authorization

PetroProcure API uses JWT bearer authentication and permission policies.

Required JWT claims:

- `sub` or `nameidentifier`: application user GUID
- `name`: user name
- `email`: email address
- `role`: one or more application roles
- `permission`: optional explicit permissions
- `department_id`: one or more department GUIDs

Endpoints use `RequirePermission(...)`. A valid token without the required permission receives `403`; an anonymous request receives `401`.

Identity values are never accepted from request bodies. Audit fields such as creator, uploader, status changer, and workflow actor are taken from `ICurrentUserService`. System administrator behavior is derived only from the `SystemAdmin` role.

Department-scoped operations additionally verify the authenticated user's department claims or persisted department assignment.
