namespace CitizenBank.Features.Authentication.Register;

using CitizenBank.Features.Authentication;

using System.Threading.Tasks;

using RhoMicro.ApplicationFramework.Aspects;
using CitizenBank.Features.Shared;

partial class ServerRegisterService(
    IDoesCitizenExistService doesCitizenExistService,
    IValidatePrehashedPasswordParametersService validateService,
    ICreatePasswordParametersService parametersService,
    IHashPasswordService hashingService,
    ICreateBioCodeService bioCodeService,
    IPersistRegistrationRequestService persistService)
{
    [ServiceMethodImplementation(Request = typeof(ServerRegister), Service = typeof(IServerRegisterService))]
    async ValueTask<ServerRegister.Result> ServerRegister([Intercept] CitizenName name, PrehashedPassword password, CancellationToken ct)
    {
        var existsResult = await doesCitizenExistService.DoesCitizenExist(name, ct);
        if(existsResult.TryAsDoesNotExist(out var dne))
            return dne;

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
