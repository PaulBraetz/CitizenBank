namespace CitizenBank.Features.Authentication;

using CitizenBank.Features.Shared;

public sealed record RegistrationRequest(CitizenName Name, HashedPassword Password, BioCode BioCode);

