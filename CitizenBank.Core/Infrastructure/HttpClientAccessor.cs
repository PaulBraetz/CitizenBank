namespace CitizenBank.Infrastructure;

using System.Net.Http;

public class HttpClientAccessor(IHttpClientFactory factory)
{
    public virtual HttpClient Client => factory.CreateClient();
}
sealed class HttpClientAccessor<T>(IHttpClientFactory factory) : HttpClientAccessor(factory)
{
    public override HttpClient Client => typeof(T).FullName is { } n
        ? factory.CreateClient(n)
        : factory.CreateClient();
}