using DevExpress.XtraReports.UI;

namespace DevExpressProjectTemplate.PredefinedReports {
    public static class ReportsFactory
    {
        public static readonly Dictionary<string, Func<XtraReport>> Reports = new() {
            ["LargeDatasetReport"] = () => new CachedDocumentSourceReport.Report()
        };

        public static XtraReport GetReport(string reportName) {
            return Reports[reportName]();
        }
    }
}