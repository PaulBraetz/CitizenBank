namespace CitizenBank.Features.Authentication.Login;
using System.Threading.Tasks;

using CitizenBank.Infrastructure;

using Microsoft.EntityFrameworkCore;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Composition;

partial class LoadRegistrationRequestService(CitizenBankContext context)
{
    [ServiceMethodImplementation(Request = typeof(LoadRegistrationRequest), Service = typeof(ILoadRegistrationRequestService))]
    async ValueTask<LoadRegistrationRequest.Result> LoadRegistrationRequest(CitizenName name, CancellationToken ct)
    {
        var nameString = name.AsString;
        LoadRegistrationRequest.Result result = await context.RegistrationRequests.SingleOrDefaultAsync(r => r.Name == nameString, ct) is { } r
            ? r.ToRegistrationRequest()
            : new LoadRegistrationRequest.NotFound();

        return result;
    }
}
