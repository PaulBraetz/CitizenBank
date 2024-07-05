namespace TaskforceGenerator.Domain.Authentication.Requests;
/// <summary>
/// Command for committing a connections new bio code.
/// </summary>
/// <param name="CitizenName">The name of the citizen whose connections bio code to set.</param>
/// <param name="Code">The code to set the connections bio code to.</param>
/// <param name="CancellationToken">The token used to signal the service execution to be cancelled.</param>
public readonly record struct CommitBioCodeChange(
    String CitizenName,
    BioCode Code,
    CancellationToken CancellationToken) : IServiceRequest;
