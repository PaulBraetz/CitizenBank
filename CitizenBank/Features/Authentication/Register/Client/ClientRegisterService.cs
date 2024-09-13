namespace CitizenBank.Features.Authentication.Register.Client;

using System.Threading.Tasks;

using CitizenBank.Features.Authentication.Register.Server;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Common;

sealed partial class ClientRegisterService(
    IValidatePasswordAgainstGuidelineService guidelineService,
    ICreatePrehashedPasswordParametersService parametersService,
    IPrehashPasswordService hashService,
    IServerRegisterService registerService)
{
    [ServiceMethod]
    async ValueTask<ClientRegister.Result> ClientRegister(CitizenName name, ClearPassword password, CancellationToken ct)
    {
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
            onFailure: f => f);

        return result;
    }
}
