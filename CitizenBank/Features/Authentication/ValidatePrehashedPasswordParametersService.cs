namespace CitizenBank.Features.Authentication;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Common;
using RhoMicro.ApplicationFramework.Composition;

[FakeService]
partial class ValidatePrehashedPasswordParametersService
{
    [ServiceMethodImplementation(Request = typeof(ValidatePrehashedPasswordParameters), Service = typeof(IValidatePrehashedPasswordParametersService))]
    static ValidatePrehashedPasswordParameters.Result ValidatePrehashedPasswordParameters(PrehashedPasswordParameters parameters) =>
        //parameters.HashSize >= PrehashedPasswordMinimumParameters.HashSize
        //&& parameters.Iterations >= PrehashedPasswordMinimumParameters.Iterations
        //&& parameters.Prf >= PrehashedPasswordMinimumParameters.Prf
        //&& parameters.Salt.Length >= PrehashedPasswordMinimumParameters.SaltLength
        //? new Success()
        //: new ValidatePrehashedPasswordParameters.Insecure();
        new Success();
}
