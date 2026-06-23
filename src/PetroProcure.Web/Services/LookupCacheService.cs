using PetroProcure.Contracts.V1.Common;
using PetroProcure.Web.Services.Api;

namespace PetroProcure.Web.Services;

public interface ILookupCacheService
{
    Task<string> GetUserNameAsync(Guid? id, CancellationToken ct = default);
    Task<string> GetDepartmentNameAsync(Guid? id, CancellationToken ct = default);
    Task<IReadOnlyList<LookupDto>> GetUsersAsync(CancellationToken ct = default);
    Task<IReadOnlyList<LookupDto>> GetDepartmentsAsync(CancellationToken ct = default);
    void Clear();
}

public sealed class LookupCacheService(IPetroProcureApiClient api) : ILookupCacheService
{
    private IReadOnlyList<LookupDto>? _users;
    private IReadOnlyList<LookupDto>? _departments;

    public async Task<string> GetUserNameAsync(Guid? id, CancellationToken ct = default)
    {
        if (!id.HasValue || id.Value == Guid.Empty) return "کاربر نامشخص";
        var user = (await GetUsersAsync(ct)).FirstOrDefault(item => item.Id == id.Value);
        return string.IsNullOrWhiteSpace(user?.Name) ? "کاربر نامشخص" : user.Name;
    }

    public async Task<string> GetDepartmentNameAsync(Guid? id, CancellationToken ct = default)
    {
        if (!id.HasValue || id.Value == Guid.Empty) return "واحد نامشخص";
        var department = (await GetDepartmentsAsync(ct)).FirstOrDefault(item => item.Id == id.Value);
        return string.IsNullOrWhiteSpace(department?.Name) ? "واحد نامشخص" : department.Name;
    }

    public async Task<IReadOnlyList<LookupDto>> GetUsersAsync(CancellationToken ct = default)
    {
        if (_users is not null) return _users;
        try { _users = await api.GetUserLookupsAsync(ct); }
        catch { _users = []; }
        return _users;
    }

    public async Task<IReadOnlyList<LookupDto>> GetDepartmentsAsync(CancellationToken ct = default)
    {
        if (_departments is not null) return _departments;
        try { _departments = await api.GetDepartmentLookupsAsync(ct); }
        catch { _departments = []; }
        return _departments;
    }

    public void Clear()
    {
        _users = null;
        _departments = null;
    }
}
