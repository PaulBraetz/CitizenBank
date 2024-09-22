namespace CitizenBank.Composition;
using CitizenBank.Persistence;

using RhoMicro.ApplicationFramework.Composition;

using SimpleInjector;
using SimpleInjector.Integration.ServiceCollection;

/// <summary>
/// Contains core object graph composers.
/// </summary>
public static class CoreComposers
{
    /// <summary>
    /// Gets the core composer instance.
    /// </summary>
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
    /// <summary>
    /// Gets the core Simpleinjector integration event handler.
    /// </summary>
    public static Action<SimpleInjectorAddOptions> SimpleinjectorAddHandler { get; } = o => { };
}
