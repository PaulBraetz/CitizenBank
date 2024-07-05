namespace CitizenBank.Features.Authentication.Register.Server;
using RhoMicro.ApplicationFramework.Common.Abstractions;

sealed class ServerRegisterFormatter : IStaticFormatter<ServerRegister>
{
    public String Format(ServerRegister value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var result = $"ServerRegister {{ Name: {value.Name}, Password: *** }}";

        return result;
    }
}
