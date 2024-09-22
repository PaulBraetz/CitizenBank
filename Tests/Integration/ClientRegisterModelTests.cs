namespace Tests.Integration;
using CitizenBank.Features.Shared;
using CitizenBank.Features.Authentication.Register;

public class ClientRegisterModelTests : IntegrationTestBase
{
    [Fact]
    public async Task LocalClientRegister_yields_success_on_new_register()
    {
        using var scope = CreateService<ClientRegisterModel>(
            (typeof(IDoesCitizenExistService), new DoesCitizenExistServiceMock(new DoesCitizenExist.Success())));
        var model = scope.Service;

        model.Name.Input.Value = "SleepWellPupper";
        model.Password.Input.Value = "SuperSecretPassword1.";
        await model.Register.Click(default);
        Assert.True(model.Result.IsSome);
        Assert.True(model.Result.AsSome.IsCreateSuccess);
    }
    [Fact]
    public async Task LocalClientRegister_yields_failure_on_nonexisting_name()
    {
        using var scope = CreateService<ClientRegisterModel>(
            (typeof(IDoesCitizenExistService), new DoesCitizenExistServiceMock(new DoesCitizenExist.DoesNotExist())));
        var model = scope.Service;

        model.Name.Input.Value = "SleepWellPupper";
        model.Password.Input.Value = "SuperSecretPassword1.";
        await model.Register.Click(default);
        Assert.True(model.Result.IsSome);
        Assert.True(model.Result.AsSome.IsCitizenDoesNotExist);
    }
    [Fact]
    public async Task LocalClientRegister_yields_failure_on_nonexisting_name_from_server()
    {
        using var scope = CreateService<ClientRegisterModel>(
            (typeof(IDoesCitizenExistService), new DoesCitizenExistServiceMock(new DoesCitizenExist.Success())),
            (typeof(IServerRegisterService), new ServerRegisterServiceMock(new DoesCitizenExist.DoesNotExist())));
        var model = scope.Service;

        model.Name.Input.Value = "SleepWellPupper";
        model.Password.Input.Value = "SuperSecretPassword1.";
        await model.Register.Click(default);
        Assert.True(model.Result.IsSome);
        Assert.True(model.Result.AsSome.IsCitizenDoesNotExist);
    }
    [Fact]
    public async Task LocalClientRegister_yields_violated_rules_on_invalid_password()
    {
        IPasswordRule rule = new NeverMatchingPasswordRuleMock();

        using var scope = CreateService<ClientRegisterModel>(
            (typeof(IPasswordGuideline), new PasswordGuideline([rule])),
            (typeof(IDoesCitizenExistService), new DoesCitizenExistServiceMock(new DoesCitizenExist.Success())));
        var model = scope.Service;

        model.Name.Input.Value = "SleepWellPupper";
        model.Password.Input.Value = "SuperSecretPassword1.";
        await model.Register.Click(default);
        Assert.True(model.Result.IsSome);
        Assert.True(model.Result.AsSome.IsViolatedGuidelines);
        Assert.Equal(model.Result.AsSome.AsViolatedGuidelines.RulesViolated, new[] { rule });
    }
    [Fact]
    public async Task LocalClientRegister_yields_overwrite_success_on_duplicate_register()
    {
        using var scope = CreateService<ClientRegisterModel>(
            (typeof(IDoesCitizenExistService), new DoesCitizenExistServiceMock(new DoesCitizenExist.Success())));
        var model = scope.Service;

        model.Name.Input.Value = "SleepWellPupper";
        model.Password.Input.Value = "SuperSecretPassword1.";
        await model.Register.Click(default);
        await model.Register.Click(default);
        Assert.True(model.Result.IsSome);
        Assert.True(model.Result.AsSome.IsOverwriteSuccess);
    }
    [Fact]
    public void LocalClientRegister_register_button_is_disabled_if_name_input_is_empty()
    {
        using var scope = CreateService<ClientRegisterModel>();
        var model = scope.Service;

        model.Password.Input.Value = "SuperSecretPassword1.";
        Assert.True(model.Register.Disabled);
    }
    [Fact]
    public void LocalClientRegister_register_button_is_disabled_if_password_input_is_empty()
    {
        using var scope = CreateService<ClientRegisterModel>();
        var model = scope.Service;

        model.Name.Input.Value = "SleepWellPupper";
        Assert.True(model.Register.Disabled);
    }
    [Fact]
    public void LocalClientRegister_register_button_is_disabled_if_inputs_are_empty()
    {
        using var scope = CreateService<ClientRegisterModel>();
        var model = scope.Service;

        Assert.True(model.Register.Disabled);
    }
    [Fact]
    public void LocalClientRegister_register_button_is_disabled_if_inputs_are_nonempty()
    {
        using var scope = CreateService<ClientRegisterModel>();
        var model = scope.Service;

        model.Name.Input.Value = "SleepWellPupper";
        model.Password.Input.Value = "SuperSecretPassword1.";
        Assert.False(model.Register.Disabled);
    }
}