using System.Security.Cryptography;
using System.Text;

namespace PetroProcure.Infrastructure.Persistence.Seeding;

public static class StableGuid
{
    public static Guid Create(string value)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(value));
        return new Guid(bytes);
    }
}
