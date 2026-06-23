namespace PetroProcure.Web.Services;

public sealed class TabLoadState<T>
{
    public T? Data { get; set; }
    public bool Loading { get; set; }
    public string? Error { get; set; }
}
