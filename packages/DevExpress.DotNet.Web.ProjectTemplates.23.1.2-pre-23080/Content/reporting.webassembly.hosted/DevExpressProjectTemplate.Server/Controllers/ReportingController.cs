using DevExpress.AspNetCore.Reporting.QueryBuilder;
using DevExpress.AspNetCore.Reporting.QueryBuilder.Native.Services;
using DevExpress.AspNetCore.Reporting.ReportDesigner;
using DevExpress.AspNetCore.Reporting.ReportDesigner.Native.Services;
using DevExpress.AspNetCore.Reporting.WebDocumentViewer;
using DevExpress.AspNetCore.Reporting.WebDocumentViewer.Native.Services;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Web.ReportDesigner;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace DevExpressProjectTemplate.Controllers {
    public class CustomWebDocumentViewerController : WebDocumentViewerController {
        public CustomWebDocumentViewerController(IWebDocumentViewerMvcControllerService controllerService) : base(controllerService) {
        }
    }

    public class CustomReportDesignerController : ReportDesignerController {
        public CustomReportDesignerController(IReportDesignerMvcControllerService controllerService) : base(controllerService) {
        }

        [HttpPost("[action]")]
        public object GetReportDesignerModel(
            [FromForm] string reportUrl,
            [FromForm] ReportDesignerSettingsBase designerModelSettings,
            [FromServices] IReportDesignerClientSideModelGenerator designerClientSideModelGenerator) {
            Dictionary<string, object> dataSources = new Dictionary<string, object>();
            SqlDataSource ds = new SqlDataSource("NWindConnectionString");
            dataSources.Add("sqlDataSource1", ds);
            ReportDesignerModel model;
            if(string.IsNullOrEmpty(reportUrl))
                model = designerClientSideModelGenerator.GetModel(new XtraReport(), dataSources, "/DXXRD", "/DXXRDV", "/DXXQB");
            else
                model = designerClientSideModelGenerator.GetModel(reportUrl, dataSources, "/DXXRD", "/DXXRDV", "/DXXQB");
            model.WizardSettings.EnableSqlDataSource = true;
            model.Assign(designerModelSettings);
            var modelJsonScript = designerClientSideModelGenerator.GetJsonModelScript(model);
            return Content(modelJsonScript, "application/json");
        }
    }

    public class CustomQueryBuilderController : QueryBuilderController {
        public CustomQueryBuilderController(IQueryBuilderMvcControllerService controllerService) : base(controllerService) {
        }
    }
}