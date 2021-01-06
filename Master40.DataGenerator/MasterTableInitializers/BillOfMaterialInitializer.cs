using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DataGenerator.DataModel.ProductStructure;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DataGenerator.MasterTableInitializers
{
    public class BillOfMaterialInitializer
    {
        public static void Init(List<List<Node>> nodesPerLevel, MasterDBContext context)
        {

            var boms = nodesPerLevel.SelectMany(_ => _).SelectMany(x => x.Operations).SelectMany(x => x.Bom);
            context.ArticleBoms.AddRange(boms);
            context.SaveChanges();
        }
    }
}