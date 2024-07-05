using System.Collections.Generic;
using System.Linq;

using static CBCommon.Enums.CitizenBankEnums;

namespace CBCommon.Components
{
	public class TransactionContractExposureRuleBook
	{
		public ICollection<TransactionContractExposureRule> Rules { get; set; }
		public IEnumerable<TransactionPartnersRelationship> GetSuccessors(TransactionPartnersRelationship precursor)
		{
			return Rules.Where(r => r.Precursor == precursor).Select(r => r.Successor);
		}
	}
}
