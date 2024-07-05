namespace TaskforceGenerator.Domain.Authentication;
using TaskforceGenerator.Domain.Authentication.Abstractions;

/// <summary>
/// Default implementation of <see cref="IUserContext"/> and <see cref="IObservableUserContext"/>.
/// </summary>
public sealed class UserContext : IUserContext, IObservableUserContext
{
    private ICitizenConnection? _citizen;

    /// <inheritdoc/>
    public ICitizenConnection? Citizen
    {
        get => _citizen;
        set
        {
            var oldValue = _citizen;
            _citizen = value;
            CitizenChanged?.Invoke(
                this,
                new(
                    oldValue: oldValue,
                    newValue: value,
                    nameof(Citizen)
                ));
        }
    }
    /// <inheritdoc/>
    public event EventHandler<PropertyValueChangeArgs<ICitizenConnection?>>? CitizenChanged;
}
