using Master40.DB.Data.Context;

namespace Master40.DataGenerator.Repository
{
    public class GlobalMasterRepository
    {

        public static void DeleteAllButResourceData(MasterDBContext ctx)
        {
            ctx.ProductionOrderBoms.RemoveRange(ctx.ProductionOrderBoms);
            ctx.ProductionOrderOperations.RemoveRange(ctx.ProductionOrderOperations);
            ctx.ProductionOrders.RemoveRange(ctx.ProductionOrders);
            ctx.MeasurementValues.RemoveRange(ctx.MeasurementValues);
            ctx.StockExchanges.RemoveRange(ctx.StockExchanges);
            ctx.DemandToProviders.RemoveRange(ctx.DemandToProviders);
            ctx.ProviderToDemand.RemoveRange(ctx.ProviderToDemand);
            ctx.PurchaseOrderParts.RemoveRange(ctx.PurchaseOrderParts);
            ctx.PurchaseOrders.RemoveRange(ctx.PurchaseOrders);
            ctx.CustomerOrderParts.RemoveRange(ctx.CustomerOrderParts);
            ctx.CustomerOrders.RemoveRange(ctx.CustomerOrders);

            ctx.ValueTypes.RemoveRange(ctx.ValueTypes);
            ctx.ArticleToBusinessPartners.RemoveRange(ctx.ArticleToBusinessPartners);
            ctx.BusinessPartners.RemoveRange(ctx.BusinessPartners);
            ctx.Attributes.RemoveRange(ctx.Attributes);
            ctx.Characteristics.RemoveRange(ctx.Characteristics);
            ctx.ArticleBoms.RemoveRange(ctx.ArticleBoms);
            ctx.Operations.RemoveRange(ctx.Operations);
            ctx.Stocks.RemoveRange(ctx.Stocks);
            ctx.Articles.RemoveRange(ctx.Articles);
            ctx.ArticleTypes.RemoveRange(ctx.ArticleTypes);
            ctx.Units.RemoveRange(ctx.Units);

            ctx.SaveChanges();
        }

    }
}