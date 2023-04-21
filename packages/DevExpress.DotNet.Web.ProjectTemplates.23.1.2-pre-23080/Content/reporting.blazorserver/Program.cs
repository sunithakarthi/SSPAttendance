//#if(add-designer || add-document-viewer) {
using DevExpress.AspNetCore.Reporting;
using Microsoft.EntityFrameworkCore;
using DevExpress.XtraReports.Web.Extensions;
using DevExpress.Security.Resources;
using DevExpress.Blazor.Reporting;
using DevExpressProjectTemplate.Data;
using DevExpressProjectTemplate.Services;
//#endif

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
//#if(add-report-viewer) {
builder.Services.AddDevExpressBlazor();
builder.Services.AddDevExpressServerSideBlazorReportViewer();
builder.Services.Configure<DevExpress.Blazor.Configuration.GlobalOptions>(options => {
    options.BootstrapVersion = DevExpress.Blazor.BootstrapVersion.v5;
});
//#endif
//#if(add-designer || add-document-viewer) {
builder.Services.AddDevExpressBlazorReporting();
builder.Services.AddScoped<ReportStorageWebExtension, CustomReportStorageWebExtension>();
builder.Services.ConfigureReportingServices(configurator => {
//#if(add-designer) {
    configurator.ConfigureReportDesigner(designerConfigurator => {
//#if(add-data-source) {
		designerConfigurator.RegisterDataSourceWizardConnectionStringsProvider<CustomSqlDataSourceWizardConnectionStringsProvider>();
//#endif
//#if(AddJsonDataSourceService) {
		designerConfigurator.RegisterDataSourceWizardJsonConnectionStorage<CustomDataSourceWizardJsonDataConnectionStorage>(true);
//#endif
//#if(AddObjectDataSourceProvider) {
        designerConfigurator.RegisterObjectDataSourceWizardTypeProvider<ObjectDataSourceWizardCustomTypeProvider>();
//#endif
    });
//#endif
//#if(add-document-viewer) {
    configurator.ConfigureWebDocumentViewer(viewerConfigurator => {
        viewerConfigurator.UseCachedReportSourceBuilder();
//#if(AddJsonDataSourceService) {
		viewerConfigurator.RegisterJsonDataConnectionProviderFactory<CustomJsonDataConnectionProviderFactory>();
//#endif
        viewerConfigurator.RegisterConnectionProviderFactory<CustomSqlDataConnectionProviderFactory>();
    });
//#endif
    configurator.UseAsyncEngine();
});
builder.Services.AddDbContext<ReportDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("ReportsDataConnectionString")));
//#endif
builder.WebHost.UseStaticWebAssets();

var app = builder.Build();

//#if(add-designer || add-document-viewer) {
var db = app.Services.GetService<ReportDbContext>();

db.InitializeDatabase();
var contentDirectoryAllowRule = DirectoryAccessRule.Allow(new DirectoryInfo(Path.Combine(app.Environment.ContentRootPath, "..", "Content")).FullName);
AccessSettings.ReportingSpecificResources.TrySetRules(contentDirectoryAllowRule, UrlAccessRule.Allow());
app.UseDevExpressBlazorReporting();
//#endif
//#if(add-designer) {
app.UseReporting(builder => {
    builder.UserDesignerOptions.DataBindingMode = DevExpress.XtraReports.UI.DataBindingMode.Expressions;
});
//#endif

// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
} else {
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseEndpoints(endpoints => {
    endpoints.MapControllers();
    endpoints.MapBlazorHub();
    endpoints.MapFallbackToPage("/_Host");
});

string contentPath = app.Environment.ContentRootPath;
AppDomain.CurrentDomain.SetData("DXResourceDirectory", contentPath);

app.Run();
