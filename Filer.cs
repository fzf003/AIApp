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
       // await context.Kernel.InvokePromptAsync("你是一个精通北京文化人,介绍自己");
        await next(context);

        Console.WriteLine("最终:" + context.RenderedPrompt);
        Console.WriteLine("===============================================================");
    }
}