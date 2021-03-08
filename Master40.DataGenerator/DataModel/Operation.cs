using System.Collections.Generic;
using Master40.DB.DataModel;

namespace Master40.DataGenerator.DataModel
{
    public class Operation
    {
        public M_Operation MOperation { get; set; }
        public List<M_ArticleBom> Bom { get; set; } = new List<M_ArticleBom>();
        public int SetupTimeOfCapability { get; set; }
        public int InternMachineGroupIndex { get; set; }

    }
}