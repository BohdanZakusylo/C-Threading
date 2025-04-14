using System.Text.Json;
using System.Text.Json.Serialization;

namespace ABMB.Controllers.AirbnbModule;

public class AvailabilityResponse
{
    [JsonPropertyName("requestId")]
    public string? RequestId { get; set; }

    [JsonPropertyName("results")]
    public List<AvailabilityResult> Results { get; set; }
}

public class AvailabilityResult
{
    [JsonPropertyName("date")]
    public string Date { get; set; }

    [JsonPropertyName("closed_to_arrival")]
    public int ClosedToArrival { get; set; }

    [JsonPropertyName("closed_to_departure")]
    public int ClosedToDeparture { get; set; }

    [JsonPropertyName("available")]
    public int Available { get; set; }

    [JsonPropertyName("available_for_checkin")]
    public int AvailableForCheckin { get; set; }

    [JsonPropertyName("minNights")]
    public int MinNights { get; set; }

    [JsonPropertyName("maxNights")]
    public int MaxNights { get; set; }
}

public class PriceResponse
{
    [JsonPropertyName("request_id")]
    public string RequestId { get; set; }

    [JsonPropertyName("results")]
    public List<PriceResult> Results { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; }
}

public class PriceResult
{
    [JsonPropertyName("date")]
    public string Date { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("price_eur")]
    public decimal PriceEur { get; set; }

    [JsonPropertyName("price_usd")]
    public decimal PriceUsd { get; set; }
}
