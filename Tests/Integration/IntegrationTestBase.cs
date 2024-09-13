namespace Tests.Integration;

using System.Diagnostics.CodeAnalysis;

using CitizenBank.Features.Authentication.Login.Server;
using CitizenBank.Features.Authentication.Register.Client;
using CitizenBank.Features.Authentication.Register.Server;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;

using RhoMicro.ApplicationFramework.Common.Abstractions;
using RhoMicro.ApplicationFramework.Composition;

using SimpleInjector;
using SimpleInjector.Lifestyles;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public abstract class IntegrationTestBase
{
    protected class MockRegistrationContext
    {
        public IPasswordGuideline PasswordGuideline { get; set; } =
            new PasswordGuidelineMock(static _ => throw CreateNSE<PasswordGuidelineMock>());
        static NotSupportedException CreateNSE<TMock>() => new($"Explicitly register an instance of {typeof(TMock).Name}.");
    }
    protected TService GetService<TService>(IComposer composer, Action<MockRegistrationContext>? configure = null)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(composer);

        var container = new Container();
        container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;
        //container.Options.AllowOverridingRegistrations = true;

        composer.Compose(container);
        PresentationComposers.Models.Compose(container);
        container.Register<IJSRuntime, FakeJsRuntime>(Lifestyle.Singleton);
        container.RegisterInstance<ILogger>(NullLogger.Instance);

        var mockCtx = new MockRegistrationContext();
        configure?.Invoke(mockCtx);
        container.RegisterInstance(mockCtx.PasswordGuideline);

        container.Register<IService<ServerRegister, ServerRegister.Result>, ServerRegisterService>();
        container.Register<IService<ServerLogin, ServerLogin.Result>, ServerLoginService>();
        container.Register<IService<LoadPrehashedPasswordParameters, LoadPrehashedPasswordParameters.Result>, LoadPrehashedPasswordParametersService>();

        var result = AsyncScopedLifestyle.BeginScope(container)
            .GetRequiredService<TService>();

        return result;
    }
}
