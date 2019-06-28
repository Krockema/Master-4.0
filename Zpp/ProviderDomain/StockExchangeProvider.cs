using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp.DemandDomain;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;

namespace Zpp.ProviderDomain
{
    /**
     * wraps T_StockExchange for T_StockExchange providers
     */
    public class StockExchangeProvider : Provider, IProviderLogic
    {
        public StockExchangeProvider(IProvider provider, Demands demands, IDbMasterDataCache dbMasterDataCache) : base(provider, demands, dbMasterDataCache)
        {
        }

        public override IProvider ToIProvider()
        {
            return (T_StockExchange)_provider;
        }

        public override Id GetArticleId()
        {
            Id stockId = new Id(((T_StockExchange) _provider).StockId);
            M_Stock stock = _dbMasterDataCache.M_StockGetById(stockId);
            return new Id(stock.ArticleForeignKey);
        }
    }
}