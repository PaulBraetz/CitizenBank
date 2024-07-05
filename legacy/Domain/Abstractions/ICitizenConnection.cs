namespace TaskforceGenerator.Domain.Authentication.Abstractions;

/// <summary>
/// Represents a citizen that has been connected to the application and able to verify a password set.
/// </summary>
public interface ICitizenConnection
{
    /// <summary>
    /// Gets the citizens name.
    /// </summary>
    public String CitizenName { get; }
    /// <summary>
    /// The code currently expected to be provided by operations requiring bio verification.
    /// This is the primary means by which a citizens connection between RSI and the application will be established.
    /// </summary>
    public BioCode Code { get; }
    /// <summary>
    /// Gets the password authenticating logins etc. for this connection.
    /// </summary>
    public Password Password { get; }
}
