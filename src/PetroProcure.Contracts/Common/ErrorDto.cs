namespace PetroProcure.Contracts.V1.Common;

public sealed record ErrorDto(string Code, string Message, IReadOnlyDictionary<string, string[]>? ValidationErrors = null);
