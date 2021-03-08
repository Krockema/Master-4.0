using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.GeneratorModel;

namespace Master40.DB.Data.Initializer.Tables
{
    public class DataGeneratorTableEdgeWeightRoundMode
    {
        public EdgeWeightRoundMode ROUND_ALWAYS;
        public EdgeWeightRoundMode ROUND_IF_IT_MAKES_SENSE;
        public EdgeWeightRoundMode ROUND_NEVER;

        public DataGeneratorTableEdgeWeightRoundMode()
        {
            ROUND_ALWAYS = new EdgeWeightRoundMode { Name = "always round" };
            ROUND_IF_IT_MAKES_SENSE = new EdgeWeightRoundMode { Name = "round only if it makes sense" };
            ROUND_NEVER = new EdgeWeightRoundMode { Name = "never round" };
        }

        public void Init(DataGeneratorContext ctx)
        {
            var modes = new EdgeWeightRoundMode[]
            {
                ROUND_ALWAYS,
                ROUND_IF_IT_MAKES_SENSE,
                ROUND_NEVER
            };

            ctx.EdgeWeightRoundModes.AddRange(modes);
            ctx.SaveChanges();
        }

        public void Load(DataGeneratorContext ctx)
        {
            ROUND_ALWAYS = ctx.EdgeWeightRoundModes.Single(x => x.Name == ROUND_ALWAYS.Name);
            ROUND_IF_IT_MAKES_SENSE = ctx.EdgeWeightRoundModes.Single(x => x.Name == ROUND_IF_IT_MAKES_SENSE.Name);
            ROUND_NEVER = ctx.EdgeWeightRoundModes.Single(x => x.Name == ROUND_NEVER.Name);
        }

        public static EdgeWeightRoundMode GetEdgeWeightRoundModeByName(DataGeneratorContext ctx, string name)
        {
            return ctx.EdgeWeightRoundModes.SingleOrDefault(x => x.Name == name);
        }

    }
}