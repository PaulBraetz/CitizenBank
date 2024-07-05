namespace TaskforceGenerator.Domain.Authentication.Abstractions;
/// <summary>
/// Represents the context in which the user is acting.
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// Gets or sets the citizen using which the user is performing transfers and account management.
    /// </summary>
    ICitizenConnection? Citizen { get; set; }
    /// <summary>
    /// Invoked after <see cref="Citizen"/> has changed.
    /// </summary>
    event EventHandler<PropertyValueChangeArgs<ICitizenConnection?>>? CitizenChanged;
}
