namespace Tests.Integration;

using System.Security.Cryptography;

using CitizenBank.Features.Authentication;
using CitizenBank.Features.Authentication.Login;
using CitizenBank.Features.Shared;
using CitizenBank.Persistence;

using Microsoft.AspNetCore.Cryptography.KeyDerivation;

public class ClientLoginModelTests : IntegrationTestBase
{
    [Fact]
    public async Task LocalClientLogin_yields_failure_on_nonexisting_name()
    {
        using var scope = CreateService<ClientLoginModel>(
            (typeof(IDoesCitizenExistService), new DoesCitizenExistServiceMock(new DoesCitizenExist.DoesNotExist())));
        var model = scope.Service;

        model.Name.Input.Value = "SleepWellPupper";
        model.Password.Input.Value = "SuperSecretPassword1.";
        await model.Login.Click(default);
        Assert.True(model.Result.IsSome);
        Assert.True(model.Result.AsSome.IsCitizenDoesNotExist);
    }
    [Fact]
    public async Task LocalClientLogin_yields_failure_on_nonexisting_name_from_server()
    {
        using var scope = CreateService<ClientLoginModel>(
            (typeof(IDoesCitizenExistService), new DoesCitizenExistServiceMock(new DoesCitizenExist.Success())),
            (typeof(IServerLoginService), new ServerLoginServiceMock((ServerLogin.Failure)new DoesCitizenExist.DoesNotExist())),
            (typeof(ILoadPrehashedPasswordParametersService), new LoadPrehashedPasswordParametersServiceMock(
                new PrehashedPasswordParameters(RandomNumberGenerator.GetBytes(16), 16, KeyDerivationPrf.HMACSHA512, 16))));
        var model = scope.Service;

        model.Name.Input.Value = "SleepWellPupper";
        model.Password.Input.Value = "SuperSecretPassword1.";
        await model.Login.Click(default);
        Assert.True(model.Result.IsSome);
        Assert.True(model.Result.AsSome.IsCitizenDoesNotExist);
    }
}
