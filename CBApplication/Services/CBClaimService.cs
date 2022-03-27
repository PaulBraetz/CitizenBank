using CBApplication.Services.Abstractions;
using PBApplication.Context.Abstractions;
using PBApplication.Extensions;
using PBApplication.Requests.Abstractions;
using PBApplication.Responses;
using PBApplication.Responses.Abstractions;
using PBCommon.Validation;
using PBData.Abstractions;
using PBData.Extensions;
using System;
using System.Threading.Tasks;

namespace CBApplication.Services
{
    public sealed class CBClaimService : CBService, ICBClaimService
    {
        public CBClaimService(IServiceContext serviceContext) : base(serviceContext)
        {
            Observe<ICBClaimService>(this);
        }

        public async Task<IEncryptableResponse<ICBClaimService.GetClaimsData>> GetClaims(IAsUserEncryptableRequest<ICBClaimService.GetClaimsRequestParameter> request)
        {
            var response = new EncryptableResponse<ICBClaimService.GetClaimsData>();

            async Task requestNotNull()
            {
                var entity = Connection.GetSingle<IEntity>(request.Parameter.EntityId);
                var user = GetUserEntity(request);
                Boolean mayObserve()
                {
                    return user.MayObserveClaims(Connection,entity);
                }

                void onSuccess()
                {
                    response.Data.Claims = entity.GetClaims(Connection);
                    response.Data.HeldClaims = entity.GetHeldClaims(Connection);

                    LogIfAccessingAsDelegate(user, "Observed claims for entity {0}", entity.Id.ToString());
                }

                await FirstValidateAuthenticatedDelegate(request, response)
                    .NextNullCheck(entity,
                        ValidationField.Create(nameof(request.Parameter.EntityId)),
                        ValidationCode.NotFound.WithMessage("The entity requested could not be found."))
                    .NextCompound(mayObserve,
                        response.Validation.GetField(nameof(request.Parameter.EntityId)),
                        ValidationCode.Unauthorized.WithMessage("You are not authorized to observe claims for this entity."))
                    .SetOnCriterionMet(onSuccess)
                    .Evaluate(response);
            }
            await FirstParameterizedRequestNullCheck(request, response)
                .SetOnCriterionMet(requestNotNull)
                .Evaluate(response);

            return response;
        }
    }
}
