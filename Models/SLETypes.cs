namespace EYDGateway.Models
{
    public static class SLETypes
    {
        public const string CBD = "CBD";
        public const string DOPS = "DOPS";
        public const string MiniCEX = "MiniCEX";
        public const string DOPSSim = "DOPSSim";
        public const string DtCT = "DtCT";
        public const string DENTL = "DENTL";

        public static readonly Dictionary<string, string> TypeNames = new()
        {
            { CBD, "Case-Based Discussion (CBD)" },
            { DOPS, "Direct Observation of Procedural Skills (DOPS)" },
            { MiniCEX, "Mini Clinical Evaluation Exercise (Mini-CEX)" },
            { DOPSSim, "Simulated Direct Observation of Procedural Skills (Simulated DOPS)" },
            { DtCT, "Developing the Clinical Teacher (DtCT)" },
            { DENTL, "Direct Evaluation of Non-Technical Learning (DENTL)" }
        };

        public static string GetTypeName(string typeCode)
        {
            return TypeNames.TryGetValue(typeCode, out var name) ? name : typeCode;
        }

        public static bool RequiresSingleEPA(string sleType)
        {
            var singleEPATypes = new[] { MiniCEX, DOPS, DOPSSim };
            return singleEPATypes.Contains(sleType);
        }
    }
}
