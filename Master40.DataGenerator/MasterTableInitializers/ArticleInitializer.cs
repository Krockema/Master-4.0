using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DataGenerator.DataModel.ProductStructure;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DataGenerator.MasterTableInitializers
{
    public class ArticleInitializer
    {
        public static void Init(List<List<Node>> nodesPerLevel, MasterDBContext context)
        {
            var articles = nodesPerLevel.SelectMany(_ => _).Select(x => x.Article);
            context.Articles.AddRange(entities: articles);
            context.SaveChanges();
        }
    }
}