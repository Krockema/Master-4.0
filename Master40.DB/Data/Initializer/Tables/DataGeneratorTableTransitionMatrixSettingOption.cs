using System.Collections.Generic;
using Master40.DB.Data.Context;
using Master40.DB.GeneratorModel;

namespace Master40.DB.Data.Initializer.Tables
{
    public class DataGeneratorTableTransitionMatrixSettingOption
    {
        public TransitionMatrixSettingOption BALANCED_PI_B_INIT;
        public TransitionMatrixSettingOption BALANCED_PI_A_INIT;

        public DataGeneratorTableTransitionMatrixSettingOption()
        {
            BALANCED_PI_B_INIT = new TransitionMatrixSettingOption
            {
                Id = 1, Name = "BALANCED_PI_B_INIT",
                Description = "Pi_B is initiated in the else case so that a 1 occurs exactly once in each column."
            };
            BALANCED_PI_A_INIT = new TransitionMatrixSettingOption
            {
                Id = 2, Name = "BALANCED_PI_A_INIT",
                Description =
                    "Columns of Pi_A are getting balanced during initiation so that the column sums are each getting approximated to 1."
            };
        }

        public void Init(DataGeneratorContext ctx)
        {
            var options = new TransitionMatrixSettingOption[]
            {
                BALANCED_PI_B_INIT,
                BALANCED_PI_A_INIT
            };

            ctx.TransitionMatrixSettingOptions.AddRange(options);
            ctx.SaveChanges();
        }

        public List<TransitionMatrixSettingOption> AsList()
        {
            return new List<TransitionMatrixSettingOption>
            {
                BALANCED_PI_B_INIT,
                BALANCED_PI_A_INIT
            };
        }
    }
}