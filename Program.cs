// See https://aka.ms/new-console-template for more information
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


var qwenApiKey = "";
var githubkey = "";
var modelId = "qwen-plus";
var qwenEndpoint = "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions";
 



var kernel = CreaeKernel();
 


 

var agent = CreateAgent(kernel);

 

string agentprompt = "你是一位游客";

while (true)
{
    Console.WriteLine("\n");
    Console.WriteLine("请输入提示词:");
    agentprompt = Console.ReadLine();
    if (string.IsNullOrEmpty(agentprompt))
    {
        break;
    }

    await InvokeAgentAsync(agentprompt);


}



Kernel CreaeKernel()
{
    var builder = Kernel.CreateBuilder();
    var openAiOptions = new OpenAIClientOptions()
    {
        Endpoint = new Uri("https://models.inference.ai.azure.com")
    };
    //钱
    var credential = new ApiKeyCredential(githubkey);
    var ghModelsClient = new OpenAIClient(credential, openAiOptions);
    builder.AddOpenAIChatCompletion("gpt-4.1", ghModelsClient);
    builder.Services.AddSingleton<IPromptRenderFilter, PromptFilter>();
    builder.Plugins.AddFromType<ProductSkill>("ProductSkill");
    builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));
    return builder.Build();
}
ChatCompletionAgent CreateAgent(Kernel kernel)
{
    ChatCompletionAgent agent = new ChatCompletionAgent
    {
        Name = "Qwen",
        Instructions = "你是一个精通A股的股神，没有你不知道的投资逻辑，善于推荐投资价值的股票",//"你是一个热点新闻评论者,有犀利的语言风格直戳事件本质",
        Kernel = kernel.Clone(),
        Arguments= new(new PromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new FunctionChoiceBehaviorOptions {  AllowStrictSchemaAdherence = true }) })
    };

    return agent;
}


async Task InvokeAgentAsync(string message)
{
    /*  KernelFunction GetProduct = kernel.Plugins.GetFunction("ProductSkill", "QueryProduct");

      KernelFunction getCurrentTime = kernel.Plugins.GetFunction("ProductSkill", "GetDateTime");

      var arguments = new KernelArguments
      {
          ["productId"] = message
      };

      var orderitem = await GetProduct.InvokeAsync<string>(kernel, arguments);

      var gettime = await getCurrentTime.InvokeAsync<string>(kernel);

      Console.WriteLine($"查询结果:{gettime}:{orderitem}");
    */


    Console.WriteLine($"{AuthorRole.User}: {message}");
    var messagePrompt = new ChatMessageContent(AuthorRole.User, message);
    Console.Write($"{AuthorRole.System}: ");
    await foreach (var response in agent.InvokeStreamingAsync(messagePrompt))
    {
        if (!string.IsNullOrEmpty(response.Message.Content))
        {
            Console.Write(response.Message.Content);
        }
    }

}



Console.ReadKey();








