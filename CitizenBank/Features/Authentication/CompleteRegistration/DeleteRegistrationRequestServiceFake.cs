namespace CitizenBank.Features.Authentication.CompleteRegistration;
using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Composition;

[FakeService]
sealed partial class DeleteRegistrationRequestServiceFake(DbFake db)
{
    [ServiceMethod(ServiceInterfaceName = "IDeleteRegistrationRequestService")]
    ValueTask<DeleteRegistrationRequest.Result> DeleteRegistrationRequest(CitizenName name, CancellationToken ct)
    {
        DeleteRegistrationRequest.Result result = db.RegistrationRequests.Remove(name, out _)
            ? new DeleteRegistrationRequest.Success()
            : new DeleteRegistrationRequest.DeleteRegistrationRequestFailure();

        return ValueTask.FromResult(result);
    }
}
