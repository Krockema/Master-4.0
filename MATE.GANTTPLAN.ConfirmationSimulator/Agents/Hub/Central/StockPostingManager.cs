using System;
using System.Collections.Generic;
using Mate.DataCore.Data.Context;
using Mate.DataCore.GanttPlan;
using Mate.DataCore.GanttPlan.GanttPlanModel;
using Mate.DataCore.Nominal;

namespace Mate.Production.Core.Agents.HubAgent.Types.Central
{
    public class StockPostingManager
    {

        private string _dbConnectionStringGanttPlan { get; }

        private List<GptblStockquantityposting> _stockPostings { get; set; } = new List<GptblStockquantityposting>();

        public StockPostingManager(string dbConnectionStringGanttPlan)
        {
            _dbConnectionStringGanttPlan = dbConnectionStringGanttPlan;
        }


        public void TransferStockPostings()
        {
            using (var localGanttPlanDbContext = GanttPlanDBContext.GetContext(_dbConnectionStringGanttPlan))
            {
                localGanttPlanDbContext.GptblStockquantityposting.AddRange(_stockPostings);
                localGanttPlanDbContext.SaveChanges();

            }

            ResetStockPostings();
        }

        public void ResetStockPostings()
        {
            _stockPostings.Clear();
        }

        public void AddWithdrawalStockPosting(string materialId, double quantity, string materialQuantityUnitId, GanttStockPostingType stockPostingType, long CurrentTime)
        {
            var stockPosting = new GptblStockquantityposting();

            stockPosting.StockquantitypostingId = Guid.NewGuid().ToString();
            stockPosting.MaterialId = materialId;
            stockPosting.Name = materialId;
            stockPosting.Quantity = quantity * -1;
            stockPosting.QuantityUnitId = materialQuantityUnitId;
            stockPosting.ClientId = string.Empty;
            stockPosting.PostingType = (int)stockPostingType;
            stockPosting.Date = CurrentTime.ToDateTime();
            stockPosting.Info1 = string.Empty;
            stockPosting.Info2 = string.Empty;
            stockPosting.Info3 = string.Empty;
            stockPosting.InfoObjectId = string.Empty;
            stockPosting.InfoObjecttypeId = string.Empty;

            _stockPostings.Add(stockPosting);
        }
        public void AddInsertStockPosting(string materialId, double quantity, string materialQuantityUnitId, GanttStockPostingType stockPostingType, long CurrentTime)
        {
            var stockPosting = new GptblStockquantityposting();

            stockPosting.StockquantitypostingId = Guid.NewGuid().ToString();
            stockPosting.MaterialId = materialId;
            stockPosting.Name = materialId;
            stockPosting.Quantity = quantity;
            stockPosting.QuantityUnitId = materialQuantityUnitId;
            stockPosting.ClientId = string.Empty;
            stockPosting.PostingType = (int)stockPostingType;
            stockPosting.Date = CurrentTime.ToDateTime();
            stockPosting.Info1 = string.Empty;
            stockPosting.Info2 = string.Empty;
            stockPosting.Info3 = string.Empty;
            stockPosting.InfoObjectId = string.Empty;
            stockPosting.InfoObjecttypeId = string.Empty;

            _stockPostings.Add(stockPosting);
        }

    }
}
