namespace CitizenBank.Persistence;

using CitizenBank.Features.Authentication;

class PasswordParameterNumericsEntity
{
    public required Int32 Iterations { get; set; }
    public required Int32 DegreeOfParallelism { get; set; }
    public required Int32 MemorySize { get; set; }
    public required Int32 OutputLength { get; set; }
    public PasswordParameterNumerics ToPasswordParameterNumerics() =>
        new(Iterations: Iterations,
            DegreeOfParallelism: DegreeOfParallelism,
            MemorySize: MemorySize,
            OutputLength: OutputLength);
    public static PasswordParameterNumericsEntity FromPasswordParameterNumerics(PasswordParameterNumerics numerics)
    {
        ArgumentNullException.ThrowIfNull(numerics);

        return new()
        {
            Iterations = numerics.Iterations,
            DegreeOfParallelism = numerics.DegreeOfParallelism,
            MemorySize = numerics.MemorySize,
            OutputLength = numerics.OutputLength
        };
    }
}