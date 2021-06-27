using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ParameterConvertor
{
    public class SettleBetRequest
    {
        [JsonProperty("gameID")]
        public string TradeId { get; set; }

        [JsonProperty("gameType")]
        public string ProductType { get; set; }

        [JsonProperty("gameResult")]
        public string GameResult { get; set; }

        [JsonProperty("gameEnd")]
        public DateTime? EndDate { get; set; }

        [JsonProperty("transactionID")]
        public long TransactionId { get; set; }

        [JsonProperty("bets")]
        public IEnumerable<SettleBetDetail> Bets { get; set; }
    }

    public class SettleBetDetail
    {
        [JsonProperty("memberCode")]
        public string MemberCode { get; set; }

        [JsonProperty("confirmationID")]
        public long ConfirmationId { get; set; }

        [JsonProperty("totalReturnAmt")]
        public decimal TotalReturnAmt { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("channel")]
        public int Channel { get; set; }

        [JsonProperty("wagerGroupType")]
        public string WagerGroupType { get; set; }

        [JsonProperty("parlayGameID")]
        public string ParlayGameID { get; set; }

        [JsonProperty("betDetail")]
        public IEnumerable<BetDetail> Detail { get; set; }
    }

    public class BetDetail
    {
        [JsonProperty("betNo")]
        public long BetNo { get; set; }

        [JsonProperty("seq")]
        public int Seq { get; set; }

        [JsonProperty("betType")]
        public string BetType { get; set; }

        [JsonProperty("selection")]
        public string Selection { get; set; }

        [JsonProperty("betReturnAmt")]
        public decimal BetReturnAmt { get; set; }

        [JsonProperty("detailGameResult")]
        public string DetailGameResult { get; set; }

        [JsonProperty("betResult")]
        public int? BetResult { get; set; }
    }
}