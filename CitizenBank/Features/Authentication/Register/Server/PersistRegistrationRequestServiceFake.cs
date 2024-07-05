namespace CitizenBank.Features.Authentication.Register.Server;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Composition;

[FakeService]
sealed partial class PersistRegistrationRequestServiceFake
{
    [ServiceMethod]
#pragma warning disable IDE0060 // Remove unused parameter
    static PersistRegistrationRequest.Result PersistRegistrationRequest(CitizenName name, HashedPassword password, BioCode bioCode) => new CreateSuccess();
#pragma warning restore IDE0060 // Remove unused parameter
}