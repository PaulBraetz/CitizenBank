namespace CitizenBank.Features.Authentication.Register.Client;

using System.Threading.Tasks;

using CitizenBank.Features.Authentication.Register.Server;

using RhoMicro.ApplicationFramework.Aspects;

sealed partial class ClientRegisterService(
    IValidatePasswordAgainstGuidelineService guidelineService,
    IPrehashPasswordService hashService,
    IServerRegisterService registerService)
{
    [ServiceMethod]
    async ValueTask<ClientRegister.Result> ClientRegister(CitizenName name, ClearPassword password, CancellationToken ct)
    {
        var validity = await guidelineService.ValidatePasswordAgainstGuideline(password, ct);

        if(!validity.IsValid)
            return validity;

        var prehash = await hashService.PrehashPassword(password, ct);

        var registerResult = await registerService.ServerRegister(name, prehash, ct);
        var result = registerResult.Match<ClientRegister.Result>(
            onCreateSuccess: s => new ClientRegister.CreateSuccess(s.BioCode),
            onOverwriteSuccess: s => new ClientRegister.OverwriteSuccess(s.BioCode),
            onFailure: f => f);

        return result;
    }
}
