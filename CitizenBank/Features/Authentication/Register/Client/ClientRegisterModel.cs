namespace CitizenBank.Features.Authentication.Register.Client;
using System;
using System.Threading.Tasks;

using RhoMicro.ApplicationFramework.Common;
using RhoMicro.ApplicationFramework.Common.Abstractions;
using RhoMicro.ApplicationFramework.Presentation.Models.Abstractions;

public sealed class ClientRegisterModel : HasObservableProperties
{
    public ClientRegisterModel(
        IInputGroupModel<CitizenName, String> name,
        IInputGroupModel<ClearPassword, PasswordValidity> password,
        IDisplayModel<Optional<ClientRegister.Result>> result,
        IButtonModel register,
        IClientRegisterService registerService)
    {
        Name = name;
        Password = password;
        Result = result;
        Register = register;

        _registerService = registerService;

        Name.Label = "Name";
        Password.Label = "Password";
        Register.Label = "Register";
        Register.Clicked += OnRegisterClicked;
    }

    private readonly IClientRegisterService _registerService;

    public IInputGroupModel<CitizenName, String> Name { get; }
    public IInputGroupModel<ClearPassword, PasswordValidity> Password { get; }
    public IButtonModel Register { get; }
    public IDisplayModel<Optional<ClientRegister.Result>> Result { get; }

    private async Task OnRegisterClicked(Object? _, IAsyncEventArguments args)
    {
        var result = await _registerService.ClientRegister(
                name:Name.Input.Value,
                password: Password.Input.Value,
                cancellationToken: args.CancellationToken);

        Result.Value = result;
        if(result.TryAsViolatedGuidelines(out var violatedGuidelines))
        {
            Password.Input.SetInvalid(violatedGuidelines);
        } else
        {
            Password.Input.UnsetValidity();
        }
    }
}
