using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Zpp.ProviderDomain;
using Zpp.Utils;
using ZppForPrimitives;

namespace Zpp.DemandDomain
{
    /**
     * Provides default implementations for interface methods, can be moved to interface once C# 8.0 is released
     */
    public abstract class Demand : IDemandLogic
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        protected readonly IDemand _demand;
        protected readonly Guid _guid = Guid.NewGuid();

        public Demand(IDemand demand)
        {
            if (demand == null)
            {
                throw new MrpRunException("Given demand should not be null.");
            }
            _demand = demand;
        }
        
        public Provider CreateProvider(IDbCache dbCache)
        {
            if (_demand.GetArticle().ToBuild)
            {
                ProductionOrder productionOrder =  new ProductionOrder(_demand, dbCache);
                Logger.Debug("ProductionOrder created.");
                return productionOrder;
            }
            return createPurchaseOrderPart(_demand);
        }
        
        private Provider createPurchaseOrderPart(IDemand demand)
        {
            // currently only one businessPartner per article
            M_ArticleToBusinessPartner articleToBusinessPartner = demand.GetArticle()
                .ArticleToBusinessPartners.OfType<M_ArticleToBusinessPartner>().First();
            T_PurchaseOrder purchaseOrder = new T_PurchaseOrder();
            // [Name],[DueTime],[BusinessPartnerId]
            purchaseOrder.DueTime = demand.GetDueTime();
            purchaseOrder.BusinessPartner = articleToBusinessPartner.BusinessPartner;
            purchaseOrder.Name = $"PurchaseOrder{demand.GetArticle().Name} for " +
                                 $"businessPartner {purchaseOrder.BusinessPartner.Id}";


            // demand cannot be fulfilled in time
            if (articleToBusinessPartner.DueTime > demand.GetDueTime())
            {
                Logger.Error($"Article {demand.GetArticle().Id} from demand {demand.Id} " +
                             $"should be available at {demand.GetDueTime()}, but " +
                             $"businessPartner {articleToBusinessPartner.BusinessPartner.Id} " +
                             $"can only deliver at {articleToBusinessPartner.DueTime}.");
            }

            // init a new purchaseOderPart
            T_PurchaseOrderPart purchaseOrderPart = new T_PurchaseOrderPart();

            // [PurchaseOrderId],[ArticleId],[Quantity],[State],[ProviderId]
            purchaseOrderPart.PurchaseOrder = purchaseOrder;
            purchaseOrderPart.Article = demand.GetArticle();
            purchaseOrderPart.Quantity =
                PurchaseManagerUtils.calculateQuantity(articleToBusinessPartner,
                    demand.GetQuantity());
            purchaseOrderPart.State = State.Created;
            // connects this provider with table T_Provider
            purchaseOrderPart.Provider = new T_Provider();


            Logger.Debug("PurchaseOrderPart created.");
            return new PurchaseOrderPart(purchaseOrderPart, null);
        }
        
        

        // TODO: use this
        private int CalculatePriority(int dueTime, int operationDuration, int currentTime)
        {
            return dueTime - operationDuration - currentTime;
        }
        
        public DueTime GetDueTime()
        {
            return new DueTime(_demand.GetDueTime());
        }
        
        public static Demand ToDemand(T_Demand t_demand, List<T_CustomerOrderPart> customerOrderParts,
            List<T_ProductionOrderBom> productionOrderBoms, List<T_StockExchange> stockExchanges)
        {
            IDemand iDemand = null;

            iDemand = customerOrderParts.Single(x => x.Id == t_demand.Id);
            if (iDemand != null)
            {
                return new CustomerOrderPart(iDemand);
            }

            iDemand = productionOrderBoms.Single(x => x.Id == t_demand.Id);
            if (iDemand != null)
            {
                return new ProductionOrderBom(iDemand);
            }

            iDemand = stockExchanges.Single(x => x.Id == t_demand.Id);
            if (iDemand != null)
            {
                return new StockExchangeDemand(iDemand);
            }

            return null;
        }

        public abstract IDemand ToIDemand();

        public bool HasProvider(Providers providers)
        {
            throw new NotImplementedException();
        }
        
        public override bool Equals(object obj)
        {
            var item = obj as Demand;

            if (item == null)
            {
                return false;
            }

            return _guid.Equals(item._guid);
        }
        
        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }

        public Quantity GetQuantity()
        {
            return _demand.GetQuantity();
        }

        public override string ToString()
        {
            return $"{_demand.Id}: {_demand.GetQuantity()} of {_demand.GetArticle().Name}";
        }
    }
}