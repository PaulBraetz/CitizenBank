namespace CitizenBank.Features.Authentication.Register;
using System;
using System.Threading.Tasks;

using CitizenBank.Features.Shared;

using RhoMicro.ApplicationFramework.Common;
using RhoMicro.ApplicationFramework.Common.Abstractions;
using RhoMicro.ApplicationFramework.Presentation.Models.Abstractions;

public sealed class ClientRegisterModel : HasObservableProperties
{
    public ClientRegisterModel(
        IInputGroupModel<CitizenName, DoesCitizenExist.DoesNotExist> name,
        IInputGroupModel<ClearPassword, PasswordValidity> password,
        IButtonModel register,
        IClientRegisterService registerService)
    {
        Name = name;
        Password = password;
        Register = register;

        _registerService = registerService;

        Name.Label = "Name";
        Name.Input.PropertyValueChanged += (_, args) =>
        {
            UpdateRegisterDisabled();
            if(args.PropertyName == nameof(Name.Input.Value))
            {
                Result = Optional.None<ClientRegister.Result>();
            }
        };
        Name.Input.Entered += OnRegisterClicked;

        Password.Label = "Password";
        Password.Input.PropertyValueChanged += (_, args) =>
        {
            UpdateRegisterDisabled();
            if(args.PropertyName == nameof(Password.Input.Value))
            {
                Result = Optional.None<ClientRegister.Result>();
            }
        };
        Name.Input.Entered += OnRegisterClicked;

        Register.Label = "Register";
        Register.Clicked += OnRegisterClicked;

        UpdateRegisterDisabled();
    }

    private readonly IClientRegisterService _registerService;

    public IInputGroupModel<CitizenName, DoesCitizenExist.DoesNotExist> Name { get; }
    public IInputGroupModel<ClearPassword, PasswordValidity> Password { get; }
    public IButtonModel Register { get; }
    private Optional<ClientRegister.Result> _result = Optional.None<ClientRegister.Result>();
    public Optional<ClientRegister.Result> Result
    {
        get => _result;
        private set => base.ExchangeBackingField(ref _result, value);
    }

    private void UpdateRegisterDisabled() =>
        Register.Disabled = Name.Input.Value.Value.Length == 0 || Password.Input.Value.AsString.Length == 0;
    private async Task OnRegisterClicked(Object? _, IAsyncEventArguments args)
    {
        var result = await _registerService.ClientRegister(
            name: Name.Input.Value,
            password: Password.Input.Value,
            cancellationToken: args.CancellationToken);

        Result = result;
        if(result.TryAsViolatedGuidelines(out var violatedGuidelines))
        {
            Password.Input.SetInvalid(violatedGuidelines);
        } else
        {
            Password.Input.UnsetValidity();
        }
    }
}
