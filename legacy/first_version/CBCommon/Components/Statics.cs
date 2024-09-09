using static CBCommon.Enums.CitizenBankEnums;

namespace CBCommon.Components
{
    public static class Statics
	{
		public static readonly Dictionary<TransactionPartnersRelationship, TransactionContractExposureRuleBook> TransactionContractExposureRuleBookDictionary = new()
		{
			{
				TransactionPartnersRelationship.RealToReal,
				new TransactionContractExposureRuleBook()
				{
					Rules = new List<TransactionContractExposureRule>()
					{
						new TransactionContractExposureRule(TransactionPartnersRelationship.RealToReal, TransactionPartnersRelationship.RealToReal)
					}
				}
			},
			{
				TransactionPartnersRelationship.RealToVirtual,
				new TransactionContractExposureRuleBook()
				{
					Rules = new List<TransactionContractExposureRule>
					{
						new TransactionContractExposureRule(TransactionPartnersRelationship.RealToVirtual, TransactionPartnersRelationship.RealToForward),
						new TransactionContractExposureRule(TransactionPartnersRelationship.RealToForward, TransactionPartnersRelationship.ForwardToDeposit)
					}
				}
			},
			{
				TransactionPartnersRelationship.VirtualToReal,
				new TransactionContractExposureRuleBook()
				{
					Rules = new List<TransactionContractExposureRule>
					{
						new TransactionContractExposureRule(TransactionPartnersRelationship.VirtualToReal, TransactionPartnersRelationship.DepositToForward),
						new TransactionContractExposureRule(TransactionPartnersRelationship.DepositToForward, TransactionPartnersRelationship.ForwardToReal)
					}
				}
			},
			{
				TransactionPartnersRelationship.VirtualToVirtual,
				new TransactionContractExposureRuleBook()
				{
					Rules = new List<TransactionContractExposureRule>
					{
						new TransactionContractExposureRule(TransactionPartnersRelationship.VirtualToVirtual, TransactionPartnersRelationship.DepositToForward),
						new TransactionContractExposureRule(TransactionPartnersRelationship.DepositToForward, TransactionPartnersRelationship.ForwardToForward),
						new TransactionContractExposureRule(TransactionPartnersRelationship.ForwardToForward, TransactionPartnersRelationship.ForwardToDeposit)
					}
				}
			}
		};
	}
}
