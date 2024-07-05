namespace TaskforceGenerator.Domain.Authentication.Formatters;
using TaskforceGenerator.Domain.Authentication.Requests;

/// <summary>
/// Password-hiding implementation for formatting instances of <see cref="CreatePassword"/>.
/// </summary>
public sealed class CreatePasswordFormatter : IStaticFormatter<CreatePassword>
{
    private readonly IStaticFormatter<PasswordParameters> _parametersFormatter;
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="parametersFormatter">The formatter used to format the parameters contained in values.</param>
    public CreatePasswordFormatter(IStaticFormatter<PasswordParameters> parametersFormatter)
    {
        _parametersFormatter = parametersFormatter;
    }

    /// <inheritdoc/>
    public String Format(CreatePassword value)
    {
        var result =
            $"{nameof(CreatePassword)} " +
            $"{{ " +
                $"{nameof(CreatePassword.ClearPassword)} = ***, " +
                $"{nameof(CreatePassword.Parameters)} = {_parametersFormatter.Format(value.Parameters)} " +
            $"}}";

        return result;
    }
}
