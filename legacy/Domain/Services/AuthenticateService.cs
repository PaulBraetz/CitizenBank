namespace TaskforceGenerator.Domain.Authentication.Services;
using TaskforceGenerator.Domain.Authentication.Requests;
using TaskforceGenerator.Domain.Authentication.Results;

/// <summary>
/// Service for authenticating passwords against passwords.
/// </summary>
public sealed class AuthenticateService : IService<Authenticate, OneOf<ServiceResult, PasswordMismatchResult>>
{
    private readonly IService<CreatePassword, OneOf<Password, PasswordCreationGuidelineViolatedResult>> _passwordService;
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="passwordService">The service to use when creating a password based on the clear password passed for authentication.</param>
    public AuthenticateService(IService<CreatePassword, OneOf<Password, PasswordCreationGuidelineViolatedResult>> passwordService)
    {
        _passwordService = passwordService;
    }

    /// <inheritdoc/>
    public async ValueTask<OneOf<ServiceResult, PasswordMismatchResult>> Execute(Authenticate command)
    {
        if(command.CancellationToken.IsCancellationRequested)
        {
            return ServiceResult.CompliantlyCancelled;
        }

        var (citizen, clearPassword, token) = command;
        var createPasswordResult = await new CreatePassword(
            clearPassword,
            citizen.Password.Parameters,
            false,
            token).Using(_passwordService);

        var match = createPasswordResult.Match(
            p => p.Hash.SequenceEqual(citizen.Password.Hash),
            v => false);

        OneOf<ServiceResult, PasswordMismatchResult> result = match ?
            ServiceResult.Completed :
            PasswordMismatchResult.Instance;

        return result;
    }
}
