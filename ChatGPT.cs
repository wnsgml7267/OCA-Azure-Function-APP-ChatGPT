using System.Net;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace assignment3
{
    public class ChatGPT
    {
        private readonly ILogger _logger;

        public ChatGPT(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ChatGPT>();
        }

        [Function("ChatGPT")]

        [OpenApiOperation(operationId: nameof(ChatGPT.Run), tags: new[] { "name" })]
        [OpenApiRequestBody(contentType: "text/plain", bodyType: typeof(string), Required = true, Description = "The request body")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]

        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "POST", Route = "completions")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            // 요청 payload 읽어들인 것
            var prompt = req.ReadAsString(); 

            // 인스턴스 생성
            var endpoint = Environment.GetEnvironmentVariable("AOAI_Endpoint");
            var credential = Environment.GetEnvironmentVariable("AOAI_ApiKey");
            var deploymentId = Environment.GetEnvironmentVariable("AOAI_DeploymentId");

            using (var httpClient = new HttpClient())
            {
                // httpClient 생성 및 apikey 추가
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {credential}");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Azure Function");

                // API 요청 데이터 생성
                var reqbody = new
                {
                    model = deploymentId,
                    messages = new[]
                    {
                        new {role = "system", content =  "You are a helpful assistant. You are very good at summarizing the given text into 2-3 bullet points."},
                        new {role = "user", content = prompt}
                    },
                    
                    max_tokens = 800,
                    temperature = 0.7f,
                };

                var content = new StringContent(JsonConvert.SerializeObject(reqbody), Encoding.UTF8, "application/json");

                // API 호출
                var responseGPT = httpClient.PostAsync(endpoint, content).Result;
                string message = responseGPT.Content.ReadAsStringAsync().Result;

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

                response.WriteString(message);

                return response;
            }
        }
    }
}
