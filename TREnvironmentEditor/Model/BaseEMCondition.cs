using Newtonsoft.Json;
using System.Collections.Generic;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model
{
    public abstract class BaseEMCondition
    {
        [JsonProperty(Order = -2)]
        public string Comments { get; set; }
        [JsonProperty(Order = -2, DefaultValueHandling = DefaultValueHandling.Include)]
        public EMConditionType ConditionType { get; set; }
        public bool Negate { get; set; }
        public List<BaseEMCondition> And { get; set; }
        public List<BaseEMCondition> Or { get; set; }
        public BaseEMCondition Xor { get; set; }

        public bool GetResult(TRLevel level)
        {
            bool result = Evaluate(level);
            if (Negate)
            {
                result = !result;
            }

            if (And != null)
            {
                And.ForEach(a => result &= a.GetResult(level));
            }
            if (Or != null)
            {
                Or.ForEach(o => result |= o.GetResult(level));
            }
            if (Xor != null)
            {
                result ^= Xor.GetResult(level);
            }

            return result;
        }

        public bool GetResult(TR2Level level)
        {
            bool result = Evaluate(level);
            if (Negate)
            {
                result = !result;
            }

            if (And != null)
            {
                And.ForEach(a => result &= a.GetResult(level));
            }
            if (Or != null)
            {
                Or.ForEach(o => result |= o.GetResult(level));
            }
            if (Xor != null)
            {
                result ^= Xor.GetResult(level);
            }

            return result;
        }

        public bool GetResult(TR3Level level)
        {
            bool result = Evaluate(level);
            if (Negate)
            {
                result = !result;
            }

            if (And != null)
            {
                And.ForEach(a => result &= a.GetResult(level));
            }
            if (Or != null)
            {
                Or.ForEach(o => result |= o.GetResult(level));
            }
            if (Xor != null)
            {
                result ^= Xor.GetResult(level);
            }

            return result;
        }

        protected abstract bool Evaluate(TRLevel level);
        protected abstract bool Evaluate(TR2Level level);
        protected abstract bool Evaluate(TR3Level level);
    }
}