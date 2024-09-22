namespace Tests.Integration;

using System.Net.Http;

using CitizenBank.Persistence;

sealed class HttpClientAccessorFake() : HttpClientAccessor(new HttpClientFactoryImpl())
{
    sealed class HttpClientFactoryImpl: IHttpClientFactory
    {
        public HttpClient CreateClient(String name) => throw new NotSupportedException("Http client is not accessible in tests.");
    }
}
