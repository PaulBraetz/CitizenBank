using PBApplication.Context.Abstractions;

namespace CBApplication.Services
{
#if DEBUG
	public class SearchService : CBService
	{
		/*
		 public ISearchService.ISearchResponse<TResponse, TNamed> Search<TResponse, TNamed>(ISearchService.SearchRequestBase<TResponse, TNamed> request)
			where TResponse : IGetNamedResponse<TNamed>
			where TNamed : IHasName
		{
			if (request is ISearchService.SearchTagsRequest tagsRequest)
			{
				return (ISearchService.ISearchResponse<TResponse, TNamed>)SearchTags(tagsRequest);
			}
			else if (request is ISearchService.SearchAccountsRequestBase accountsRequest)
			{
				return (ISearchService.ISearchResponse<TResponse, TNamed>)SearchAccounts(accountsRequest);
			}
			else if (request is ISearchService.SearchCitizensRequest citizensRequest)
			{
				return (ISearchService.ISearchResponse<TResponse, TNamed>)SearchCitizens(citizensRequest);
			}
			else if (request is ISearchService.SearchDepartmentsRequestBase departmentsRequest)
			{
				return (ISearchService.ISearchResponse<TResponse, TNamed>)SearchDepartments(departmentsRequest);
			}
			else
			{
				throw new InvalidOperationException("Invalid request");
			}
		}
		public ISearchService.SearchTagsResponse SearchTags(ISearchService.SearchTagsRequest request)
		{
			ISearchService.SearchTagsResponse response = new ISearchService.SearchTagsResponse() { Results = new List<ISearchService.SearchTagModel> { } };

			List<Guid> excludeIds = request.ExcludeKeys != null ? request.ExcludeKeys.Select(k => InternalEncryptionService.IdentifiablyDecryptToId(k)).ToList() : new List<Guid> { };
			IEnumerable<String> excludeTexts = request.ExcludeTexts ?? new List<String> { };

			IHasPriorityTags priorityTagProvider = GetEntity<IHasPriorityTags>(request.PriorityTagsProviderKey);

			CachedCriterionChain.Cache.Get()
				.NextNullCheck(priorityTagProvider, ValidationField.Create(nameof(request.PriorityTagsProviderKey)), ValidationCode.NotFound)
				.ThisCompound(() => request.ResultsCount >= 1 && request.ResultsCount <= 10, ValidationField.Create(nameof(request.ResultsCount)), ValidationCode.Invalid)
				.SetOnCriterionMet(() =>
				{
					List<TagEntity> priorityTags = priorityTagProvider != null ? priorityTagProvider.PriorityTags
						.Where(t => !excludeIds.Contains(t.Id) && !excludeTexts.Contains(t.Name)).ToList() : new List<TagEntity> { };

					Boolean checkText = !String.IsNullOrWhiteSpace(request.Name);
					Int32 tagsLeft = request.ResultsCount - priorityTags.Count;
					if (checkText)
					{
						priorityTags = priorityTags.Where(t => t.Name.ToLower().Contains(request.Name.ToLower())).ToList();
					}
					priorityTags = priorityTags.Take(request.ResultsCount).ToList();
					if ((priorityTagProvider.Is<AccountEntityBase>(out AccountEntityBase account, Connection) && GetSettingsFor<AccountSettingsEntityBase, AccountEntityBase>(account).Value.ForcePriorityTags) || tagsLeft < 1)
					{
						response.Results = priorityTags.Select(t => new ISearchService.SearchTagModel(t, InternalEncryptionService) { IsPrioritized = true }).ToList();
					}
					else
					{
						foreach (TagEntity tag in priorityTags)
						{
							excludeIds.Add(tag.Id);
						}
						IQueryable<TagEntity> query = Connection.Query<TagEntity>()
							.Where(t => !excludeIds.Contains(t.Id) && !excludeTexts.Contains(t.Name));
						if (checkText)
						{
							query = query.Where(t => t.Name.ToLower().Contains(request.Name.ToLower()));
						}
						response.Results = priorityTags
							.Select(t => new ISearchService.SearchTagModel(t, InternalEncryptionService) { IsPrioritized = true })
							.Concat(
								query
								.Select(t => new ISearchService.SearchTagModel(t, InternalEncryptionService) { IsPrioritized = false }))
								.Take(tagsLeft)
							.Take(request.ResultsCount)
							.ToList();
					}
				})
				.Evaluate(response);

			return response;
		}
		public ISearchService.SearchDepartmentsResponse SearchDepartments(ISearchService.SearchDepartmentsRequestBase request)
		{
			ISearchService.SearchDepartmentsResponse response = new ISearchService.SearchDepartmentsResponse();

			Lazy<IAccountEntity> account = GetAccountEntityLazily(request);
			Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
			UserEntity user = GetUserEntity(request);

			List<Guid> excludeIds = request.ExcludeKeys != null ? request.ExcludeKeys.Select(k => InternalEncryptionService.IdentifiablyDecryptToId(k)).ToList() : new List<Guid> { };

			FirstValidateAsAccount(user, citizen, account)
				.ThisCompound(() => request.ResultsCount >= 1 && request.ResultsCount <= 10, ValidationField.Create(nameof(request.ResultsCount)), ValidationCode.Invalid)
				.SetOnCriterionMet(() =>
				{
					IQueryable<DepartmentEntityBase> query = Connection.Query<DepartmentEntityBase>().Where(d => !excludeIds.Contains(d.Id) && account.Value.CanSee(d, Connection));

					if (request.Accessibility != null)
					{
						query = query.Where(d => Connection.Query<IDepartmentSettingsEntity<IDepartmentEntity>>().Single(s => s.Owner.Id == d.Id).Accessibility == request.Accessibility);
					}
					if (request.CreatorKey != IEncryptionService.IdentifiablyEncrypted.Empty)
					{
						query = query.Where(d => d.Creator.Id == InternalEncryptionService.IdentifiablyDecryptToId(request.CreatorKey));
					}
					if (request.ExcludeNames != null)
					{
						query = query.Where(d => !request.ExcludeNames.Any(n => n.Equals(d.Name)));
					}
					if (request.TagsKeys != null)
					{
						List<Guid> ids = request.TagsKeys.Select(k => InternalEncryptionService.IdentifiablyDecryptToId(k)).ToList();
						query = query.Where(d => ids.All(id => d.Tags.Any(t => t.Id == id)));
					}
					if (request.PriorityTagsKeys != null)
					{
						List<Guid> ids = request.PriorityTagsKeys.Select(k => InternalEncryptionService.IdentifiablyDecryptToId(k)).ToList();
						query = query.Where(d => ids.All(id => d.Tags.Any(t => t.Id == id)));
					}
					if (request.MembersKeys != null)
					{
						List<Guid> ids = request.MembersKeys.Select(k => InternalEncryptionService.IdentifiablyDecryptToId(k)).ToList();
						query = query.Where(d => ids.All(id => d.Members.Any(m => m.Id == id)));
					}
					if (request.AdminsKeys != null)
					{
						List<Guid> ids = request.AdminsKeys.Select(k => InternalEncryptionService.IdentifiablyDecryptToId(k)).ToList();
						query = query.Where(d => ids.All(id => d.Admins.Any(a => a.Id == id)));
					}
					if (request.CreationDate != null)
					{
						query = query.Where(d => d.CreationDate == request.CreationDate);
					}
					if (request is ISearchService.SearchSubDepartmentsRequest searchSubDepartmentsRequest)
					{
						query = query.Where(d => d is SubDepartmentEntity);
						if (request.Name.IsValidSubDepartmentName())
						{
							query = query.Where(d => d.Name.ToLower().Contains(request.Name.ToLower()));
						}
					}
					if (request is ISearchService.SearchOrgsRequest searchOrgsRequest)
					{
						query = query.Where(d => d is OrgEntity);
						if (request.Name.IsValidOrgName())
						{
							query = query.Where(d => d.Name.ToLower().Contains(request.Name.ToLower()));
						}
						if (searchOrgsRequest.SID.IsValidSID())
						{
							query = query.Where(d => (d as OrgEntity).SID.Equals(searchOrgsRequest.SID));
						}
					}
					response.Results = query
					 .Take(request.ResultsCount)
					 .Select(c => c.ToResponse(InternalEncryptionService, Connection))
					 .ToList();

					LogIfAccessingAsDelegate(user, "searched for departments");
				})
				.Evaluate(response);
			return response;
		}
		public ISearchService.SearchCitizensResponse SearchCitizens(ISearchService.SearchCitizensRequest request)
		{
			ISearchService.SearchCitizensResponse response = new ISearchService.SearchCitizensResponse();

			Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
			UserEntity user = GetUserEntity(request);

			IEnumerable<Guid> excludeIds = request.ExcludeKeys != null ? request.ExcludeKeys.Select(k => InternalEncryptionService.IdentifiablyDecryptToId(k)) : new List<Guid> { };

			FirstValidateAsCitizen(user, citizen)
				.ThisCompound(() => request.ResultsCount >= 1 && request.ResultsCount <= 10, ValidationField.Create(nameof(request.ResultsCount)), ValidationCode.Invalid)
				.SetOnCriterionMet(() =>
				{
					IQueryable<CitizenEntity> query = Connection.Query<CitizenEntity>().Where(c => !excludeIds.Contains(c.Id));
					if (!String.IsNullOrWhiteSpace(request.Name))
					{
						query = query.Where(c => c.Name.ToLower().Contains(request.Name.ToLower()));
					}
					if (request.CanBeRecruitedAsDepartmentAdmin != null)
					{
						query = query.Where(c => Connection.Query<CitizenSettingsEntity>().Single(s => s.Owner.Id == c.Id).CanBeRecruitedAsDepartmentAdmin == request.CanBeRecruitedAsDepartmentAdmin);
					}
					response.Results = query
					 .Take(request.ResultsCount)
					 .Select(c => new ICitizenService.GetCitizenResponse(c, InternalEncryptionService))
					 .ToList();

					LogIfAccessingAsDelegate(user, "searched for citizens");
				})
				.Evaluate(response);
			return response;
		}
		public ISearchService.SearchAccountsResponse SearchAccounts(ISearchService.SearchAccountsRequestBase request)
		{
			ISearchService.SearchAccountsResponse response = new ISearchService.SearchAccountsResponse();

			Lazy<IAccountEntity> account = GetAccountEntityLazily(request);
			Lazy<CitizenEntity> citizen = GetCitizenEntityLazily(request);
			UserEntity user = GetUserEntity(request);

			List<Guid> excludeIds = request.ExcludeKeys != null ? request.ExcludeKeys.Select(k => InternalEncryptionService.IdentifiablyDecryptToId(k)).ToList() : new List<Guid> { };

			FirstValidateAsAccount(user, citizen, account)
				.ThisCompound(() => request.ResultsCount >= 1 && request.ResultsCount <= 10, ValidationField.Create(nameof(request.ResultsCount)), ValidationCode.Invalid)
				.SetOnCriterionMet(() =>
				{
					IQueryable<AccountEntityBase> query = Connection.Query<AccountEntityBase>().Where(a => !excludeIds.Contains(a.Id));

					if (request.TransactionOfferLifetime != null)
					{
						query = query.Where(a => Connection.Query<IAccountSettingsEntity>().Single(s => s.Owner.Id == a.Id).TransactionOfferLifetime == request.TransactionOfferLifetime);
					}
					if (request.TagsKeys != null)
					{
						List<Guid> ids = request.TagsKeys.Select(k => InternalEncryptionService.IdentifiablyDecryptToId(k)).ToList();
						query = query.Where(a => ids.All(id => a.Tags.Any(t => t.Id == id)));
					}
					if (request.PriorityTagsKeys != null)
					{
						List<Guid> ids = request.PriorityTagsKeys.Select(k => InternalEncryptionService.IdentifiablyDecryptToId(k)).ToList();
						query = query.Where(a => ids.All(id => a.Tags.Any(t => t.Id == id)));
					}
					if (request.ManagerKey != IEncryptionService.IdentifiablyEncrypted.Empty)
					{
						CitizenEntity manager = GetEntity<CitizenEntity>(request.ManagerKey);
						if (manager != null)
						{
							query = query.Where(a => manager.Manages(a, Connection));
						}

					}
					if (!String.IsNullOrWhiteSpace(request.Name))
					{
						query = query.Where(a => a.Name.ToLower().Contains(request.Name.ToLower()));

					}
					if (request.MinimumUntilDue != null)
					{
						query = query.Where(a => Connection.Query<IAccountSettingsEntity>().Single(s => s.Owner.Id == a.Id).MinimumContractLifeSpan == request.MinimumUntilDue);

					}
					if (request.CreatorKey != IEncryptionService.IdentifiablyEncrypted.Empty)
					{
						query = query.Where(a => a.Creator.Id == InternalEncryptionService.IdentifiablyDecryptToId(request.CreatorKey));

					}
					if (request.CanBeRecruitedIntoDepartments != null)
					{
						query = query.Where(a => Connection.Query<IAccountSettingsEntity>().Single(s => s.Owner.Id == a.Id).CanBeRecruitedIntoDepartments == request.CanBeRecruitedIntoDepartments);

					}
					if (request is ISearchService.SearchVirtualAccountsRequest virtualRequest)
					{
						query = query.Where(a => a is VirtualAccountEntity);
						if (virtualRequest.DepositForwardUntilDue != null)
						{
							query = query.Where(a => Connection.Query<VirtualAccountSettingsEntity>().Single(s => s.Owner.Id == a.Id).DepositForwardLifeSpan == virtualRequest.DepositForwardUntilDue);

						}
						if (virtualRequest.DefaultDepositAccountMapAbsoluteLimit != null)
						{
							query = query.Where(a => Connection.Query<VirtualAccountSettingsEntity>().Single(s => s.Owner.Id == a.Id).DefaultDepositAccountMapAbsoluteLimit == virtualRequest.DefaultDepositAccountMapAbsoluteLimit);

						}
						if (virtualRequest.DefaultDepositAccountMapAbsoluteLimit != null)
						{
							query = query.Where(a => Connection.Query<VirtualAccountSettingsEntity>().Single(s => s.Owner.Id == a.Id).DefaultDepositAccountMapRelativeLimit == virtualRequest.DefaultDepositAccountMapRelativeLimit);

						}
						if (virtualRequest.Accessibility != null)
						{
							query = query.Where(a => Connection.Query<VirtualAccountSettingsEntity>().Single(s => s.Owner.Id == a.Id).Accessibility == virtualRequest.Accessibility);

						}
					}
					else if (request is ISearchService.SearchRealAccountsRequest realRequest)
					{
						query = query.OfType<RealAccountEntity>();
						if (realRequest.CanBeForwardingAccountFor != null)
						{
							//TODO: Implement
						}
						if (realRequest.CanBeDepositAccountFor != null)
						{
							//TODO: Implement
						}
					}
					if (request.CanReceiveTransactionOffersFor != null)
					{
						foreach (KeyValuePair<String, Boolean?> kvp in request.CanReceiveTransactionOffersFor)
						{
							//TODO: Implement
						}

					}
					if (request.CanBeMiddlemanFor != null)
					{
						foreach (KeyValuePair<String, Boolean?> kvp in request.CanBeMiddlemanFor)
						{
							//TODO: Implement
						}

					}
					response.Results = query
						.Take(request.ResultsCount)
						.ToList()
						.Select(a => a.ToResponse(InternalEncryptionService, Connection))
						.ToList();

					LogIfAccessingAsDelegate(user, "searched for accounts");
				})
				.Evaluate(response);
			return response;
		}
		 */
		public SearchService(IServiceContext serviceContext) : base(serviceContext)
		{
		}
	}
#endif
}
