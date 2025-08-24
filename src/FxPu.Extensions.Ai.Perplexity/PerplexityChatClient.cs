using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using FxPu.Utils;
using Microsoft.Extensions.AI;

namespace FxPu.Extensions.Ai.Perplexity;

public sealed class PerplexityChatClient : IChatClient
{
    private readonly string _model;
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://api.perplexity.ai";

    public PerplexityChatClient(string apiKey, string model)
    {
        _model = model;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl)
        };
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

    public async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        // convert messages
        var ppMessages = messages.Select(m => FromChatMessage(m)).ToList();

        // additional properties
        var inlineCitations = true;
        if (!ValueHelper.IsEmpty(options?.AdditionalProperties))
        {
            inlineCitations = options!.AdditionalProperties.GetValueOrDefault("InlineCitations", "true").ToString().ToLower() == "true";
        }

        // create request
        var ppRequest = new PerplexityChatCompletionRequest
        {
            Model = _model,
            Messages = ppMessages,
            ReturnCitations = true,
            ReturnRelatedQuestions = false,
            Temperature = options?.Temperature,
            TopP = options?.TopP,
            MaxTokens = options?.MaxOutputTokens
        };

        // send request
        var sw = Stopwatch.StartNew();
        var httpResponseMessage = await _httpClient.PostAsJsonAsync("/chat/completions", ppRequest, cancellationToken);
        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            var error = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"Error get chat completion: {error}");
        }
        var ppResponse = await httpResponseMessage.Content.ReadFromJsonAsync<PerplexityChatCompletionResponse>(cancellationToken);
        sw.Stop();

        return new ChatResponse(ToChatMessage(ppResponse.Choices.First().Message, inlineCitations, ppResponse.Citations))
        {
            Usage = new UsageDetails
            {
                InputTokenCount = ppResponse.Usage.PromptTokens,
                OutputTokenCount = ppResponse.Usage.CompletionTokens,
                TotalTokenCount = ppResponse.Usage.TotalTokens
            }
        };
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        return null;
    }

    public void Dispose()
    {
    }



    private ChatMessage ToChatMessage(PerplexityChatMessage ppChatMessage, bool inlineCitations, IEnumerable<string>? citations)
    {
        var role = ppChatMessage.Role switch
        {
            "system" => ChatRole.System,
            "user" => ChatRole.User,
            "assistant" => ChatRole.Assistant,
            _ => throw new ArgumentOutOfRangeException(nameof(ppChatMessage.Role))
        };

        // add citations to content
        var content = ppChatMessage.Content;
        if (inlineCitations && !ValueHelper.IsEmpty(citations))
        {
            content += CitationsToMarkDownCitations(citations, "\n\n---\n\n");
        }

        var cahtMessage = new ChatMessage(role, content);
        if (citations != null)
        {
            cahtMessage.AdditionalProperties = new();
            cahtMessage.AdditionalProperties["Citations"] = citations;
        }

        return cahtMessage;
    }


    private PerplexityChatMessage FromChatMessage(ChatMessage chatMessage)
    {
        var ppChatMessage = new PerplexityChatMessage
        {
            Role = chatMessage.Role.Value,
            Content = chatMessage.Text
        };

        // get citations
        var citations = chatMessage.AdditionalProperties?.ContainsKey("Citations") == true
            ? chatMessage.AdditionalProperties["Citations"] as IEnumerable<string>
            : null;

        // when no auto inline citations, add citations as md links at the end for model history
        if (!ValueHelper.IsEmpty(citations))
        {
            ppChatMessage.Content += CitationsToMarkDownCitations(citations, "\n\n---\n\n");
        }

        return ppChatMessage;
    }


    private string? CitationsToMarkDownCitations(IEnumerable<string>? citations, string? prefix = null)
    {
        if (ValueHelper.IsEmpty(citations))
        {
            return null;
        }

        var sb = new StringBuilder(prefix);
        int i = 0;
        foreach (var citation in citations)
        {
            i++;
            sb.Append($"[{i}]: {citation}\n");
        }

        return sb.ToString();
    }

}