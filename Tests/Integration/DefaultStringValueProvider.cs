namespace Tests.Integration;
using RhoMicro.ApplicationFramework.Presentation.Models.Abstractions;

class DefaultStringValueProvider : IDefaultValueProvider<String>
{
    public String GetDefault() => "";
}
