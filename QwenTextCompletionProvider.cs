using Microsoft.SemanticKernel;

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.TextGeneration;

public class QwenTextGenerationService : ITextGenerationService
{
    private readonly string _apiKey;
    private readonly string _endpoint;

    public QwenTextGenerationService(string apiKey, string endpoint)
    {
        _apiKey = apiKey;
        _endpoint = endpoint;
    }

    // 可选：添加自定义属性
    public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

    // 实现同步文本生成接口
    public async Task<IReadOnlyList<TextContent>> GetTextContentsAsync(
        string prompt,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var payload = new
        {
            model = "qwen-plus", // 按需换成你购买的模型
          
               messages = new[]
             {
               /* new
                {
                    role = "system",
                    content = "你是一个智能助手，擅长回答各种问题。请根据用户的输入提供准确和有用的回答。"
                },*/
                new
                {
                    role = "user",
                    content = prompt
                }
            },
            parameters=new
            {
                
                result_format = "message", // 返回格式
                top_p = 0.8, // 采样参数
                temperature = 0.7, // 温度参数
                enable_search = true, // 是否启用搜索
                enable_thinking = true, // 是否启用思考
                thinking_budget = 4000 // 思考预算，单位毫秒
            }
           
        };

       

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await client.PostAsync(_endpoint, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync(cancellationToken);

      
        using var doc = JsonDocument.Parse(result);
        var output = doc.RootElement.TryGetProperty("output", out var outputNode)
            ? outputNode.GetString() ?? ""
            : result; 

        var textContent = new TextContent(output);
        return new List<TextContent> { textContent };
    }

    // 若暂时不支持流式，直接抛异常
    public IAsyncEnumerable<StreamingTextContent> GetStreamingTextContentsAsync(
        string prompt,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("QwenTextGenerationService 暂不支持流式输出。");
    }
}