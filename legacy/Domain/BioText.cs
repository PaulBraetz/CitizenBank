namespace TaskforceGenerator.Domain.Authentication.Requests;
/// <summary>
/// Represents a citizens public bio text.
/// </summary>
[MacroRecord(Options = RecordOptions.Default ^ RecordOptions.ToString)]
[Field(typeof(String), "_value",
    Visibility = Visibility.Private,
    Options = FieldOptions.Deconstructable)]
public readonly partial struct BioText
{
    /// <inheritdoc/>
    public override String ToString() => $"BioText(Value: {StringUtils.Ellipsis(_value)})";
}