using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.SimulationCore.Helper;
using Master40.XUnitTest.DBContext;
using Xunit;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;
using Zpp.Test.WrappersForPrimitives;

namespace Zpp.Test
{
    public class TestProvider : AbstractTest
    {


        public TestProvider()
        {

        }

        /**
         * Verifies, that 
         * - 
         */
        [Fact(Skip = "Not implemented yet.")]
        public void TestCreateNeededDemands()
        {
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);
            
            // TODO
            Assert.True(false);
        }

    }
}