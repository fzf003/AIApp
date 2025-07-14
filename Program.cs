using Azure;
using Json.Schema.Generation.Intents;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextGeneration;
using OpenAI;
using SemanticKernelApp;
using System.ClientModel;
using System.Text;


var qwenApiKey = "<key>"; // 替换为你的Qwen API Key
var githubkey = "<key>";// 替换为你的Github API Key
var modelId = "qwen-plus";
//var qwenEndpoint = "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions";

var qwenkernel = CreateQwenKernel();

var openkernel = CreaeOpenKernel();


var qwenagent = CreateQwenAgent(qwenkernel);

var openagent = CreateOpenAIAgent(openkernel);


List<ChatCompletionAgent> agents = new List<ChatCompletionAgent>()
{
    qwenagent,
    openagent
};

CancellationTokenSource tokenSource = new CancellationTokenSource();

//tokenSource.CancelAfter(TimeSpan.FromSeconds(40)); // 设置超时时间为30秒


string agentprompt = string.Empty;

while (true)
{
    Console.WriteLine("\n");
    Console.WriteLine("请输入提示词:");
    agentprompt = Console.ReadLine();
    if (string.IsNullOrEmpty(agentprompt))
    {
        break;
    }

    try
    {
        KernelArguments arguments = new() { { "topic", "sea" } };

        var result = await openkernel.InvokePromptAsync(agentprompt,cancellationToken: tokenSource.Token);

        Console.WriteLine($"{AuthorRole.Assistant}: {result.RenderedPrompt}");

        await InvokeAgentAsync(result.RenderedPrompt, cancellationToken: tokenSource.Token);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }

}


Kernel CreateQwenKernel()
{
    var builder = Kernel.CreateBuilder();
    //builder.Services.AddSingleton<IPromptRenderFilter, PromptFilter>();
    builder.Services.AddSingleton<IAutoFunctionInvocationFilter, AutoFilter>();
    builder.Plugins.AddFromType<ProductSkill>("ProductSkill");
    builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));
    builder.Services.AddDashScopeChatCompletion(qwenApiKey, modelId);
    return builder.Build();
}

Kernel CreaeOpenKernel()
{
    var builder = Kernel.CreateBuilder();
    var openAiOptions = new OpenAIClientOptions()
    {
        Endpoint = new Uri("https://models.inference.ai.azure.com")
    };

    var credential = new ApiKeyCredential(githubkey);
    var ghModelsClient = new OpenAIClient(credential, openAiOptions);
    builder.AddOpenAIChatCompletion("gpt-4o", ghModelsClient);
    builder.Services.AddSingleton<IPromptRenderFilter, PromptFilter>();
    builder.Plugins.AddFromType<ProductSkill>("ProductSkill");
    builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));

    return builder.Build();
}

ChatCompletionAgent CreateQwenAgent(Kernel kernel)
{

    KernelFunction getWeatherForCity = kernel.Plugins.GetFunction("ProductSkill", "GetDateTime");

    KernelFunction GetProduct = kernel.Plugins.GetFunction("ProductSkill", "QueryProduct");


    ChatCompletionAgent agent = new()
    {
        Name = "千问",
        Instructions = "你是一个精通北京文化的人.",
        Kernel = kernel.Clone(),

        Arguments = new KernelArguments(new PromptExecutionSettings
        {
            ExtensionData = new Dictionary<string, object?>
              {
                  { "thinking_budget", 4000 }, // 思考预算，单位毫秒
                  { "enable_search", true }, // 是否启用搜索
                  { "enable_thinking", true } // 是否启用思考
              },
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(functions: new[] { getWeatherForCity, GetProduct }, options: new FunctionChoiceBehaviorOptions { AllowStrictSchemaAdherence = true })
        })
    };

    return agent;

}


ChatCompletionAgent CreateOpenAIAgent(Kernel kernel)
{
    ChatCompletionAgent agent = new ChatCompletionAgent
    {
        Name = "OpenAI",
        Instructions = "你是一个精通A股的股神,没有你不知道的投资逻辑,善于推荐投资价值的股票",//"你是一个热点新闻评论者,有犀利的语言风格直戳事件本质",
        Kernel = kernel.Clone(),
        Arguments = new(new PromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new FunctionChoiceBehaviorOptions { AllowStrictSchemaAdherence = true }) })
    };

    return agent;
}



async Task InvokeAgentAsync(string message, bool IsStream = true, CancellationToken cancellationToken = default)
{


    List<string> history = new() { "北京", "千问" };

    ChatCompletionAgent agent = null;



    if (history.Any(p => message.Contains(p)))
    {
        Console.WriteLine("切换到千问");
        agent = agents.Where(p => p.Name == "千问").FirstOrDefault();
    }
    else
    {
        agent = agents.Where(p => p.Name == "OpenAI").FirstOrDefault();
    }



    if (agent is { })
    {


        // message = result?.RenderedPrompt;
        var messagePrompt = new ChatMessageContent(AuthorRole.User, message);
        Console.Write($"{AuthorRole.System}: ");

        List<ChatMessageContent> chatMessageContents = new List<ChatMessageContent>()
           {
               new ChatMessageContent(AuthorRole.User, message,encoding:Encoding.UTF8),
               new ChatMessageContent(AuthorRole.System, $"北京时间:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}",encoding:Encoding.UTF8)
           };

        if (IsStream)
        {
            await foreach (var response in agent!.InvokeStreamingAsync(chatMessageContents, cancellationToken: cancellationToken))
            {
                if (!string.IsNullOrEmpty(response.Message.Content))
                {
                    Console.Write(response.Message.Content);
                }
            }
        }
        else
        {
            await agent!.InvokeAsync(chatMessageContents, cancellationToken: tokenSource.Token).ForEachAsync(p =>
            {

                if (!string.IsNullOrEmpty(p.Message.Content))
                {
                    Console.Write(p.Message.Content);
                }
            });

        }
    }

}



Console.ReadKey();








