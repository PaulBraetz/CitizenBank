namespace CitizenBank.Features.Shared;

using RhoMicro.ApplicationFramework.Presentation.Views.Blazor;
using RhoMicro.ApplicationFramework.Presentation.Views.Blazor.Abstractions;
using RhoMicro.ApplicationFramework.Presentation.Views.Blazor.Components.Primitives;

class InputControlCssStyle : IInputControlCssStyle
{
    sealed class CssStyle : ICssStyle
    {
        public required CssClassNames ClassNames { get; init; }
    }
    public ICssStyle ValidStyle { get; } = new CssStyle()
    {
        ClassNames = CssClassNames.Create("text-green-600")
    };
    public ICssStyle InvalidStyle { get; } = new CssStyle()
    {
        ClassNames = CssClassNames.Create("text-red-600")
    };
    public ICssStyle NoneValidityStyle { get; } = new CssStyle()
    {
        ClassNames = CssClassNames.Create("text-black")
    };
    public CssClassNames ClassNames { get; } = CssClassNames.Empty;
}
