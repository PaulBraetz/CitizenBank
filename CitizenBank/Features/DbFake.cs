namespace CitizenBank.Features;
using System.Collections.Concurrent;

using CitizenBank.Features.Authentication;

using RhoMicro.ApplicationFramework.Composition;

[FakeService]
internal sealed class DbFake
{
    public ConcurrentDictionary<CitizenName, RegistrationRequest> RegistrationRequests { get; } = [];
    public ConcurrentDictionary<CitizenName, Registration> Registrations { get; } = [];
}
