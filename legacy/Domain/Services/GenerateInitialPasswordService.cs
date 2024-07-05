namespace TaskforceGenerator.Domain.Authentication.Services;
using TaskforceGenerator.Domain.Authentication.Requests;
using TaskforceGenerator.Domain.Authentication.Results;

/// <summary>
/// Service for generating initial citizen passwords.
/// </summary>
public sealed class GenerateInitialPasswordService : IService<GenerateInitialPassword, Password>
{
    private readonly IService<CreatePassword, OneOf<Password, PasswordCreationGuidelineViolatedResult>> _passwordService;
    private readonly IService<CreatePasswordParameters, PasswordParameters> _parametersService;
    private readonly Int32 _generatedPasswordLength;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="passwordService">The service to use when creating a new password.</param>
    /// <param name="parametersService">The service to use when creating new password parameters.</param>
    /// <param name="generatedPasswordLength">The length of generated (random) passwords.</param>
    public GenerateInitialPasswordService(
        IService<CreatePassword, OneOf<Password, PasswordCreationGuidelineViolatedResult>> passwordService,
        IService<CreatePasswordParameters, PasswordParameters> parametersService,
        Int32 generatedPasswordLength)
    {
        _passwordService = passwordService;
        _parametersService = parametersService;
        _generatedPasswordLength = generatedPasswordLength;
    }

    /// <inheritdoc/>
    public async ValueTask<Password> Execute(GenerateInitialPassword query)
    {
        var parameters = await new CreatePasswordParameters().Using(_parametersService);
        var clearPassword = GenerateRandomPassword();
        var createResult = await new CreatePassword(
            clearPassword,
            parameters,
            IsInitialPassword: true,
            query.CancellationToken).Using(_passwordService);

        var password = createResult.Match(
            p => p,
            e => throw new Exception("The generated default password did not match password guidelines."));

        return password;
    }

    private String GenerateRandomPassword()
    {
        var chars = Enumerable
            .Repeat(0, _generatedPasswordLength)
            .Select(i => Random.Shared.Next('A', 'Z'))
            .Select(i => (Char)i);
        var result = String.Concat(chars);

        return result;
    }
}
