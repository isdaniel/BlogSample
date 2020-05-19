using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ParameterConvertor
{
    class Program
    {
        static void Main(string[] args)
        {
         
        }

        public List<Txn> FilerProcessList(ProcessRequest dataModel,ProductGroup productGroupId ,int processStatus, List<string> cancelTicket = null)
        {
            if (cancelTicket == null)
            {
                cancelTicket = new List<string>();
            }

            List<Txn> resultSwtxns = new List<Txn>();

            var gameEndTime = dataModel.EndDate;

            resultSwtxns.AddRange(
            dataModel.Trans.Where(x => x.GroupType.ToLower() == "normal" && x.Status == processStatus).Select(o => new Txn()
            {
                ProductDatetime = gameEndTime,
                ProductGroupID = productGroupId,
                ProcessCode = dataModel.ProcessID,
                ProcessType = dataModel.ProcessType,
                ProcessNo = o.DetailInfo.First().SeqNo.ToString(),
                TransactionID = $"{o.DetailInfo.First().SeqNo.ToString()}_{dataModel.TransactionId.ToString()}",
                MemberCode = o.MemberCode,
                ReturnPrice = o.DetailInfo.First().ReturnAmount,
                ProcessStatus = (processStatus == 4 && cancelTicket.Contains(o.DetailInfo.First().SeqNo.ToString())) ? ProcessStatus.Canceled : StatusMapper(o.Status), 
                UserDefinedString1 = o.GroupType,
                UserDefinedString2 = StatusMapper(o.Status) == ProcessStatus.Void ? JsonConvert.SerializeObject(o.DetailInfo.Select(x => 
                    new { GameID = dataModel.ProcessID, BetType = x.BetType, Selection = x.Selection }).First()) : "",
                UserDefinedString3 = dataModel.TransactionId.ToString(),
                UserDefinedInteger1 = (processStatus == 4 && cancelTicket.Contains(o.DetailInfo.First().SeqNo.ToString())) ? (int?) 4: null //todo

            }));

            resultSwtxns.AddRange(
                dataModel.Trans.Where(x => x.GroupType.ToLower() == "parlay" && x.Status == processStatus).Select(o => new Txn()
                {
                    ProductDatetime = gameEndTime,
                    ProductGroupID = productGroupId,
                    ProcessCode = o.ParlayTranId,
                    ProcessType = dataModel.ProcessType,
                    ProcessNo = o.ConfirmationId.ToString(),
                    TransactionID = $"{o.ConfirmationId.ToString()}_{dataModel.TransactionId.ToString()}",
                    MemberCode = o.MemberCode,
                    ReturnPrice = o.TotalReturnAmt,
                    ProcessStatus = (processStatus == 4 && cancelTicket.Contains(o.ConfirmationId.ToString())) ? ProcessStatus.Canceled :StatusMapper(o.Status),
                    UserDefinedString1 = o.GroupType,
                    UserDefinedString2 = StatusMapper(o.Status) == ProcessStatus.Void ? JsonConvert.SerializeObject(o.DetailInfo.Select(x => 
                        new { GameID = dataModel.ProcessID, BetType = x.BetType, Selection = x.Selection })) : "",
                    UserDefinedString3 = dataModel.TransactionId.ToString(),
                    UserDefinedInteger1 = (processStatus == 4 && cancelTicket.Contains(o.ConfirmationId.ToString())) ? (int?) 4: null //todo
                }));

            return resultSwtxns;
        }

        private ProcessStatus StatusMapper(int status)
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
