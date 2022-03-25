using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

public partial class Membership2AspNetIdentityApiClient
{
    private readonly HttpClient _httpClient;

    public Membership2AspNetIdentityApiClient(
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(configuration.GetValue<string>("Membership2AspNetIdentityApi"));
    }

    public async Task<MembershipPasswordResult?> GetClearTextPassword(string username)
    {
        var endpoint = $"Home/GetClearTextPassword?username={username}";
        return await _httpClient.GetFromJsonAsync<MembershipPasswordResult>(endpoint);
    }
}

