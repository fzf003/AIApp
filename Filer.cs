using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;

public sealed class PromptFilter : IPromptRenderFilter
{


    public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
    {

        Console.WriteLine("================修改提示语开始===================================");
        Console.WriteLine($"Filter:{context.Function.Name}");
        Console.WriteLine(JsonSerializer.Serialize(context.Arguments));
        Console.WriteLine($"是否流:{context.IsStreaming}");
        // await context.Kernel.InvokePromptAsync("你是一个精通北京文化人,介绍自己");
        // context.Arguments.Add("prompt", "你是一个精通北京文化人,介绍自己");

       KernelFunction getWeatherForCity = context.Kernel.Plugins.GetFunction("ProductSkill", "GetDateTime");

        var currdatetime = await getWeatherForCity.InvokeAsync<string>(context.Kernel, context.Arguments);

        await next(context);


        //context.Arguments.Add("currdatetime", currdatetime);
      
       // KernelArguments arguments = new() { { "currdatetime", currdatetime } };

        context.RenderedPrompt = $"千问,当前时间:{currdatetime} 夏威夷有什么?";

        Console.WriteLine("最终:" + context.RenderedPrompt);



        Console.WriteLine("================修改提示语结束===================================");
    }
}

public class AutoFilter : IAutoFunctionInvocationFilter
{
    public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
    {
        Console.WriteLine("================修改自动提示语开始===================================");
        Console.WriteLine($"Filter:{context.Function.Name}");
        Console.WriteLine(JsonSerializer.Serialize(context.Arguments));
        Console.WriteLine($"是否流:{context.IsStreaming}");
         Console.WriteLine("最终:" + context.ChatMessageContent.Content);
        Console.WriteLine("================修改自动提示语结束===================================");
    }
}