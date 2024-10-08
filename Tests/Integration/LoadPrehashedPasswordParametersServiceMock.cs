﻿namespace Tests.Integration;

using System.Threading;
using System.Threading.Tasks;

using CitizenBank.Features.Authentication;
using CitizenBank.Features.Authentication.Login;
using CitizenBank.Features.Shared;

using RhoMicro.ApplicationFramework.Composition;

[FakeService]
sealed class LoadPrehashedPasswordParametersServiceMock(Func<CitizenName, LoginType, CancellationToken, ValueTask<LoadPrehashedPasswordParameters.Result>> impl) : ILoadPrehashedPasswordParametersService
{
    public LoadPrehashedPasswordParametersServiceMock(LoadPrehashedPasswordParameters.Result result) : this((_, _, _) => ValueTask.FromResult(result)) { }
    public ValueTask<LoadPrehashedPasswordParameters.Result> LoadPrehashedPasswordParameters(CitizenName name, LoginType loginType, CancellationToken cancellationToken = default) =>
        impl.Invoke(name, loginType, cancellationToken);
}
