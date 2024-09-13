namespace CitizenBank.Features.Authentication.Login.Server;

using System.Threading.Tasks;

using CitizenBank.Features.Authentication.Infrastructure;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Composition;

[FakeService]
sealed partial class LoadRegistrationService(CitizenBankContext context)
{
    [ServiceMethod(ServiceInterfaceName = "ILoadRegistrationService")]
    ValueTask<LoadRegistration.Result> LoadRegistration(CitizenName name)
    {
        var nameString = name.AsString;
        LoadRegistration.Result result = context.Registrations.SingleOrDefault(r => r.Name == nameString) is { } r
            ? r.ToRegistration()
            : new LoadRegistration.DoesNotExist();

        return ValueTask.FromResult(result);
    }
}
