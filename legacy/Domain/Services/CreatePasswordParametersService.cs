namespace TaskforceGenerator.Domain.Authentication.Services;
using TaskforceGenerator.Domain.Authentication.Requests;

/// <summary>
/// Service for creating new password parameters.
/// </summary>
public sealed class CreatePasswordParametersService : IService<CreatePasswordParameters, PasswordParameters>
{
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="saltLength">The amount of salt bytes to generate for new parameters.</param>
    /// <param name="numericsPrototype">The prototype to use for the numerical parameters part.</param>
    public CreatePasswordParametersService(Int32 saltLength, PasswordParameterNumerics numericsPrototype)
    {
        _saltLength = saltLength;
        _numericsPrototype = numericsPrototype;
    }

    private readonly Int32 _saltLength;
    private readonly PasswordParameterNumerics _numericsPrototype;

    /// <inheritdoc/>
    public ValueTask<PasswordParameters> Execute(CreatePasswordParameters query)
    {
        var numerics = _numericsPrototype;
        var salt = new Byte[_saltLength];
        Random.Shared.NextBytes(salt);
        var data = new PasswordParameterData(
            AssociatedData: [],
            KnownSecret: [],
            Salt: salt);
        var result = new PasswordParameters(numerics, data);

        return ValueTask.FromResult(result);
    }
}
