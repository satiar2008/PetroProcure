using PetroProcure.Domain.Modules.Organization;

namespace PetroProcure.UnitTests.Security;

public sealed class UserDepartmentTests
{
    [Fact]
    public void Constructor_AssignsUserProfileToDepartment()
    {
        var userProfileId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();

        var assignment = new UserDepartment(Guid.NewGuid(), userProfileId, departmentId, isPrimary: true);

        Assert.Equal(userProfileId, assignment.UserProfileId);
        Assert.Equal(departmentId, assignment.DepartmentId);
        Assert.True(assignment.IsPrimary);
    }
}
