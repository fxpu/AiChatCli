namespace FxPu.LlmClient
{
    public interface ILlmClient
    {
        ValueTask<LlmChatCompletionResponse> GetChatCompletionAsync(LlmChatCompletionRequest llmChatCompletionRequest, CancellationToken cancellationToken = default);
    }
}
