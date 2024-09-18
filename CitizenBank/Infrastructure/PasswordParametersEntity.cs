namespace CitizenBank.Infrastructure;

using CitizenBank.Features.Authentication;

class PasswordParametersEntity
{
    public required PasswordParameterNumericsEntity Numerics { get; set; }
    public required PasswordParameterDataEntity Data { get; set; }
    public PasswordParameters ToPasswordParameters() =>
        new(Numerics: Numerics.ToPasswordParameterNumerics(),
            Data: Data.ToPasswordParameterData());
    public static PasswordParametersEntity FromPasswordParameters(PasswordParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        return new()
        {
            Data = PasswordParameterDataEntity.FromPasswordParameterData(parameters.Data),
            Numerics = PasswordParameterNumericsEntity.FromPasswordParameterNumerics(parameters.Numerics)
        };
    }
}
