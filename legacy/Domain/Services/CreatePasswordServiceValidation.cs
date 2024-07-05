namespace TaskforceGenerator.Domain.Authentication.Services;

using TaskforceGenerator.Domain.Authentication.Requests;
using TaskforceGenerator.Domain.Authentication.Results;

/// <summary>
/// Decorator for validating <see cref="CreatePassword"/> executions.
/// </summary>
public sealed class CreatePasswordServiceValidation : IService<CreatePassword, OneOf<Password, PasswordCreationGuidelineViolatedResult>>
{
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="passwordGuideline">The password guideline to apply to clear passwords provided.</param>
    /// <param name="decorated">The decorated service.</param>
    public CreatePasswordServiceValidation(
        IPasswordGuideline passwordGuideline,
        IService<CreatePassword, OneOf<Password, PasswordCreationGuidelineViolatedResult>> decorated)
    {
        _passwordGuideline = passwordGuideline;
        _decorated = decorated;
    }

    private readonly IPasswordGuideline _passwordGuideline;
    private readonly IService<CreatePassword, OneOf<Password, PasswordCreationGuidelineViolatedResult>> _decorated;

    /// <inheritdoc/>
    public ValueTask<OneOf<Password, PasswordCreationGuidelineViolatedResult>> Execute(CreatePassword command)
    {
        var validity = command.IsInitialPassword ?
            PasswordValidity.Empty :
            _passwordGuideline.Assess(command.ClearPassword);

        if(validity.RulesViolated.Length > 0)
        {
            return ValueTask
                .FromResult<OneOf<Password, PasswordCreationGuidelineViolatedResult>>(
                    new PasswordCreationGuidelineViolatedResult(validity));
        }

        if(command.Parameters.Numerics.Iterations < 1)
        {
            throw new Exception($"Must have at least one iteration when hashing passwords.");
        }

        var result = _decorated.Execute(command);

        return result;
    }
}
