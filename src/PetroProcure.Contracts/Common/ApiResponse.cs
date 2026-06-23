namespace PetroProcure.Contracts.V1.Common;

public sealed record ApiResponse<T>(T? Data, bool Succeeded = true, ErrorDto? Error = null);
