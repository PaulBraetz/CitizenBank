namespace CitizenBank.Features.Authentication.Login;

using System.Threading.Tasks;

using CitizenBank.Features.Shared;
using CitizenBank.Persistence;

using Microsoft.EntityFrameworkCore;

using RhoMicro.ApplicationFramework.Aspects;

partial class LoadRegistrationService(CitizenBankContext context)
{
    [ServiceMethodImplementation(Request = typeof(LoadRegistration), Service = typeof(ILoadRegistrationService))]
    async ValueTask<LoadRegistration.Result> LoadRegistration(CitizenName name, CancellationToken ct)
    {
        var nameString = name.Value;
        LoadRegistration.Result result = await context.Registrations.SingleOrDefaultAsync(r => r.Name == nameString, ct) is { } r
            ? r.ToRegistration()
            : new LoadRegistration.DoesNotExist();

        return result;
    }
}
