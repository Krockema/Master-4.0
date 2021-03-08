using System.Collections.Generic;
using System.Linq;
using Master40.DB.GeneratorModel;

namespace Master40.DB.Data.Initializer.Tables
{
    public class DataGeneratorTableTransitionMatrixSettingConfiguration
    {
        public TransitionMatrixSettingConfiguration BALANCED_PI_B_INIT;
        public TransitionMatrixSettingConfiguration BALANCED_PI_A_INIT;

        public DataGeneratorTableTransitionMatrixSettingConfiguration()
        {
            SetupDefaultValues();
        }

        public DataGeneratorTableTransitionMatrixSettingConfiguration(List<TransitionMatrixSettingConfiguration> setting)
        {
            SetupDefaultValues();
            var list = AsList();
            setting.ForEach(x =>
                list.Single(y => y.SettingOptionId == x.SettingOptionId).SettingValue = x.SettingValue);
        }

        private void SetupDefaultValues()
        {
            var options = new DataGeneratorTableTransitionMatrixSettingOption();
            BALANCED_PI_B_INIT = new TransitionMatrixSettingConfiguration()
                { SettingOptionId = options.BALANCED_PI_B_INIT.Id, SettingValue = 1.0 };
            BALANCED_PI_A_INIT = new TransitionMatrixSettingConfiguration()
                { SettingOptionId = options.BALANCED_PI_A_INIT.Id, SettingValue = 0.0 };
        }

        public List<TransitionMatrixSettingConfiguration> AsList()
        {
            return new List<TransitionMatrixSettingConfiguration>
            {
                BALANCED_PI_B_INIT,
                BALANCED_PI_A_INIT
            };
        }
    }
}