using System.Linq;
using Master40.DataGenerator.DataModel.ProductStructure;
using Master40.DB.GeneratorModel;

namespace Master40.DataGenerator.Verification
{
    public class ProductStructureVerifier
    {

        public void VerifyComplexityAndReutilizationRation(ProductStructureInput input, ProductStructure productStructure)
        {
            var nodesOfLastAssemblyLevelCounter =
                productStructure.NodesPerLevel[input.DepthOfAssembly - 1].Count;
            var actualComplexityRatio = productStructure.Edges.Count /
                                        (double) (productStructure.NodesCounter - nodesOfLastAssemblyLevelCounter);
            var actualReutilizationRation = productStructure.Edges.Count /
                                            (double) (productStructure.NodesCounter - input.EndProductCount);

            System.Diagnostics.Debug.WriteLine("################################# Generated product structure has a complexity ratio of " + actualComplexityRatio + " (input was " + input.ComplexityRatio + ")");
            System.Diagnostics.Debug.WriteLine("################################# Generated product structure has a reutilization ratio of " + actualReutilizationRation + " (input was " + input.ReutilisationRatio + ")");
        }

    }
}