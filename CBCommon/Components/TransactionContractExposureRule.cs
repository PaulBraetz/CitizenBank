using static CBCommon.Enums.CitizenBankEnums;

namespace CBCommon.Components
{
	public class TransactionContractExposureRule
	{
		public TransactionContractExposureRule(TransactionPartnersRelationship precursor, TransactionPartnersRelationship successor)
		{
			Precursor = precursor;
			Successor = successor;
		}
		public TransactionPartnersRelationship Precursor { get; }
		public TransactionPartnersRelationship Successor { get; }
	}
}
