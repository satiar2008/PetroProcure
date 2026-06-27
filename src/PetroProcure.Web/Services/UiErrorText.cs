namespace PetroProcure.Web.Services;

using PetroProcure.Web.Services.Api;

public static class UiErrorText
{
    public const string Generic = "عملیات ناموفق بود. لطفاً دوباره تلاش کنید.";

    public static string From(Exception exception) =>
        exception is ApiClientException apiException
            ? apiException.Error.UserMessage
            : Generic;
}
