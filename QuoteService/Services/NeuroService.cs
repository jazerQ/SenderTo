using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using QuoteService.Services.JsonModels;
using QuoteService.Settings;

namespace QuoteService.Services;

public class NeuroService(IHttpClientFactory factory, IOptionsMonitor<DeepSeekSettings> options)
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly string _model = "deepseek/deepseek-r1-0528:free";
    
    public async Task<string> GetQuote()
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", options.CurrentValue.Token);
        var msg = new Message()
        {
            role = "user",
            content = "Напиши философскую мысль, ничего лишнего просто философская мысль 10 слов. Один вариант, одна мысль, твое сообщение должно быть длинной максимум 80 символов, не пиши количество символов"
        };

        var requestModel = new ChatRequest()
        {
            model = _model,
            messages = new List<Message>()
        };
        requestModel.messages.Add(msg);

        string json = JsonConvert.SerializeObject(requestModel);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync(options.CurrentValue.BaseUrl, content);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var mainText = JsonDocument.Parse(responseString).RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content").GetString();
        return mainText;
    }
}