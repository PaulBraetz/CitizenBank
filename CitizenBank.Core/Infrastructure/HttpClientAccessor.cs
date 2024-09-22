namespace CitizenBank.Persistence;

using System.Net.Http;

public class HttpClientAccessor(IHttpClientFactory factory)
{
    protected IHttpClientFactory Factory => factory;
    public virtual HttpClient Client => Factory.CreateClient();
}
sealed class HttpClientAccessor<T>(IHttpClientFactory factory) : HttpClientAccessor(factory)
{
    public override HttpClient Client => typeof(T).FullName is { } n
        ? Factory.CreateClient(n)
        : Factory.CreateClient();
}