using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace RateTheWork.Infrastructure.Services;

public interface ISecretService
{
    Task<string?> GetSecretAsync(string key);
    Task SetSecretAsync(string key, string value);
    Task DeleteSecretAsync(string key);
}

public class CloudflareKVService : ISecretService
{
    private readonly RestClient _client;
    private readonly ILogger<CloudflareKVService> _logger;
    private readonly string _accountId;
    private readonly string _namespaceId;
    private readonly string _apiToken;

    public CloudflareKVService(
        IConfiguration configuration,
        ILogger<CloudflareKVService> logger)
    {
        _logger = logger;
        _accountId = configuration["CLOUDFLARE_ACCOUNT_ID"] ?? 
            throw new InvalidOperationException("CLOUDFLARE_ACCOUNT_ID not configured");
        _namespaceId = configuration["CLOUDFLARE_KV_NAMESPACE_ID"] ?? 
            throw new InvalidOperationException("CLOUDFLARE_KV_NAMESPACE_ID not configured");
        _apiToken = configuration["CLOUDFLARE_KV_API_TOKEN"] ?? 
            throw new InvalidOperationException("CLOUDFLARE_KV_API_TOKEN not configured");

        _client = new RestClient($"https://api.cloudflare.com/client/v4/accounts/{_accountId}/storage/kv/namespaces/{_namespaceId}");
        _client.AddDefaultHeader("Authorization", $"Bearer {_apiToken}");
    }

    public async Task<string?> GetSecretAsync(string key)
    {
        try
        {
            var request = new RestRequest($"values/{key}");
            var response = await _client.ExecuteAsync(request);

            if (response.IsSuccessful && response.Content != null)
            {
                return response.Content;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Secret not found for key: {Key}", key);
                return null;
            }

            _logger.LogError("Failed to get secret: {StatusCode} - {Content}", 
                response.StatusCode, response.Content);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting secret for key: {Key}", key);
            throw;
        }
    }

    public async Task SetSecretAsync(string key, string value)
    {
        try
        {
            var request = new RestRequest($"values/{key}", Method.Put);
            request.AddStringBody(value, DataFormat.None);
            
            var response = await _client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                _logger.LogError("Failed to set secret: {StatusCode} - {Content}", 
                    response.StatusCode, response.Content);
                throw new InvalidOperationException($"Failed to set secret: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting secret for key: {Key}", key);
            throw;
        }
    }

    public async Task DeleteSecretAsync(string key)
    {
        try
        {
            var request = new RestRequest($"values/{key}", Method.Delete);
            var response = await _client.ExecuteAsync(request);

            if (!response.IsSuccessful && response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogError("Failed to delete secret: {StatusCode} - {Content}", 
                    response.StatusCode, response.Content);
                throw new InvalidOperationException($"Failed to delete secret: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting secret for key: {Key}", key);
            throw;
        }
    }
}