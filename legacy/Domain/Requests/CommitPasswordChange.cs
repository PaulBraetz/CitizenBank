namespace TaskforceGenerator.Domain.Authentication.Requests;
/// <summary>
/// Command for setting a citizens connection password.
/// </summary>
/// <param name="CitizenName">The name of the citizen whose connection password to set.</param>
/// <param name="Code">The bio code used for optimistic locking of the citizen.</param>
/// <param name="Password">The password to set the citizens password to.</param>
/// <param name="CancellationToken">The token used to signal the service execution to be cancelled.</param>
public readonly record struct CommitPasswordChange(String CitizenName, BioCode Code, Password Password, CancellationToken CancellationToken) : IServiceRequest;
