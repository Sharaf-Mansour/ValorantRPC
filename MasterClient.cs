namespace ValorantDRPC;

internal class MasterClient
{
     readonly  Uri BaseAddress;
    HttpClient httpClient => new ();
    
    public MasterClient(string Url) => BaseAddress = new(Url);

    public async ValueTask<T?> GetAsync<T>(Dictionary<string,string> keyValues)
    {
        foreach(var item in keyValues)
        httpClient.DefaultRequestHeaders.Add(item.Key,item.Value);

        var Response = await httpClient.GetAsync(BaseAddress);

         return JsonSerializer.Deserialize<T>(await Response.Content.ReadAsStringAsync());
    }
    public async ValueTask<T?> PutAsync<T>(Dictionary<string, string> keyValues, Dictionary<string, string> body)
    {
        foreach (var item in keyValues)
            httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);

        HttpContent httpContent = new FormUrlEncodedContent(body);
        var Response = await httpClient.PutAsync(BaseAddress, httpContent);

        return JsonSerializer.Deserialize<T>(await Response.Content.ReadAsStringAsync());
    }

    public async ValueTask<T?> PostAsync<T>(Dictionary<string, string> keyValues, Dictionary<string, string> body)
    {
        foreach (var item in keyValues)
            httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);

        HttpContent httpContent = new FormUrlEncodedContent(body);
        var Response = await httpClient.PostAsync(BaseAddress, httpContent);

        return JsonSerializer.Deserialize<T>(await Response.Content.ReadAsStringAsync());
    }
}
