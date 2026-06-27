using System.Globalization;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace PetroProcure.AI;

public readonly record struct CallbackAuthResult(bool IsAuthorized, string? FailureReason)
{
    public static CallbackAuthResult Authorized() => new(true, null);
    public static CallbackAuthResult Unauthorized(string reason) => new(false, reason);
}

/// <summary>
/// Authenticates inbound AiCore callbacks using a shared API key/bearer token, an optional
/// HMAC-SHA256 signature over the raw body, and an optional timestamp freshness check.
/// Never logs or returns the configured secret.
/// </summary>
public interface IAiCoreCallbackAuthenticator
{
    CallbackAuthResult Validate(
        string? apiKeyHeader,
        string? authorizationHeader,
        string? signatureHeader,
        string? timestampHeader,
        string rawBody);
}

public sealed class AiCoreCallbackAuthenticator(IOptions<AiCoreIntegrationOptions> options)
    : IAiCoreCallbackAuthenticator
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> seenSignatures = new();

    public CallbackAuthResult Validate(
        string? apiKeyHeader,
        string? authorizationHeader,
        string? signatureHeader,
        string? timestampHeader,
        string rawBody)
    {
        var settings = options.Value;
        var apiKey = settings.ApiKey;
        var callbackSecret = settings.CallbackSecret;

        var hasApiKey = !string.IsNullOrWhiteSpace(apiKey);
        var hasSecret = !string.IsNullOrWhiteSpace(callbackSecret);

        // 1) API key / bearer token check (when an API key is configured).
        if (hasApiKey)
        {
            var bearer = ExtractBearer(authorizationHeader);
            var apiKeyMatches = !string.IsNullOrWhiteSpace(apiKeyHeader) && FixedEquals(apiKeyHeader, apiKey!);
            var bearerMatches = !string.IsNullOrWhiteSpace(bearer) && FixedEquals(bearer!, apiKey!);
            if (!apiKeyMatches && !bearerMatches)
                return CallbackAuthResult.Unauthorized("Invalid or missing AiCore API key.");
        }

        // 2) Timestamp freshness (validated whenever a timestamp header is supplied).
        if (!string.IsNullOrWhiteSpace(timestampHeader))
        {
            if (!TryParseTimestamp(timestampHeader!, out var timestamp))
                return CallbackAuthResult.Unauthorized("Invalid callback timestamp.");

            var tolerance = TimeSpan.FromSeconds(Math.Max(1, settings.CallbackTimestampToleranceSeconds));
            if (Math.Abs((DateTimeOffset.UtcNow - timestamp).TotalSeconds) > tolerance.TotalSeconds)
                return CallbackAuthResult.Unauthorized("Callback timestamp is outside the allowed tolerance.");
        }

        // 3) HMAC signature check (when a callback secret is configured).
        if (hasSecret)
        {
            if (string.IsNullOrWhiteSpace(signatureHeader))
                return CallbackAuthResult.Unauthorized("Missing callback signature.");

            // Signature covers "{timestamp}.{body}" when a timestamp is present, otherwise the body alone.
            var signedPayload = string.IsNullOrWhiteSpace(timestampHeader)
                ? rawBody
                : $"{timestampHeader}.{rawBody}";

            if (!SignatureMatches(signatureHeader!, signedPayload, callbackSecret!))
                return CallbackAuthResult.Unauthorized("Invalid callback signature.");

            if (!string.IsNullOrWhiteSpace(timestampHeader))
            {
                PruneSeenSignatures(settings.CallbackTimestampToleranceSeconds);
                var replayKey = signatureHeader.Trim();
                if (!seenSignatures.TryAdd(replayKey, DateTimeOffset.UtcNow))
                    return CallbackAuthResult.Unauthorized("Replay callback signature.");
            }
        }

        // If neither an API key nor a secret is configured, accept (development/no-auth posture).
        return CallbackAuthResult.Authorized();
    }

    private static string? ExtractBearer(string? authorizationHeader)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader)) return null;
        const string prefix = "Bearer ";
        return authorizationHeader.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? authorizationHeader[prefix.Length..].Trim()
            : null;
    }

    private static bool SignatureMatches(string providedSignature, string signedPayload, string secret)
    {
        var provided = providedSignature.Trim();
        const string shaPrefix = "sha256=";
        if (provided.StartsWith(shaPrefix, StringComparison.OrdinalIgnoreCase))
            provided = provided[shaPrefix.Length..];

        var expectedBytes = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(signedPayload));
        if (!TryDecodeHex(provided, out var providedBytes))
            return false;

        return providedBytes.Length == expectedBytes.Length
            && CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes);
    }

    private static bool TryParseTimestamp(string value, out DateTimeOffset timestamp)
    {
        value = value.Trim();
        // Unix seconds.
        if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var unixSeconds))
        {
            timestamp = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
            return true;
        }
        // ISO-8601.
        return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out timestamp);
    }

    private static bool TryDecodeHex(string value, out byte[] bytes)
    {
        try
        {
            bytes = Convert.FromHexString(value);
            return true;
        }
        catch (FormatException)
        {
            bytes = [];
            return false;
        }
    }

    private static bool FixedEquals(string left, string right) =>
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(left), Encoding.UTF8.GetBytes(right));

    private void PruneSeenSignatures(int toleranceSeconds)
    {
        var cutoff = DateTimeOffset.UtcNow.AddSeconds(-Math.Max(1, toleranceSeconds));
        foreach (var item in seenSignatures)
            if (item.Value < cutoff)
                seenSignatures.TryRemove(item.Key, out _);
    }
}
