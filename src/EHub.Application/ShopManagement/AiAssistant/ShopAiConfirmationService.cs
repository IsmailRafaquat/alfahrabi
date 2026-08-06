using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Timing;

namespace EHub.ShopManagement.AiAssistant;

public class ShopAiConfirmationService : IShopAiConfirmationService, ITransientDependency
{
    private readonly IOptions<ShopAiOptions> _options;
    private readonly IClock _clock;

    public ShopAiConfirmationService(IOptions<ShopAiOptions> options, IClock clock)
    {
        _options = options;
        _clock = clock;
    }

    public ShopAiConfirmationToken Issue(Guid tenantId, Guid userId, Guid conversationId, Guid messageId, ShopAiActionType action, string payloadJson)
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var token = Base64Url(tokenBytes);
        var expiryDate = _clock.Now.AddMinutes(Math.Max(1, _options.Value.ConfirmationExpiryMinutes));

        var hash = ComputeHash(token, tenantId, userId, conversationId, messageId, action, payloadJson, expiryDate);

        return new ShopAiConfirmationToken { Token = token, TokenHash = hash, ExpiryDate = expiryDate };
    }

    public ShopAiConfirmationCheckResult Verify(
        string providedToken,
        string storedTokenHash,
        Guid tenantId,
        Guid userId,
        Guid conversationId,
        Guid messageId,
        ShopAiActionType action,
        string payloadJson,
        DateTime? expiryDate)
    {
        if (string.IsNullOrWhiteSpace(providedToken) || string.IsNullOrWhiteSpace(storedTokenHash))
        {
            return ShopAiConfirmationCheckResult.Fail("AiConfirmationInvalid", "ShopManagement:AiConfirmationInvalid");
        }

        if (!expiryDate.HasValue || _clock.Now > expiryDate.Value)
        {
            return ShopAiConfirmationCheckResult.Fail("AiConfirmationExpired", "ShopManagement:AiConfirmationExpired");
        }

        // expiryDate is folded into the hash itself, so a tampered expiry can never make an
        // otherwise-expired token pass - the hash simply won't match unless the stored value is used.
        var expected = ComputeHash(providedToken, tenantId, userId, conversationId, messageId, action, payloadJson, expiryDate.Value);

        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var storedBytes = Encoding.UTF8.GetBytes(storedTokenHash);
        var matches = expectedBytes.Length == storedBytes.Length && CryptographicOperations.FixedTimeEquals(expectedBytes, storedBytes);

        return matches
            ? ShopAiConfirmationCheckResult.Ok()
            : ShopAiConfirmationCheckResult.Fail("AiConfirmationInvalid", "ShopManagement:AiConfirmationInvalid");
    }

    private static string ComputeHash(string token, Guid tenantId, Guid userId, Guid conversationId, Guid messageId, ShopAiActionType action, string payloadJson, DateTime expiryDate)
    {
        var payloadHash = Base64Url(SHA256.HashData(Encoding.UTF8.GetBytes(payloadJson)));
        var signingInput = $"{token}|{tenantId:N}|{userId:N}|{conversationId:N}|{messageId:N}|{action}|{payloadHash}|{expiryDate.Ticks}";
        return Base64Url(SHA256.HashData(Encoding.UTF8.GetBytes(signingInput)));
    }

    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
