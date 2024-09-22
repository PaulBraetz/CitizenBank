namespace CitizenBank.Features.Authentication.Register;

using System.Threading.Tasks;

using CitizenBank.Features.Shared;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Common;

partial class ClientRegisterService(
    IDoesCitizenExistService doesCitizenExistService,
    IValidatePasswordAgainstGuidelineService guidelineService,
    ICreatePrehashedPasswordParametersService parametersService,
    IPrehashPasswordService hashService,
    IServerRegisterService registerService)
{
    [ServiceMethodImplementation(Request = typeof(ClientRegister), Service = typeof(IClientRegisterService))]
    async ValueTask<ClientRegister.Result> ClientRegister(CitizenName name, ClearPassword password, CancellationToken ct)
    {
        var exists = await doesCitizenExistService.DoesCitizenExist(name, ct);
        if(exists.TryAsDoesNotExist(out var doesNotExist))
            return doesNotExist;

        var validity = await guidelineService.ValidatePasswordAgainstGuideline(password, ct);

        if(!validity.IsValid)
            return validity;

        var parameters = await parametersService.CreatePrehashedPasswordParameters(ct);
        var prehashed = await hashService.PrehashPassword(password, parameters, ct);
        var registerResult = await registerService.ServerRegister(name, prehashed, ct);
        var result = registerResult.Match<ClientRegister.Result>(
            onCreateSuccess: s => new ClientRegister.CreateSuccess(s.BioCode),
            onOverwriteSuccess: s => new ClientRegister.OverwriteSuccess(s.BioCode),
            onInsecure: i => new Failure("Insecure prehash parameters detected. This is likely due to an out of date client. In order to fix this, use the latest client and re-register your citizen."),
            onDoesNotExist: f => f,
            onFailure: f => f);

        return result;
    }
}
