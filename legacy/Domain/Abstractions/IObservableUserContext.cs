namespace TaskforceGenerator.Domain.Authentication.Abstractions;
/// <summary>
/// Represents an observable view onto the context in which the user is acting.
/// </summary>
public interface IObservableUserContext
{
    /// <summary>
    /// Gets the citizen using which the user is performing transfers and account management.
    /// </summary>
    ICitizenConnection? Citizen { get; }
    /// <summary>
    /// Invoked after <see cref="Citizen"/> has changed.
    /// </summary>
    event EventHandler<PropertyValueChangeArgs<ICitizenConnection?>>? CitizenChanged;
}
