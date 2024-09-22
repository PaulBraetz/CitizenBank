namespace Tests.Integration;

using System.Threading;

using CitizenBank.Features.Shared;

sealed class DoesCitizenExistServiceMock(Func<CitizenName, CancellationToken, ValueTask<DoesCitizenExist.Result>> impl) : IDoesCitizenExistService
{
    public DoesCitizenExistServiceMock(DoesCitizenExist.Result result) : this((_, _) => ValueTask.FromResult(result)) { }
    public ValueTask<DoesCitizenExist.Result> DoesCitizenExist(CitizenName name, CancellationToken cancellationToken = default) => impl.Invoke(name, cancellationToken);
}
