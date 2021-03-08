using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DataGenerator.DataModel.ProductStructure;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DataGenerator.MasterTableInitializers
{
    public class OperationInitializer
    {
        public static void Init(List<List<Node>> nodesPerLevel, MasterDBContext context)
        {
            var operations = nodesPerLevel.SelectMany(_ => _).SelectMany(x => x.Operations)
                .Select(x => x.MOperation);
            context.Operations.AddRange(operations);
            context.SaveChanges();
        }
    }
}