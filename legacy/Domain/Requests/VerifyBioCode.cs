namespace TaskforceGenerator.Domain.Authentication.Requests;
/// <summary>
/// Query for verifying a bio code.
/// </summary>
/// <param name="RequiredCode">The code expected to match the citizens bio and system entity.</param>
/// <param name="Name">The name of the citizen whose bio code to assert.</param>
/// <param name="CancellationToken">The token used to signal the service execution to be cancelled.</param>
public readonly record struct VerifyBioCode(
String RequiredCode,
String Name,
    CancellationToken CancellationToken) : 
    IServiceRequest<OneOf<VerifyBioCodeResult, CitizenNotRegisteredResult, CitizenNotExistingResult>>
{
}
/// <summary>
/// The result of the query.
/// </summary>
public enum VerifyBioCodeResult
{
    /// <summary>
    /// Indicates that the required code matches against both the bio and system entity.
    /// </summary>
    Match,
    /// <summary>
    /// Indicates that the required code does not match the system entities code.
    /// </summary>
    CitizenMismatch,
    /// <summary>
    /// Indicates that the required code does not match the bio retrieved for the citizen.
    /// </summary>
    BioMismatch
}
