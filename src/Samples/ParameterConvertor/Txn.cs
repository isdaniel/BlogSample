using System;

namespace ParameterConvertor
{
    public class Txn
    {
        public DateTime DateCreatedUTC { get; set; }

        public DateTime? ProductDatetime { get; set; }

        public ProductGroup ProductGroupID { get; set; }

        public string ProcessCode { get; set; }

        public string ProcessType { get; set; }

        public string ProcessNo { get; set; }

        public string TransactionID { get; set; }

        public string MemberCode { get; set; }

        public Decimal? Price { get; set; }

        public Decimal? BasePrice { get; set; }

        public Decimal? ReturnAmount { get; set; }

        public ProcessStatus ProcessStatus { get; set; }

        public string IpAddress { get; set; }

        public int? ChannelID { get; set; }

        public string Remark1 { get; set; }

        public string Remark2 { get; set; }

        public int? UserDefinedInteger1 { get; set; }

        public int? UserDefinedInteger2 { get; set; }

        public int? UserDefinedInteger3 { get; set; }

        public DateTime? UserDefinedDateTime1 { get; set; }

        public DateTime? UserDefinedDateTime2 { get; set; }

        public DateTime? UserDefinedDateTime3 { get; set; }

        public Decimal? UserDefinedDecimal1 { get; set; }

        public Decimal? UserDefinedDecimal2 { get; set; }

        public Decimal? UserDefinedDecimal3 { get; set; }

        public string UserDefinedString1 { get; set; }

        public string UserDefinedString2 { get; set; }

        public string UserDefinedString3 { get; set; }

        public DateTime? FinishDatetime { get; set; }

        public int? PossessVersion { get; set; }

        public int? CurrencyVersionNo { get; set; }

        public string GameResult { get; set; }
    }
}