using Microsoft.AspNetCore.Components;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticKernelApp
{
   
    public class ProductSkill
    {
        [KernelFunction("QueryProduct")]
        [Description("查询商品信息")]

        public string QueryProduct([Description("商品Id")] string productId)
        {
            // 模拟数据库查询
            return $"商品 {productId}，价格 ¥199，库存 23 件";
        }


        [KernelFunction("GetDateTime")]
        [Description("获取当前时间")]
        public string GetDateTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

     
    
}
