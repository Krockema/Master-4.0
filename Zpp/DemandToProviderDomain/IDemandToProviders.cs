using Zpp.DemandDomain;
using Zpp.ProviderDomain;

namespace Zpp.DemandToProviderDomain
{
    /**
     * Maps one demand to max. two providers (at least one of the providers must be exhausted,
     * the demand must be satisfied by both providers)
     */
    public interface IDemandToProviders
    {
        bool IsSatisfied(Demand demand);

        /// <summary>
        /// given provider is added to given demand
        /// </summary>
        /// <param name="demand">that gets a new provider</param>
        /// <param name="provider">that is added to demand</param>
        void AddProviderForDemand(Demand demand, Provider provider);

        /**
         * Should  do "AddProviderForDemand" for every provider of given providers
         */
        void AddProvidersForDemand(Demand demand, Providers providers);

        Provider FindNonExhaustedProvider(Demand demand);
    }
}