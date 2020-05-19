using System;
using System.Collections.Generic;

namespace ParameterConvertor
{
    public class ProcessRequest
    {
        public string ProcessID { get; set; }

        public string ProcessType { get; set; }

        public DateTime? EndDate { get; set; }
        public long TransactionId { get; set; }
        public IEnumerable<ProcessDetail> Trans { get; set; }
    }

    public class ProcessDetail
    {
        public string MemberCode { get; set; }

        public long ConfirmationId { get; set; }

        public decimal TotalReturnAmt { get; set; }

        public int Status { get; set; }
    
        public string GroupType { get; set; }

        public string ParlayTranId { get; set; }

        public IEnumerable<DetailInfo> DetailInfo { get; set; }
    }


    public class DetailInfo
    {
        public long SeqNo { get; set; }

        public string BetType { get; set; }

        public string Selection { get; set; }

        public decimal ReturnAmount { get; set; }
    }

}