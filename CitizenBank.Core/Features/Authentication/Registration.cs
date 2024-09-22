namespace CitizenBank.Features.Authentication;

using CitizenBank.Features.Shared;

public sealed record Registration(CitizenName Name, HashedPassword Password);
