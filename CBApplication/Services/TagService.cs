using CBApplication.Requests.Abstractions;
using CBApplication.Services.Abstractions;

using CBCommon.Extensions;

using CBData.Abstractions;
using CBData.Entities;

using NHibernate.Linq.Functions;
using PBApplication.Context.Abstractions;
using PBApplication.Extensions;
using PBApplication.Responses;
using PBApplication.Responses.Abstractions;
using PBCommon;
using PBCommon.Extensions;
using PBCommon.Validation;
using PBData.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using static CBApplication.Services.Abstractions.IEventfulAccountService;
using static CBApplication.Services.Abstractions.IEventfulTagService;
using static CBApplication.Services.Abstractions.ITagService;

namespace CBApplication.Services.Public
{
	sealed class TagService : CBService, IEventfulTagService
	{
		public TagService(IServiceContext serviceContext) : base(serviceContext)
		{
			Observe<IEventfulTagService>(this);
		}

		public async Task<IGetPaginatedEncryptableResponse<SearchTagModel>> SearchTags(IAsAccountGetPaginatedEncryptableRequest<SearchTagsParameter> request)
		{
<<<<<<< HEAD

=======
>>>>>>> 830ac890b7424c0df58250177e3a5dc71afc2934

			var response = new GetPaginatedEncryptableResponse<SearchTagModel>();

			async Task notNullRequest()
			{
				var data = Connection.Query<TagEntity>();

				if (request.Parameter.Name != null)
				{
					var name = request.Parameter.Name.ToLower();
					data = data.Where(t => t.Name.ToLower().Contains(name));
				}
				if (request.Parameter.ExcludeIds?.Any() ?? false)
				{
					data = data.Where(t => !request.Parameter.ExcludeIds.Contains(t.Id));
				}
				if (request.Parameter.ExcludeTexts?.Any() ?? false)
				{
					data = data.Where(t => !request.Parameter.ExcludeTexts.Contains(t.Name));
				}

				var priorityTagsIds = request.Parameter.PriorityTagsProviderId.HasValue ?
					Connection.GetSingle<IHasPriorityTags>(request.Parameter.PriorityTagsProviderId.Value).PriorityTags.Select(t => t.Id) :
					Array.Empty<Guid>();

				if (priorityTagsIds.Any())
				{
					data = data.OrderBy(t => priorityTagsIds.Contains(t.Id));
				}

				void setData()
				{
					response.LastPage = data.GetPageCount(request.PerPage) - 1;
					response.Data = data.Paginate(request.PerPage, request.Page)
						.Select(a => a.CloneAsT())
						.Select(t => new SearchTagModel(t, priorityTagsIds.Contains(t.Id)))
						.ToList();
				}

				await CachedCriterionChain.Cache.Get()
					.ThisValidatePagination(request, data)
					.SetOnCriterionMet(setData)
					.Evaluate(response);
			}

			await FirstParameterizedRequestNullCheck(request, response)
				.SetOnCriterionMet(notNullRequest)
				.CatchAll(ValidationField.Create(nameof(request)))
				.Evaluate(response);

			return response;
		}

		public async Task<TagEntity> GetTag(String name)
		{
<<<<<<< HEAD

=======
>>>>>>> 830ac890b7424c0df58250177e3a5dc71afc2934

			TagEntity run()
			{
				name = name?.Trim().ToLower();
				if (String.IsNullOrEmpty(name) || !name.IsValidTagName())
				{
					name = String.Intern("NoTag");
				}
				TagEntity retVal = Connection.GetSingle<TagEntity>(e => e.Name.Equals(name));
				if (retVal == null)
				{
					name = String.Intern($"#{name}");
					retVal = new TagEntity(name);
					Connection.Insert(retVal);
					Connection.SaveChanges();
				}
				return retVal;
			}
			return await Task.Run(run);
		}
	}
}
