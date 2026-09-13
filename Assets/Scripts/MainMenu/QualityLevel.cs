using System.ComponentModel;

namespace BabyBanjo.Core.Rendering
{
    public enum QualityLevel
    {
        //{"Very Low", "Low", "Medium", "High", "Very High", "Ultra"};
        [Description("Very Low")]
        VeryLow,
        [Description("Low")]
        Low,
        [Description("Medium")]

        Medium,
        [Description("High")]
        High,
        [Description("Very High")]
        VeryHigh,
        [Description("Ultra")]
        Ultra,
    }
}