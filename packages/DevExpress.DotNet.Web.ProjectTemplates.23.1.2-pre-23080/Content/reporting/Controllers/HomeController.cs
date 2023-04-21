using Microsoft.AspNetCore.Mvc;
//#if(add-data-source) {
using System.Collections.Generic;
using DevExpress.DataAccess.Sql;
//#endif

namespace DevExpressProjectTemplate.Controllers {
    public class HomeController : Controller {
        public IActionResult Index() {
            return View();
        }
        public IActionResult Error() {
            Models.ErrorModel model = new Models.ErrorModel();
            return View(model);
        }
//#if(add-designer) {

        public IActionResult Designer() {
            Models.ReportDesignerModel model = new Models.ReportDesignerModel();
//#if(add-data-source) {
            // Create a SQL data source with the specified connection string.
            SqlDataSource ds = new SqlDataSource("NWindConnectionString");
            // Create a SQL query to access the Products data table.
            SelectQuery query = SelectQueryFluentBuilder.AddTable("Products").SelectAllColumnsFromTable().Build("Products");
            ds.Queries.Add(query);
            ds.RebuildResultSchema();
            model.DataSources = new Dictionary<string, object>();
            model.DataSources.Add("Northwind", ds);
//#endif
            return View(model);
        }
//#endif
//#if(add-viewer) {

        public IActionResult Viewer() {
            return View();
        }
//#endif
    }
}