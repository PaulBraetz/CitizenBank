namespace CitizenBank.Features.Authentication.Register.Server;

using CitizenBank.Features.Authentication;

using System.Threading.Tasks;
using RhoMicro.ApplicationFramework.Aspects;

public partial class ServerRegisterService(
    IValidatePrehashedPasswordParametersService validateService,
    ICreatePasswordParametersService parametersService,
    IHashPasswordService hashingService,
    ICreateBioCodeService bioCodeService,
    IPersistRegistrationRequestService persistService)
{
    [ServiceMethod]
    async ValueTask<ServerRegister.Result> ServerRegister(CitizenName name, PrehashedPassword password, CancellationToken ct)
    {
        var validationResult = await validateService.ValidatePrehashedPasswordParameters(password.Parameters, ct);
        if(validationResult.TryAsInsecure(out var f))
            return f;

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
