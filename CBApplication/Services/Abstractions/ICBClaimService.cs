using PBApplication.Requests.Abstractions;
using PBApplication.Responses.Abstractions;
using PBApplication.Services.Abstractions;
using PBCommon.Encryption;
using PBCommon.Encryption.Abstractions;
using PBData.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBApplication.Services.Abstractions
{
    public interface ICBClaimService : IService
    {
        sealed class GetClaimsData : EncryptableBase<Guid>
        {
            public IEnumerable<IClaimEntity> Claims { get; set; }
            public IEnumerable<IClaimEntity> HeldClaims { get; set; }

            protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
            {
                await Claims.SafeDecrypt(decryptor);
                await HeldClaims.SafeDecrypt(decryptor);
            }

            protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
            {
                await Claims.SafeEncrypt(encryptor);
                await HeldClaims.SafeEncrypt(encryptor);
            }
        }
        sealed class GetClaimsRequestParameter : EncryptableBase<Guid>
        {
            public Guid EntityId { get; set; }

            protected override async Task DecryptSelf(IDecryptor<Guid> decryptor)
            {
                EntityId = await decryptor.Decrypt(EntityId);
            }

            protected override async Task EncryptSelf(IEncryptor<Guid> encryptor)
            {
                EntityId = await encryptor.Encrypt(EntityId);
            }
        }
        Task<IEncryptableResponse<GetClaimsData>> GetClaims(IAsUserEncryptableRequest<GetClaimsRequestParameter> request);
    }
}
