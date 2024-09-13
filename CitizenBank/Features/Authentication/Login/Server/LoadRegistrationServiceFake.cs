namespace CitizenBank.Features.Authentication.Login.Server;

using System.Threading.Tasks;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Composition;

[FakeService]
sealed partial class LoadRegistrationService(DbFake db)
{
    [ServiceMethod(ServiceInterfaceName = "ILoadRegistrationService")]
    ValueTask<LoadRegistration.Result> LoadRegistration(CitizenName name)
    {
        LoadRegistration.Result result = db.Registrations.TryGetValue(name, out var r)
            ? r
            : new LoadRegistration.DoesNotExist();

        return ValueTask.FromResult(result);
    }
}
