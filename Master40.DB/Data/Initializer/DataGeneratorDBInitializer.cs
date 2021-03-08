using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer.Tables;
using Microsoft.EntityFrameworkCore.Internal;

namespace Master40.DB.Data.Initializer
{
    public class DataGeneratorDBInitializer
    {
        public static void DbInitialize(DataGeneratorContext context)
        {
            context.Database.EnsureCreated();

            var settingOptions = new DataGeneratorTableTransitionMatrixSettingOption();
            settingOptions.Init(context);

            if (context.EdgeWeightRoundModes.Any())
            {
                return;
            }

            var roundModes = new DataGeneratorTableEdgeWeightRoundMode();
            roundModes.Init(context);
        }

    }
}