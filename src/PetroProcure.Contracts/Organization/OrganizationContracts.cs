namespace PetroProcure.Contracts.V1.Organization;

public sealed record DepartmentDto(Guid Id, string Name, string Type, bool IsActive);
public sealed record UpdateDepartmentRequest(string Name);
