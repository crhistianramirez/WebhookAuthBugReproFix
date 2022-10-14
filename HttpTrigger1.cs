using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using System.Security.Cryptography;
using System.Linq;

namespace BugRepro.Functions
{
    public static class HttpTrigger1
    {
        [FunctionName("ordercalculate")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            if(await IsRequestValid(req))
            {
                return new OkObjectResult("Success!");
            } else
            {
                return new OkObjectResult("Forbidden!");
            }
        }

        public static async Task<bool> IsRequestValid(HttpRequest req)
        {
            var sent = req.Headers["X-oc-hash"].FirstOrDefault();
            string requestBodyString = await new StreamReader(req.Body).ReadToEndAsync();
            var bodyBytes = Encoding.UTF8.GetBytes(requestBodyString);
            var keyBytes = Encoding.UTF8.GetBytes("YOUR_WEBHOOK_HASH_KEY"); // get from environment variables
            var hash = new HMACSHA256(keyBytes).ComputeHash(bodyBytes);
            var computed = Convert.ToBase64String(hash);
            var isValid = sent == computed;
            return isValid;
        }
    }
}
