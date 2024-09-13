namespace CitizenBank.Features.Authentication.Login.Server;
using System.Threading.Tasks;

using CitizenBank.Features.Authentication.Infrastructure;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Composition;

[FakeService]
sealed partial class LoadRegistrationRequestService(CitizenBankContext context)
{
    [ServiceMethod(ServiceInterfaceName = "ILoadRegistrationRequestService")]
    ValueTask<LoadRegistrationRequest.Result> LoadRegistrationRequest(CitizenName name, CancellationToken ct)
    {
        var nameString = name.AsString;
        LoadRegistrationRequest.Result result = context.RegistrationRequests.SingleOrDefault(r => r.Name == nameString) is { } r
            ? r.ToRegistrationRequest()
            : new LoadRegistrationRequest.NotFound();

        return ValueTask.FromResult(result);
    }
}
