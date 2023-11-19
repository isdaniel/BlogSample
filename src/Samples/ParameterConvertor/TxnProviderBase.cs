using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ParameterConvertor
{
    public abstract class TxnProviderBase
    {
        protected abstract string WagerGroupType { get; }

        /// <summary>
        /// Filter SettleBet SPI Data Model
        /// </summary>
        /// <param name="dataModel"></param>
        /// <param name="betStatus"></param>
        /// <returns></returns>
        public List<Txn> GetTxn(SettleBetRequest dataModel,ProductGroup productGroupId ,int betStatus, List<string> cancelWagerNo = null)
        {
            if (cancelWagerNo == null)
            {
                cancelWagerNo = new List<string>();
            }
            //Need to consider filteredWagerNo param
            List<Txn> result = new List<Txn>();


            dataModel.Bets.Where(x => x.WagerGroupType.ToLower() == WagerGroupType && x.Status == betStatus)
                .Select(o => new Txn()
                {
                    ProductDatetime =  dataModel.EndDate,
                    ProductGroupID = productGroupId,
                    ProcessCode = GetProcessCode(dataModel),
                    ProcessType = dataModel.ProductType,
                    ProcessNo = GetBetNo(o).ToString(),
                    TransactionID = $"{GetBetNo(o).ToString()}_{dataModel.TransactionId.ToString()}",
                    MemberCode = o.MemberCode,
                    ReturnAmount = GetReturnAmount(o),
                    ProcessStatus = (betStatus == 4 && cancelWagerNo.Any(a=>a == GetBetNo(o).ToString())) ? ProcessStatus.Canceled : 
                        GetBrandWagerStatus(o.Status), //TODO
                    UserDefinedString1 = o.WagerGroupType,
                    UserDefinedString2 = GetBrandWagerStatus(o.Status) == ProcessStatus.Void ? JsonConvert.SerializeObject(o.Detail.Select(x => new { Seq = x.Seq, GameID = dataModel.TradeId, BetType = x.BetType, Selection = x.Selection }).First()) : "",
                    UserDefinedString3 = dataModel.TransactionId.ToString(),
                    UserDefinedInteger1 = (betStatus == 4 && cancelWagerNo.Contains(o.Detail.First().BetNo.ToString())) ? (int?) 4: null //todo
                });
            result.AddRange(
                dataModel.Bets.Where(x => x.WagerGroupType.ToLower() == "parlay" && x.Status == betStatus).Select(o => new Txn()
                {
                    ProductDatetime =  dataModel.EndDate,
                    ProductGroupID = productGroupId,
                    ProcessCode = o.ParlayGameID,
                    ProcessType = dataModel.ProductType,
                    ProcessNo = o.ConfirmationId.ToString(),
                    TransactionID = $"{o.ConfirmationId.ToString()}_{dataModel.TransactionId.ToString()}",
                    MemberCode = o.MemberCode,
                    ReturnAmount = o.TotalReturnAmt,
                    ProcessStatus = (betStatus == 4 && cancelWagerNo.Contains(o.ConfirmationId.ToString())) ? ProcessStatus.Canceled :GetBrandWagerStatus(o.Status),
                    UserDefinedString1 = o.WagerGroupType,
                    UserDefinedString2 = GetBrandWagerStatus(o.Status) == ProcessStatus.Void ? JsonConvert.SerializeObject(o.Detail.Select(x => new { Seq = x.Seq, GameID = dataModel.TradeId, BetType = x.BetType, Selection = x.Selection })) : "",
                    UserDefinedString3 = dataModel.TransactionId.ToString(),
                    UserDefinedInteger1 = (betStatus == 4 && cancelWagerNo.Contains(o.ConfirmationId.ToString())) ? (int?) 4: null //todo
                }));

            return result;
        }
        protected abstract string GetBetNo(SettleBetDetail dataModel);

        protected abstract decimal GetReturnAmount(SettleBetDetail dataModel);
        protected abstract string GetProcessCode(SettleBetRequest dataModel);
        

        private ProcessStatus GetBrandWagerStatus(int status)
        {
            var result = ProcessStatus.Confirmed;
            switch (status)
            {
                case 1:
                    result = ProcessStatus.Confirmed;
                    break;
                case 2:
                    result = ProcessStatus.Done;
                    break;
                case 3:
                    result = ProcessStatus.Canceled;
                    break;
                case 5:
                    result = ProcessStatus.Void;
                    break;
            }

            return result;
        }
    }
}