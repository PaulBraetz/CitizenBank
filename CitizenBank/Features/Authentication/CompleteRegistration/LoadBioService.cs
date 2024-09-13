namespace CitizenBank.Features.Authentication.CompleteRegistration;
using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Composition;

[FakeService]
sealed partial class LoadBioService
{
    [ServiceMethod(ServiceInterfaceName = "ILoadBioService")]
    ValueTask<LoadBio.Result> LoadBio(CitizenName name, CancellationToken ct)
    {
        return ValueTask.FromResult((LoadBio.Result)new Bio(""));
    }
}
