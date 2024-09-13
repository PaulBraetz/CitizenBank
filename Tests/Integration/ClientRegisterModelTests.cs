namespace Tests.Integration;

using CitizenBank.Composition;
using CitizenBank.Features.Authentication;
using CitizenBank.Features.Authentication.Login.Server;
using CitizenBank.Features.Authentication.Register.Client;
using CitizenBank.Features.Authentication.Register.Server;

using Microsoft.AspNetCore.Cryptography.KeyDerivation;

using RhoMicro.ApplicationFramework.Composition;

public class ClientRegisterModelTests : IntegrationTestBase
{
    private static void ConfigureMockContext(MockRegistrationContext ctx)
    {
        ctx.PasswordGuideline = new PasswordGuidelineMock(static p => PasswordValidity.Empty);
        ctx.ServerRegisterService = new ServerRegisterServiceMock((r, ct) => ValueTask.FromResult<ServerRegister.Result>(new ServerRegister.CreateSuccess(new BioCode(""))));
        ctx.LoadPrehashedPasswordParametersService = new LoadPrehashedPasswordParametersServiceMock(static (r, ct) =>
        {
            var salt = new Byte[512];
            new Random(0).NextBytes(salt);
            var result = ValueTask.FromResult<LoadPrehashedPasswordParameters.Result>(new PrehashedPasswordParameters(
                Salt: [.. salt],
                HashSize: 512,
                Prf: KeyDerivationPrf.HMACSHA256,
                Iterations: 100));

            return result;
        });
    }
    [Fact]
    public async Task LocalClientRegister_yields_success_on_new_register()
    {
        var model = GetService<ClientRegisterModel>(Composers.LocalClient,ConfigureMockContext);
        model.Name.Input.Value = "SleepWellPupper";
        model.Password.Input.Value = "SuperSecretPassword1.";
        await model.Register.Click(default);
        Assert.True(model.Result.IsSome);
        Assert.True(model.Result.AsSome.IsCreateSuccess);
    }
    [Fact]
    public async Task LocalClientRegister_yields_overwrite_success_on_duplicate_register()
    {
        var model = GetService<ClientRegisterModel>(
            Composers.LocalClient,
            ConfigureMockContext);
        model.Name.Input.Value = "SleepWellPupper";
        model.Password.Input.Value = "SuperSecretPassword1.";
        await model.Register.Click(default);
        await model.Register.Click(default);
        Assert.True(model.Result.IsSome);
        Assert.True(model.Result.AsSome.IsOverwriteSuccess);
    }
}