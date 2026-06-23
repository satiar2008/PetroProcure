using System.Net;

namespace PetroProcure.Web.Services.Api;

public enum ApiErrorType
{
    None = 0,
    Unauthorized,
    Forbidden,
    NotFound,
    ValidationError,
    ServerError,
    NetworkError,
    UnknownError
}

public sealed record ApiClientError(ApiErrorType Type, string UserMessage, string? TechnicalMessage = null);

public sealed record ApiClientResult<T>(T? Value, ApiClientError? Error)
{
    public bool Succeeded => Error is null;
    public static ApiClientResult<T> Success(T value) => new(value, null);
    public static ApiClientResult<T> Failure(ApiClientError error) => new(default, error);
}

public sealed class ApiClientException(ApiClientError error) : HttpRequestException(error.UserMessage)
{
    public ApiClientError Error { get; } = error;
}

public static class ApiClientErrors
{
    public static ApiClientError FromStatusCode(HttpStatusCode statusCode, string? detail = null) =>
        statusCode switch
        {
            HttpStatusCode.Unauthorized => new(ApiErrorType.Unauthorized,
                "نشست کاربری معتبر نیست. لطفاً دوباره وارد سامانه شوید.", detail),
            HttpStatusCode.Forbidden => new(ApiErrorType.Forbidden,
                "شما مجوز لازم برای انجام این عملیات را ندارید.", detail),
            HttpStatusCode.NotFound => new(ApiErrorType.NotFound,
                "اطلاعات درخواستی یافت نشد.", detail),
            HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity => new(ApiErrorType.ValidationError,
                "اطلاعات واردشده معتبر نیست. لطفاً فرم را بررسی کنید.", detail),
            >= HttpStatusCode.InternalServerError => new(ApiErrorType.ServerError,
                "خطای داخلی سرویس رخ داد. لطفاً بعداً دوباره تلاش کنید.", detail),
            _ => new(ApiErrorType.UnknownError,
                "ارتباط با سرویس با خطای نامشخص مواجه شد.", detail)
        };
}
