namespace CitizenBank.Features.Authentication.Register.Server;

using CitizenBank.Features.Authentication;

using System.Threading.Tasks;
using RhoMicro.ApplicationFramework.Aspects;

sealed partial class ServerRegisterService(
    ICreatePasswordParametersService parametersService,
    IHashPasswordService hashingService,
    ICreateBioCodeService bioCodeService,
    IPersistRegistrationRequestService persistService)
{
    [ServiceMethod]
    async ValueTask<ServerRegister.Result> ServerRegister(CitizenName name, PrehashedPassword password, CancellationToken ct)
    {
        var parameters = await parametersService.CreatePasswordParameters(ct);
        var hashedPw = await hashingService.HashPassword(password, parameters, ct);
        var bioCode = await bioCodeService.CreateBioCode(name, ct);
        var persistResult = await persistService.PersistRegistrationRequest(name, hashedPw, bioCode, ct);
        var result = persistResult.Match<ServerRegister.Result>(
            onCreateSuccess: s => new ServerRegister.CreateSuccess(bioCode),
            onOverwriteSuccess: s => new ServerRegister.OverwriteSuccess(bioCode),
            onFailure: f => f);

        return result;
    }
}
