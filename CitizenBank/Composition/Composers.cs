namespace CitizenBank.Composition;

using System.Threading.Tasks;

using CitizenBank.Features;
using CitizenBank.Features.Authentication;
using CitizenBank.Features.Authentication.Login.Client;
using CitizenBank.Features.Authentication.Login.Server;
using CitizenBank.Features.Authentication.Register.Client;
using CitizenBank.Features.Authentication.Register.Server;

using Microsoft.AspNetCore.Cryptography.KeyDerivation.PBKDF2;
using Microsoft.Extensions.Logging;

using RhoMicro.ApplicationFramework.Common;
using RhoMicro.ApplicationFramework.Common.Abstractions;
using RhoMicro.ApplicationFramework.Common.Environment;
using RhoMicro.ApplicationFramework.Composition;
using RhoMicro.ApplicationFramework.Presentation.Models.Abstractions;
using RhoMicro.ApplicationFramework.Presentation.Views.Blazor.Components;
using RhoMicro.ApplicationFramework.Presentation.Views.Blazor.Components.Primitives;

using SimpleInjector;

/// <summary>
/// Contains template composers.
/// </summary>
public static class Composers
{
    sealed class UnsupportedService<TRequest, TResult> : IService<TRequest, TResult>
        where TRequest : IRequest<TResult>
    {
        public ValueTask<TResult> Execute(TRequest request, CancellationToken _) =>
            throw new NotSupportedException($"{typeof(IService<TRequest, TResult>)} is not supported.");
    }
    private static IComposer Core { get; } = Composer.Create(c =>
    {
        c.RegisterSingleton<IInputControlCssStyle, InputControlCssStyle>();

        c.Register<IClipboardModel, JsClipboardModel>();

        c.Register<IInputModel<CitizenName, String>, CitizenNameInputModel>();
        c.RegisterSingleton<IDefaultValueProvider<CitizenName>, CitizenNameDefaultProvider>();
        c.RegisterSingleton<IDefaultValueProvider<ClearPassword>, ClearPasswordDefaultValueProvider>();

        c.Register<ClientLoginModel>();
        c.Register<IInputModel<ClearPassword, ValidatePassword.Mismatch>, Features.Authentication.Login.Client.ClearPasswordInputModel>();
        c.RegisterSingleton<IDefaultValueProvider<ValidatePassword.Mismatch>, PasswordMismatchDefaultValueProvider>();

        c.Register<ClientRegisterModel>();
        c.RegisterSingleton<IDefaultValueProvider<Optional<ClientRegister.Result>>, OptionalClientRegisterResultDefaultValueProvider>();
        c.Register<IInputModel<ClearPassword, PasswordValidity>, Features.Authentication.Register.Client.ClearPasswordInputModel>();
        c.RegisterSingleton<IDefaultValueProvider<PasswordValidity>, PasswordValidityDefaultValueProvider>();

#if DEBUG
        c.RegisterInstance<IEnvironmentConfiguration>(EnvironmentConfiguration.Development);
#else
        c.RegisterInstance<IEnvironmentConfiguration>(EnvironmentConfiguration.Production);
#endif
    });

    //Server requires these as stubs when verifying ui components (mainly)
    static readonly HashSet<Type> _requiredUnsupportedServerServices = [
        typeof(IService<ClientRegister, ClientRegister.Result>),
        typeof(IService<ClientLogin, ClientLogin.Result>)
    ];

    /// <summary>
    /// Gets the default composition root for web application servers.
    /// </summary>
    public static IComposer WebServer { get; } = Core + Composer.Create(c =>
    {
        c.RegisterSingleton<YieldingManagedPbkdf2Provider>();
        c.RegisterSingleton<DbFake>();
        c.RegisterInstance(new Yielder());

        c.RegisterServices(typeof(Composers).Assembly, options: new()
        {
            RegistrationPredicate = static ctx =>
            {
                var hasRequirednamespace = ctx.ImplementationType.Namespace == null
                    || ctx.ImplementationType.Namespace.EndsWith("server", StringComparison.OrdinalIgnoreCase)
                    || !ctx.ImplementationType.Namespace.EndsWith("client", StringComparison.OrdinalIgnoreCase);
                var isRequiredButUnsupported = _requiredUnsupportedServerServices.Contains(ctx.ServiceType);
                var result = hasRequirednamespace || isRequiredButUnsupported;

                return result;
            },
            RegistrationCallback = static ctx =>
            {
                var (info, container) = ctx;
                var serviceImpl = _requiredUnsupportedServerServices.Contains(info.ServiceType)
                    ? typeof(UnsupportedService<,>).MakeGenericType(info.ServiceType.GenericTypeArguments[0], info.ServiceType.GenericTypeArguments[1])
                    : info.ImplementationType;
                var (service, _, tradiditionalService, traditionalImpl) = info;
                container.Register(service, serviceImpl, Lifestyle.Scoped);
                container.Register(tradiditionalService, traditionalImpl, Lifestyle.Scoped);
            }
        });
    });
    //Client requires these as adapters around api clients.
    static readonly HashSet<Type> _requiredClientServicesAdapters = [
        typeof(IService<ServerRegister, ServerRegister.Result>),
        typeof(IService<ServerLogin, ServerLogin.Result>),
        ];
    private static IComposer Client { get; } = Core + Composer.Create(c =>
    {
        c.RegisterSingleton<YieldingManagedPbkdf2Provider>();
        c.RegisterSingleton<DbFake>(); //TODO: add actual fake
        c.RegisterInstance(
            new Yielder(
                yieldInterval: TimeSpan.FromMilliseconds(100),
                yieldTime: TimeSpan.FromMilliseconds(100)));

        c.RegisterServices(typeof(Composers).Assembly, options: new()
        {
            RegistrationPredicate = static ctx =>
            {
                var hasRequirednamespace = ctx.ImplementationType.Namespace == null
                    || ctx.ImplementationType.Namespace.EndsWith("client", StringComparison.OrdinalIgnoreCase)
                    || !ctx.ImplementationType.Namespace.EndsWith("server", StringComparison.OrdinalIgnoreCase);
                var isRequiredButUnsupported = _requiredClientServicesAdapters.Contains(ctx.ServiceType);
                var result = hasRequirednamespace || isRequiredButUnsupported;

                return result;
            },
            RegistrationCallback = static ctx =>
            {
                var (info, container) = ctx;
                var (service, serviceImpl, tradiditionalService, traditionalImpl) = info;

                if(!_requiredClientServicesAdapters.Contains(info.ServiceType))
                {
                    container.Register(service, serviceImpl, Lifestyle.Scoped);
                }

                container.Register(tradiditionalService, traditionalImpl, Lifestyle.Scoped);
            }
        });
    });

    /// <summary>
    /// Gets the default composition root for web application clients.
    /// </summary>
    public static IComposer WebClient { get; } = Client;
    /// <summary>
    /// Gets the default composition root for local application.
    /// </summary>
    public static IComposer LocalClient { get; } = Client;
}
