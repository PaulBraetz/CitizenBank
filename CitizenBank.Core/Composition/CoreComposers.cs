namespace CitizenBank.Composition;

using System.Net.Http;

using CitizenBank.Features.Authentication;
using CitizenBank.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using RhoMicro.ApplicationFramework.Composition;

using SimpleInjector;
using SimpleInjector.Integration.ServiceCollection;

/// <summary>
/// Contains template composers.
/// </summary>
public static class CoreComposers
{
    public static IComposer Instance { get; } = Composer.Create(c =>
    {
        c.RegisterConditional(
            typeof(HttpClientAccessor),
            ctx => ctx.Consumer != null
                ? typeof(HttpClientAccessor<>).MakeGenericType(ctx.Consumer!.ImplementationType)
                : typeof(HttpClientAccessor),
            Lifestyle.Singleton,
            ctx => !ctx.Handled);
    });
    public static Action<SimpleInjectorAddOptions> SimpleinjectorAddHandler { get; } = o => { };
}
