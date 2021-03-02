using System;
using System.Collections.Generic;
using System.Data.HashFunction.xxHash;
using System.Linq;
using Master40.DataGenerator.DataModel.ProductStructure;
using Master40.DB.DataModel;

namespace Master40.DataGenerator.Generators
{
    public class BillOfMaterialGenerator
    {

        public void GenerateBillOfMaterial(List<List<Node>> nodesPerLevel, Random rng)
        {
            foreach (var article in nodesPerLevel.SelectMany(_ => _).Where(x => x.AssemblyLevel < nodesPerLevel.Count))
            {
                List<List<Edge>> incomingMaterialAllocation = new List<List<Edge>>();
                foreach (var operation in article.Operations)
                {
                    incomingMaterialAllocation.Add(new List<Edge>());
                }

                foreach (var edge in article.IncomingEdges)
                {
                    var operationNumber = rng.Next(incomingMaterialAllocation.Count);
                    incomingMaterialAllocation[operationNumber].Add(edge);
                }

                List<List<Edge>> possibleSetsForFirstOperation =
                    incomingMaterialAllocation.FindAll(x => x.Count > 0);
                var randomSet = rng.Next(possibleSetsForFirstOperation.Count);
                List<Edge> firstOperation = possibleSetsForFirstOperation[randomSet];

                List<List<Edge>> bom = new List<List<Edge>>();
                incomingMaterialAllocation.Remove(firstOperation);
                bom.Add(firstOperation);
                bom.AddRange(incomingMaterialAllocation);

                for (var i = 0; i < bom.Count; i++)
                {
                    foreach (var edge in bom[i])
                    {
                        var name = "[" + edge.Start.Article.Name + "] in (" +
                                   article.Operations[i].MOperation.Name + ")";
                        var articleBom = new M_ArticleBom()
                        {
                            ArticleChildId = edge.Start.Article.Id,
                            Name = name, Quantity = (decimal) edge.Weight,
                            ArticleParentId = article.Article.Id,
                            OperationId = article.Operations[i].MOperation.Id
                        };
                        article.Operations[i].Bom.Add(articleBom);
                    }
                }
            }
        }

    }
}