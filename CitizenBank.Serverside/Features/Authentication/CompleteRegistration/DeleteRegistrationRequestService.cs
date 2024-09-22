namespace CitizenBank.Features.Authentication.CompleteRegistration;

using CitizenBank.Features.Shared;
using CitizenBank.Persistence;

using Microsoft.EntityFrameworkCore;

using RhoMicro.ApplicationFramework.Aspects;

partial class DeleteRegistrationRequestService(CitizenBankContext context)
{
    [ServiceMethodImplementation(Request = typeof(DeleteRegistrationRequest), Service = typeof(IDeleteRegistrationRequestService))]
    async ValueTask<DeleteRegistrationRequest.Result> DeleteRegistrationRequest(CitizenName name, CancellationToken ct)
    {
        var nameString = name.Value;
        var request = await context.RegistrationRequests.SingleOrDefaultAsync(e => e.Name == nameString, ct);
        if(request == null)
            return new DeleteRegistrationRequest.Success(); //fail instead? request is nonexistent after call anyway

        _ = context.Remove(request);
        _ = await context.SaveChangesAsync(ct);
        
        return new DeleteRegistrationRequest.Success();
    }
}
