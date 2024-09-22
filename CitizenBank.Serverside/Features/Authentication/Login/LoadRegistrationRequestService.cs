namespace CitizenBank.Features.Authentication.Login;
using System.Threading.Tasks;

using CitizenBank.Features.Shared;
using CitizenBank.Persistence;

using Microsoft.EntityFrameworkCore;

using RhoMicro.ApplicationFramework.Aspects;

partial class LoadRegistrationRequestService(CitizenBankContext context)
{
    [ServiceMethodImplementation(Request = typeof(LoadRegistrationRequest), Service = typeof(ILoadRegistrationRequestService))]
    async ValueTask<LoadRegistrationRequest.Result> LoadRegistrationRequest(CitizenName name, CancellationToken ct)
    {
        var nameString = name.Value;
        LoadRegistrationRequest.Result result = await context.RegistrationRequests.SingleOrDefaultAsync(r => r.Name == nameString, ct) is { } r
            ? r.ToRegistrationRequest()
            : new LoadRegistrationRequest.NotFound();

        return result;
    }
}
