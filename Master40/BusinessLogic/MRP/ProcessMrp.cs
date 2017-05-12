﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.BusinessLogic.Helper;
using Master40.Data;
using Master40.Models.DB;
using Master40.Models;
using Microsoft.EntityFrameworkCore;

namespace Master40.BusinessLogic.MRP
{
    public interface IProcessMrp
    {
        List<LogMessage> Logger { get; set; }
        Task Process(int orderId);
    }

    public class ProcessMrp : IProcessMrp
    {
        private readonly MasterDBContext _context;
        public List<LogMessage> Logger { get; set; }
        public ProcessMrp(MasterDBContext context)
        {
            _context = context;
        }

        async Task IProcessMrp.Process(int orderId)
        {
            await Task.Run(() => {
            
                var order = _context.OrderParts.Where(a => a.OrderId == orderId);
                foreach (var orderPart in order)
                {
                    var manufacturingSchedule = new ManufacturingSchedule();
                    ExecutePlanning(null,null,orderPart.OrderPartId);
                    //Todo: Push to db
                }
                

            });
        }

        // gross, net requirement, create schedule, backward, forward, call children
        private void ExecutePlanning(DemandOrderPart demand, DemandOrderPart demandRequester,int orderPartId)
        {
            var orderPart = _context.OrderParts.Include(a => a.Article).Single(a => a.OrderPartId == orderPartId);
            if (demand == null)
            {
                demand = new DemandOrderPart()
                {
                    OrderPartId = orderPartId,
                    Quantity = orderPart.Quantity,
                    Article = orderPart.Article,
                    ArticleId = orderPart.ArticleId,
                    OrderPart = orderPart,
                    IsProvided = false,
                };
                demandRequester = demand;
            }
           
            IDemandForecast demandForecast = new DemandForecast(_context);
            IScheduling schedule = new Scheduling(_context);
            var productionOrders = demandForecast.NetRequirement(demand, orderPartId);
            foreach (var log in demandForecast.Logger)
            {
                Logger.Add(log);
            }
            
            var manufacturingScheduleItem = schedule.CreateSchedule(orderPartId, productionOrders);
            
            schedule.BackwardScheduling(manufacturingScheduleItem);
            //schedule.ForwardScheduling(manufacturingScheduleItem);
            if (productionOrders != null)
            {
                var children =
                    _context.ArticleBoms.Include(a => a.ArticleChild)
                        .Where(a => a.ArticleParentId == demand.ArticleId);
                if (children.Any())
                {
                    foreach (var child in children)
                    {
                        ExecutePlanning(new DemandOrderPart()
                        {
                            ArticleId = child.ArticleChildId,
                            Article = child.ArticleChild,
                            Quantity = productionOrders.Quantity * (int) child.Quantity,
                            DemandRequesterId = demandRequester.DemandId
                        }, demandRequester,orderPartId);
                    }
                }
            }
        }


    }


}