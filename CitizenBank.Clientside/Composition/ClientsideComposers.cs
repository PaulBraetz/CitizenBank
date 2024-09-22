namespace CitizenBank.Composition;

using CitizenBank.Features.Authentication;
using CitizenBank.Features.Authentication.Login;
using CitizenBank.Features.Authentication.Register;
using CitizenBank.Features.Shared;

using Microsoft.AspNetCore.Cryptography.KeyDerivation.PBKDF2;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using RhoMicro.ApplicationFramework.Common;
using RhoMicro.ApplicationFramework.Composition;
using RhoMicro.ApplicationFramework.Presentation.Models;
using RhoMicro.ApplicationFramework.Presentation.Models.Abstractions;
using RhoMicro.ApplicationFramework.Presentation.Views.Blazor.Components;
using RhoMicro.ApplicationFramework.Presentation.Views.Blazor.Components.Primitives;

using SimpleInjector.Integration.ServiceCollection;

/// <summary>
/// Contains core DI composers.
/// </summary>
public static class ClientsideComposers
{
    public static IComposer Default { get; } = CoreComposers.Instance + Composer.Create(c =>
    {
        c.RegisterSingleton<YieldingManagedPbkdf2Provider>();
        c.RegisterInstance(
            new Yielder(
                yieldInterval: TimeSpan.FromMilliseconds(100),
                yieldTime: TimeSpan.FromMilliseconds(100)));

        c.RegisterSingleton<IInputControlCssStyle, InputControlCssStyle>();

        c.Register<IInputModel<LoginType, LoadPrehashedPasswordParameters.Failure>, LoginTypeInputModel>();
        c.RegisterInstance<IDefaultValueProvider<LoginType>>(
            new DefaultValueProvider<LoginType>(() => LoginType.Regular));
        c.RegisterInstance<IDefaultValueProvider<LoadPrehashedPasswordParameters.Failure>>(
            new DefaultValueProvider<LoadPrehashedPasswordParameters.Failure>(() => new LoadPrehashedPasswordParameters.RegistrationNotFound()));

        c.Register<IInputModel<CitizenName, DoesCitizenExist.DoesNotExist>, CitizenNameInputModel>();
        c.RegisterInstance<IDefaultValueProvider<CitizenName>>(
            new DefaultValueProvider<CitizenName>(() => CitizenName.Empty));
        c.RegisterInstance<IDefaultValueProvider<DoesCitizenExist.DoesNotExist>>(
            new DefaultValueProvider<DoesCitizenExist.DoesNotExist>(() => new()));
        c.RegisterInstance<IDefaultValueProvider<ClearPassword>>(
            new DefaultValueProvider<ClearPassword>(() => ClearPassword.Empty));

        c.Register<ClientLoginModel>();
        c.Register<IInputModel<ClearPassword, ValidatePassword.Mismatch>, Features.Authentication.Login.ClearPasswordInputModel>();
        c.RegisterInstance<IDefaultValueProvider<ValidatePassword.Mismatch>>(
            new DefaultValueProvider<ValidatePassword.Mismatch>(() => new()));

        c.Register<ClientRegisterModel>();
        c.RegisterInstance<IDefaultValueProvider<Optional<ClientRegister.Result>>>(
            new DefaultValueProvider<Optional<ClientRegister.Result>>(() => Optional.None<ClientRegister.Result>()));
        c.Register<IInputModel<ClearPassword, PasswordValidity>, Features.Authentication.Register.ClearPasswordInputModel>();
        c.RegisterInstance<IDefaultValueProvider<PasswordValidity>>(
            new DefaultValueProvider<PasswordValidity>(() => PasswordValidity.Empty));
    });
    /// <summary>
    /// Gets the default composition root for web application clients.
    /// </summary>
    public static IComposer WebClient { get; } = Default + Composer.Create(c =>
    {
        c.Register<IClipboardModel, JsClipboardModel>();
    });
    /// <summary>
    /// Gets the default composition root for local application.
    /// </summary>
    public static IComposer LocalClient { get; } = Default + Composer.Create(c =>
    {
        c.Register<IClipboardModel, ClipboardModel>();
    });
    public static Action<SimpleInjectorAddOptions> SimpleinjectorAddHandler { get; } =
        o =>
        {
            _ = o.Services
                .AddTransient<IPasswordGuideline>(sp =>
                    sp.GetRequiredService<IOptions<RegexPasswordGuideline>>().Value
                    ?? throw new InvalidOperationException($"Unable to resolve password guidelines."))
                .AddOptions<RegexPasswordGuideline>()
                .BindConfiguration("PasswordGuideline")
                .Validate(
                    o => true,
                    "")
                .ValidateOnStart();
        };
}
