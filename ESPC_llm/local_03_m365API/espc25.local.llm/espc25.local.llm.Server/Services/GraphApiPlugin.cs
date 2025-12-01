using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;

namespace espc25.local.llm.Server.Services;

/// <summary>
/// Plugin for Microsoft Graph API calls using authenticated HTTP client
/// </summary>
public class GraphApiPlugin
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    public GraphApiPlugin(HttpClient httpClient, ILogger logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    [KernelFunction("GetMyProfile")]
    [Description("Get the current user's profile information from Microsoft Graph")]
    public async Task<string> GetMyProfileAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("https://graph.microsoft.com/v1.0/me");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            return $"Error: {response.StatusCode} - {response.ReasonPhrase}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile from Microsoft Graph");
            return $"Error getting user profile: {ex.Message}";
        }
    }

    [KernelFunction("GetMyGroups")]
    [Description("Get the groups that the current user is a member of")]
    public async Task<string> GetMyGroupsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("https://graph.microsoft.com/v1.0/me/memberOf");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            return $"Error: {response.StatusCode} - {response.ReasonPhrase}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user groups from Microsoft Graph");
            return $"Error getting user groups: {ex.Message}";
        }
    }

    [KernelFunction("GetMyMail")]
    [Description("Get the current user's recent emails")]
    [return: Description("Recent emails from the user's mailbox")]
    public async Task<string> GetMyMailAsync(
        [Description("Number of emails to retrieve (default: 10, max: 50)")] int count = 10)
    {
        try
        {
            count = Math.Min(count, 50); // Limit to 50 for performance
            var response = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/me/messages?$top={count}&$select=subject,from,receivedDateTime,isRead");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            return $"Error: {response.StatusCode} - {response.ReasonPhrase}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user mail from Microsoft Graph");
            return $"Error getting user mail: {ex.Message}";
        }
    }

    [KernelFunction("GetMyCalendarEvents")]
    [Description("Get the current user's upcoming calendar events")]
    [return: Description("Upcoming calendar events")]
    public async Task<string> GetMyCalendarEventsAsync(
        [Description("Number of events to retrieve (default: 10, max: 25)")] int count = 10)
    {
        try
        {
            count = Math.Min(count, 25); // Limit to 25 for performance
            var response = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/me/events?$top={count}&$select=subject,start,end,organizer,attendees");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            return $"Error: {response.StatusCode} - {response.ReasonPhrase}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting calendar events from Microsoft Graph");
            return $"Error getting calendar events: {ex.Message}";
        }
    }

    [KernelFunction("SearchFiles")]
    [Description("Search for files in the user's OneDrive")]
    [return: Description("Search results from OneDrive")]
    public async Task<string> SearchFilesAsync(
        [Description("Search query for files")] string query,
        [Description("Number of results to return (default: 10, max: 25)")] int count = 10)
    {
        try
        {
            count = Math.Min(count, 25); // Limit to 25 for performance
            var encodedQuery = Uri.EscapeDataString(query);
            var response = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/me/drive/search(q='{encodedQuery}')?$top={count}&$select=name,webUrl,lastModifiedDateTime,size");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            return $"Error: {response.StatusCode} - {response.ReasonPhrase}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching files in Microsoft Graph");
            return $"Error searching files: {ex.Message}";
        }
    }

    [KernelFunction("GetMyContacts")]
    [Description("Get the current user's contacts")]
    [return: Description("User's contact list")]
    public async Task<string> GetMyContactsAsync(
        [Description("Number of contacts to retrieve (default: 20, max: 100)")] int count = 20)
    {
        try
        {
            count = Math.Min(count, 100); // Limit to 100 for performance
            var response = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/me/contacts?$top={count}&$select=displayName,emailAddresses,businessPhones,mobilePhone,companyName,jobTitle");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            return $"Error: {response.StatusCode} - {response.ReasonPhrase}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user contacts from Microsoft Graph");
            return $"Error getting user contacts: {ex.Message}";
        }
    }
}
