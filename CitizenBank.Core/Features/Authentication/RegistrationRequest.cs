namespace CitizenBank.Features.Authentication;

public sealed record RegistrationRequest(CitizenName Name, HashedPassword Password, BioCode BioCode);

