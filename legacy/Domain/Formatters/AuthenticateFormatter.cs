namespace TaskforceGenerator.Domain.Authentication.Formatters;
using TaskforceGenerator.Domain.Authentication.Abstractions;
using TaskforceGenerator.Domain.Authentication.Requests;

/// <summary>
/// Password-hiding implementation for formatting instances of <see cref="Authenticate"/>.
/// </summary>
public sealed class AuthenticateFormatter : IStaticFormatter<Authenticate>
{
    private readonly IStaticFormatter<ICitizenConnection> _connectionFormatter;
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="connectionFormatter">The formatter used to format the connection contained in values.</param>
    public AuthenticateFormatter(IStaticFormatter<ICitizenConnection> connectionFormatter)
    {
        _connectionFormatter = connectionFormatter;
    }

    /// <inheritdoc/>
    public String Format(Authenticate value)
    {
        var result =
            $"{nameof(Authenticate)} " +
            $"{{ " +
                $"{nameof(Authenticate.ClearPassword)} = ***, " +
                $"{nameof(Authenticate.Connection)} = {_connectionFormatter.Format(value.Connection)} " +
            $"}}";

        return result;
    }
}
