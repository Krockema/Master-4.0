using System.Collections.Generic;

namespace Master40.DataGenerator.DataModel.ProductStructure
{
    public class ProductStructure
    {
        public List<List<Node>> NodesPerLevel { get; set; }
        public List<Edge> Edges { get; set; }
        public int NodesCounter { get; set; }

        public ProductStructure()
        {
            NodesPerLevel = new List<List<Node>>();
            Edges = new List<Edge>();
            NodesCounter = 0;
        }

    }
}
