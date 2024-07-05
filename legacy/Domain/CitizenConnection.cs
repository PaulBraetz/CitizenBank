namespace TaskforceGenerator.Domain.Authentication;
using TaskforceGenerator.Domain.Authentication.Abstractions;

/// <summary>
/// Default implementation of <see cref="ICitizenConnection"/>.
/// </summary>
/// <param name="CitizenName">The citizens name.</param>
/// <param name="Code">
/// The code currently expected to be provided by operations requiring bio verification.
/// This is the primary means by which a citizens connection between RSI and the application will be established.
/// </param>
/// <param name="Password">The password authenticating logins etc. for this connection.</param>
public sealed record CitizenConnection(String CitizenName, BioCode Code, Password Password) : ICitizenConnection;
