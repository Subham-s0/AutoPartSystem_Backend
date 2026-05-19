using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Infrastructure.Settings;

namespace VehiStock.Infrastructure.Services;

public class KhaltiClient : IKhaltiClient
{
    public const string HttpClientName = "Khalti";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly KhaltiSettings _settings;
    private readonly ILogger<KhaltiClient> _logger;

    public KhaltiClient(HttpClient httpClient, IOptions<KhaltiSettings> options, ILogger<KhaltiClient> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_settings.SecretKey))
        {
            throw new InvalidOperationException("Khalti:SecretKey is not configured.");
        }

        if (_httpClient.BaseAddress is null && !string.IsNullOrWhiteSpace(_settings.BaseUrl))
        {
            _httpClient.BaseAddress = new Uri(_settings.BaseUrl.EndsWith("/") ? _settings.BaseUrl : _settings.BaseUrl + "/");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Key", _settings.SecretKey);
    }

    public async Task<KhaltiInitiateResult> InitiateAsync(KhaltiInitiateInput input, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            return_url = _settings.ReturnUrl,
            website_url = _settings.WebsiteUrl,
            amount = input.AmountPaisa,
            purchase_order_id = input.PurchaseOrderId,
            purchase_order_name = input.PurchaseOrderName,
            customer_info = new
            {
                name = input.CustomerName,
                email = input.CustomerEmail,
                phone = input.CustomerPhone
            }
        };

        using var response = await _httpClient.PostAsJsonAsync("epayment/initiate/", payload, JsonOptions, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Khalti initiate failed ({Status}): {Body}", (int)response.StatusCode, body);
            throw new InvalidOperationException($"Khalti payment initiation failed ({(int)response.StatusCode}). {body}");
        }

        var parsed = JsonSerializer.Deserialize<KhaltiInitiateApiResponse>(body, JsonOptions)
            ?? throw new InvalidOperationException("Khalti returned an empty initiate response.");

        if (string.IsNullOrWhiteSpace(parsed.Pidx) || string.IsNullOrWhiteSpace(parsed.PaymentUrl))
        {
            throw new InvalidOperationException("Khalti initiate response is missing pidx or payment_url.");
        }

        return new KhaltiInitiateResult
        {
            Pidx = parsed.Pidx,
            PaymentUrl = parsed.PaymentUrl,
            ExpiresAt = parsed.ExpiresAt
        };
    }

    public async Task<KhaltiLookupResult> LookupAsync(string pidx, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pidx))
        {
            throw new ArgumentException("pidx is required.", nameof(pidx));
        }

        var payload = new { pidx };
        using var response = await _httpClient.PostAsJsonAsync("epayment/lookup/", payload, JsonOptions, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Khalti lookup failed ({Status}): {Body}", (int)response.StatusCode, body);
            throw new InvalidOperationException($"Khalti payment lookup failed ({(int)response.StatusCode}). {body}");
        }

        var parsed = JsonSerializer.Deserialize<KhaltiLookupApiResponse>(body, JsonOptions)
            ?? throw new InvalidOperationException("Khalti returned an empty lookup response.");

        if (string.IsNullOrWhiteSpace(parsed.Pidx) || string.IsNullOrWhiteSpace(parsed.Status))
        {
            throw new InvalidOperationException("Khalti lookup response is missing required fields.");
        }

        return new KhaltiLookupResult
        {
            Pidx = parsed.Pidx,
            Status = parsed.Status,
            TransactionId = parsed.TransactionId,
            TotalAmountPaisa = parsed.TotalAmount,
            FeePaisa = parsed.Fee,
            Refunded = parsed.Refunded
        };
    }

    private sealed class KhaltiInitiateApiResponse
    {
        [JsonPropertyName("pidx")] public string Pidx { get; set; } = string.Empty;
        [JsonPropertyName("payment_url")] public string PaymentUrl { get; set; } = string.Empty;
        [JsonPropertyName("expires_at")] public DateTime? ExpiresAt { get; set; }
    }

    private sealed class KhaltiLookupApiResponse
    {
        [JsonPropertyName("pidx")] public string Pidx { get; set; } = string.Empty;
        [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
        [JsonPropertyName("transaction_id")] public string? TransactionId { get; set; }
        [JsonPropertyName("total_amount")] public int TotalAmount { get; set; }
        [JsonPropertyName("fee")] public int Fee { get; set; }
        [JsonPropertyName("refunded")] public bool Refunded { get; set; }
    }
}
