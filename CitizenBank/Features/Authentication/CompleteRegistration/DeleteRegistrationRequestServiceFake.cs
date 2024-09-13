namespace CitizenBank.Features.Authentication.CompleteRegistration;

using CitizenBank.Features.Authentication.Infrastructure;

using Microsoft.EntityFrameworkCore;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Common;
using RhoMicro.ApplicationFramework.Composition;

[FakeService]
sealed partial class DeleteRegistrationRequestService(CitizenBankContext context)
{
    [ServiceMethod(ServiceInterfaceName = "IDeleteRegistrationRequestService")]
    async ValueTask<DeleteRegistrationRequest.Result> DeleteRegistrationRequest(CitizenName name, CancellationToken ct)
    {
        var nameString = name.AsString;
        var request = context.RegistrationRequests.SingleOrDefault(e => e.Name == nameString);
        if(request == null)
            return new DeleteRegistrationRequest.Success(); //fail instead? request is nonexistent after call anyway

        var tracker = context.Remove(request);
        _ = await context.SaveChangesAsync(ct);
        DeleteRegistrationRequest.Result result = tracker.State == EntityState.Deleted
            ? new DeleteRegistrationRequest.Success()
            : new Failure("Invalid entity state detected after removal.");

        return result;
    }
}
