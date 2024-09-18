namespace Tests.Integration;

using System.Threading;

using CitizenBank.Features;
using CitizenBank.Features.Authentication;

sealed class DoesCitizenExistServiceMock(Func<CitizenName, CancellationToken, ValueTask<DoesCitizenExist.Result>> impl) : IDoesCitizenExistService
    {
        public ValueTask<DoesCitizenExist.Result> DoesCitizenExist(CitizenName name, CancellationToken cancellationToken = default) => impl.Invoke(name, cancellationToken);
    }
