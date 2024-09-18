namespace CitizenBank.Features.Authentication.Register;

using System.Threading.Tasks;

using CitizenBank.Infrastructure;

using RhoMicro.ApplicationFramework.Aspects;

partial class CreateBioCodeService(CitizenBankContext context)
{
    const String _alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    [ServiceMethodImplementation(Request = typeof(CreateBioCode), Service = typeof(ICreateBioCodeService))]
    async ValueTask<BioCode> CreateBioCode(CitizenName name, CancellationToken ct)
    {
        var buffer = new Char[16];
        BioCode code;
        Boolean exists;
        do
        {
            for(var i = 0; i < buffer.Length; i++)
                buffer[i] = _alphabet[Random.Shared.Next(_alphabet.Length)];

            code = new String(buffer);
            exists = await context.RegistrationRequests.FindAsync([name.AsString], ct) 
                is { BioCode: { } existingBioCode }
                && existingBioCode == code.Value;
        } while(exists);

        return code;
    }
}
