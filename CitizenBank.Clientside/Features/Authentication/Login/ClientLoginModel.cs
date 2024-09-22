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
        //ISelectInputGroupModel<PrehashedPasswordParametersSource, String> parameterSource,
        IButtonModel login,
        IClientLoginService loginService)
    {
        Name = name;
        Password = password;
        //ParameterSource = parameterSource;
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

        Login.Label = "Login";
        Login.Clicked += OnLoginClicked;

        UpdateLoginDisabled();
    }

    private readonly IClientLoginService _loginService;

    public IInputGroupModel<CitizenName, DoesCitizenExist.DoesNotExist> Name { get; }
    public IInputGroupModel<ClearPassword, ValidatePassword.Mismatch> Password { get; }
    //public ISelectInputGroupModel<PrehashedPasswordParametersSource, String> ParameterSource { get; }
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
            parametersSource: PrehashedPasswordParametersSource.RegistrationRequest,
            cancellationToken: args.CancellationToken);

        Result = result;
        if(result.TryAsPasswordMismatch(out var passwordMismatch))
        {
            Password.Input.SetInvalid(passwordMismatch);
        } else
        {
            Password.Input.UnsetValidity();
        }
        
        if(result.TryAsCitizenDoesNotExist(out var citizenDoesNotExist))
        {
            Name.Input.SetInvalid(citizenDoesNotExist);
        } else
        {
            Name.Input.UnsetValidity();
        }
    }
}
