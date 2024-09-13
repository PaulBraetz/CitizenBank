namespace CitizenBank.Features.Authentication.Register.Server;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Composition;

[FakeService]
sealed partial class PersistRegistrationRequestService(DbFake repository)
{
    [ServiceMethod(ServiceInterfaceName = "IRegistrationRequestPersister")]
    PersistRegistrationRequest.Result PersistRegistrationRequest(CitizenName name, HashedPassword password, BioCode bioCode)
    {
        var newRequest = new RegistrationRequest(name, password, bioCode);
        var added = repository.RegistrationRequests.AddOrUpdate(name, newRequest, (_, _) => newRequest);

        PersistRegistrationRequest.Result result = Object.ReferenceEquals(newRequest, added)
            ? new PersistRegistrationRequest.OverwriteSuccess()
            : new PersistRegistrationRequest.CreateSuccess();

        return result;
    }
}
