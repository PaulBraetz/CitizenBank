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
        IInputGroupModel<ClearPassword, ValidatePassword.Mismatch> password,
        IButtonModel login,
        IClientLoginService loginService)
    {
        Name = name;
        Password = password;
        Login = login;

        _loginService = loginService;

        Name.Label = "Name";
        Name.PropertyValueChanged += (_, _) => Result = Optional<ClientLogin.Result>.None();
        Name.Input.Entered += OnLoginClicked;

        Password.Label = "Password";
        Password.PropertyValueChanged += (_, _) => Result = Optional<ClientLogin.Result>.None();
        Name.Input.Entered += OnLoginClicked;

        Login.Label = "Login";
        Login.Clicked += OnLoginClicked;
    }

    private readonly IClientLoginService _loginService;

    public IInputGroupModel<CitizenName, String> Name { get; }
    public IInputGroupModel<ClearPassword, ValidatePassword.Mismatch> Password { get; }
    public IButtonModel Login { get; }
    private Optional<ClientLogin.Result> _result = Optional<ClientLogin.Result>.None();
    public Optional<ClientLogin.Result> Result
    {
        get => _result;
        private set => base.ExchangeBackingField(ref _result, value);
    }

    private async Task OnLoginClicked(Object? _, IAsyncEventArguments args)
    {
        var result = await _loginService.ClientLogin(
            name: Name.Input.Value,
            password: Password.Input.Value,
            cancellationToken: args.CancellationToken);

        Result = result;
        if(result.TryAsMismatch(out var passwordMismatch))
        {
            Password.Input.SetInvalid(passwordMismatch);
        } else
        {
            Password.Input.UnsetValidity();
        }
    }
}
