namespace CitizenBank.Features.Authentication.Login.Client;
using System;
using System.Threading.Tasks;

using RhoMicro.ApplicationFramework.Common;
using RhoMicro.ApplicationFramework.Common.Abstractions;
using RhoMicro.ApplicationFramework.Presentation.Models.Abstractions;

public sealed class ClientLoginModel : HasObservableProperties
{
    public ClientLoginModel(
        IInputGroupModel<CitizenName, String> name,
        IInputGroupModel<ClearPassword, ValidatePassword.PasswordMismatch> password,
        IButtonModel login,
        IClientLoginService loginService)
    {
        Name = name;
        Password = password;
        Login = login;

        _loginService = loginService;

        Name.Label = "Name";
        Name.PropertyValueChanged += (_, _) => Result = Optional<ClientLogin.Result>.None();

        Password.Label = "Password";
        Password.PropertyValueChanged += (_, _) => Result = Optional<ClientLogin.Result>.None();

        Login.Label = "Login";
        Login.Clicked += OnRegisterClicked;
    }

    private readonly IClientLoginService _loginService;

    public IInputGroupModel<CitizenName, String> Name { get; }
    public IInputGroupModel<ClearPassword, ValidatePassword.PasswordMismatch> Password { get; }
    public IButtonModel Login { get; }
    private Optional<ClientLogin.Result> _result = Optional<ClientLogin.Result>.None();
    public Optional<ClientLogin.Result> Result
    {
        get => _result;
        private set => base.ExchangeBackingField(ref _result, value);
    }

    private async Task OnRegisterClicked(Object? _, IAsyncEventArguments args)
    {
        var result = await _loginService.ClientLogin(
            name: Name.Input.Value,
            password: Password.Input.Value,
            cancellationToken: args.CancellationToken);

        Result = result;
        if(result.TryAsPasswordMismatch(out var passwordMismatch))
        {
            Password.Input.SetInvalid(passwordMismatch);
        } else
        {
            Password.Input.UnsetValidity();
        }
    }
}
