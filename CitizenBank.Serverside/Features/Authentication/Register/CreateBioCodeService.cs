namespace CitizenBank.Features.Authentication.Register;

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using CitizenBank.Features.Shared;
using CitizenBank.Persistence;

using RhoMicro.ApplicationFramework.Aspects;

partial class CreateBioCodeService(CitizenBankContext context)
{
    const String _alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    [ServiceMethodImplementation(Request = typeof(CreateBioCode), Service = typeof(ICreateBioCodeService))]
    [SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "Not security relevant.")]
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
            exists = await context.RegistrationRequests.FindAsync([name.Value], ct) 
                is { BioCode: { } existingBioCode }
                && existingBioCode == code.Value;
        } while(exists);

        return code;
    }
}
