namespace CitizenBank.Features.Authentication.Login;

using System;
using System.Threading.Tasks;

using CitizenBank.Features.Shared;

using RhoMicro.ApplicationFramework.Common;
using RhoMicro.ApplicationFramework.Common.Abstractions;
using RhoMicro.ApplicationFramework.Presentation.Models.Abstractions;

public sealed class ClientLoginModel : HasObservableProperties
{
    public ClientLoginModel(
        IInputGroupModel<CitizenName, DoesCitizenExist.DoesNotExist> name,
        IInputGroupModel<ClearPassword, ValidatePassword.Mismatch> password,
        IInputGroupModel<LoginType, LoadPrehashedPasswordParameters.Failure> loginType,
        IButtonModel login,
        IClientLoginService loginService)
    {
        Name = name;
        Password = password;
        LoginType = loginType;
        Login = login;

        _loginService = loginService;

        Name.Label = "Name";
        Name.Input.PropertyValueChanged += (_, args) =>
        {
            UpdateLoginDisabled();
            if(args.PropertyName == nameof(Name.Input.Value))
            {
                Result = Optional.None<ClientLogin.Result>();
            }
        };
        Name.Input.Entered += OnLoginClicked;

        Password.Label = "Password";
        Password.Input.PropertyValueChanged += (_, args) =>
        {
            UpdateLoginDisabled();
            if(args.PropertyName == nameof(Password.Input.Value))
            {
                Result = Optional.None<ClientLogin.Result>();
            }
        };
        Name.Input.Entered += OnLoginClicked;

        LoginType.Label = "Complete Registration";

        Login.Label = "Login";
        Login.Clicked += OnLoginClicked;

        UpdateLoginDisabled();
    }

    private readonly IClientLoginService _loginService;

    public IInputGroupModel<CitizenName, DoesCitizenExist.DoesNotExist> Name { get; }
    public IInputGroupModel<ClearPassword, ValidatePassword.Mismatch> Password { get; }
    public IInputGroupModel<LoginType, LoadPrehashedPasswordParameters.Failure> LoginType { get; }
    public IButtonModel Login { get; }
    private Optional<ClientLogin.Result> _result = Optional.None<ClientLogin.Result>();
    public Optional<ClientLogin.Result> Result
    {
        get => _result;
        private set => ExchangeBackingField(ref _result, value);
    }

    private void UpdateLoginDisabled() =>
        Login.Disabled = Name.Input.Value.Value.Length == 0 || Password.Input.Value.AsString.Length == 0;
    private async Task OnLoginClicked(Object? _, IAsyncEventArguments args)
    {
        var result = await _loginService.ClientLogin(
            name: Name.Input.Value,
            password: Password.Input.Value,
            loginType: LoginType.Input.Value,
            cancellationToken: args.CancellationToken);

        Result = result;
        Password.Input.UnsetValidity();
        Name.Input.UnsetValidity();
        LoginType.Input.UnsetValidity();
        result.Switch(
            onSuccess: _ =>
            {
                Password.Input.SetValid();
                Name.Input.SetValid();
                LoginType.Input.SetValid();
            },
            onFailure: _ => { },
            onCitizenDoesNotExist: Name.Input.SetInvalid,
            onParametersError: LoginType.Input.SetInvalid,
            onPasswordMismatch: Password.Input.SetInvalid);
    }
}
