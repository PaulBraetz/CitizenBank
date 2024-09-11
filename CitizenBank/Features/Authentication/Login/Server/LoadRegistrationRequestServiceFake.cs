﻿namespace CitizenBank.Features.Authentication.Login.Server;
using System.Threading.Tasks;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Composition;

[FakeService]
sealed partial class LoadRegistrationRequestServiceFake(DbFake db)
{
    [ServiceMethod(ServiceInterfaceName = "ILoadRegistrationRequestService")]
    ValueTask<LoadRegistrationRequest.Result> LoadRegistrationRequest(CitizenName name, CancellationToken ct)
    {
        LoadRegistrationRequest.Result result = db.RegistrationRequests.TryGetValue(name, out var r)
            ? r
            : new LoadRegistrationRequest.DoesNotExist();

        return ValueTask.FromResult(result);
    }
}