namespace CitizenBank.Features;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

using CitizenBank.Features.Authentication;

using RhoMicro.ApplicationFramework.Composition;

[FakeService]
internal sealed partial class DbFake
{
    public ConcurrentDictionary<CitizenName, RegistrationRequest> RegistrationRequests { get; } = [];
    public ConcurrentDictionary<CitizenName, Registration> Registrations { get; } = [];
}
