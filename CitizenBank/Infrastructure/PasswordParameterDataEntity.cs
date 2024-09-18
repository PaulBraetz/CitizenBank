namespace CitizenBank.Infrastructure;

using CitizenBank.Features.Authentication;

class PasswordParameterDataEntity
{
    public required Byte[] AssociatedData { get; set; }
    public required Byte[] KnownSecret { get; set; }
    public required Byte[] Salt { get; set; }
    public PasswordParameterData ToPasswordParameterData() =>
        new(AssociatedData: [.. AssociatedData],
            KnownSecret: [.. KnownSecret],
            Salt: [.. Salt]);
    public static PasswordParameterDataEntity FromPasswordParameterData(PasswordParameterData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return new()
        {
            AssociatedData = [.. data.AssociatedData],
            KnownSecret = [.. data.KnownSecret],
            Salt = [.. data.Salt]
        };
    }
}
