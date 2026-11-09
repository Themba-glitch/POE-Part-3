using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

namespace RaceDay.Mvc.Services;

public class ApiResponse
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public HttpStatusCode StatusCode { get; init; }
    public string Body { get; init; } = "";
    public List<string> SetCookies { get; init; } = new();
    public bool Ok => (int)StatusCode >= 200 && (int)StatusCode < 300;

    public T? Read<T>()
    {
        try { return JsonSerializer.Deserialize<T>(Body, JsonOptions); }
        catch (JsonException) { return default; }
    }

    public string FriendlyError()
    {
        if (StatusCode == HttpStatusCode.ServiceUnavailable)
            return "We can't reach the RaceDay service right now. Please try again shortly.";
        if (StatusCode == HttpStatusCode.Unauthorized)
            return "Your session has expired. Please log in again.";
        if (StatusCode == HttpStatusCode.Forbidden)
            return "You don't have permission to do that.";

        var text = Body.Trim().Trim('"');
        if (!string.IsNullOrEmpty(text) && !text.StartsWith("{") && !text.StartsWith("<") && text.Length <= 200)
            return text;

        try
        {
            using var doc = JsonDocument.Parse(Body);
            if (doc.RootElement.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in errors.EnumerateObject())
                    foreach (var msg in prop.Value.EnumerateArray())
                        return msg.GetString() ?? "Please check your details and try again.";
            }
        }
        catch (JsonException) { }

        return "Something went wrong. Please try again.";
    }
}

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _accessor;

    public ApiClient(HttpClient http, IHttpContextAccessor accessor)
    {
        _http = http;
        _accessor = accessor;
    }

    // Every call to the API goes through here. It attaches the logged-in user's API session cookie.
    public async Task<ApiResponse> SendAsync(HttpMethod method, string url, object? body = null)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url);

            var apiCookie = _accessor.HttpContext?.User.FindFirstValue("ApiCookie");
            if (!string.IsNullOrEmpty(apiCookie))
                request.Headers.Add("Cookie", apiCookie);

            if (body != null)
                request.Content = JsonContent.Create(body);

            using var response = await _http.SendAsync(request);

            var cookies = response.Headers.TryGetValues("Set-Cookie", out var values)
                ? values.ToList()
                : new List<string>();

            return new ApiResponse
            {
                StatusCode = response.StatusCode,
                Body = await response.Content.ReadAsStringAsync(),
                SetCookies = cookies
            };
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return new ApiResponse { StatusCode = HttpStatusCode.ServiceUnavailable };
        }
    }
}
