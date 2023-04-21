import { Component, Inject, ViewEncapsulation } from '@angular/core';
//#if(EnableClientRichEdit) {
import 'devexpress-reporting/dx-richedit';
//#endif

@Component({
  selector: 'report-designer',
  encapsulation: ViewEncapsulation.None,
  templateUrl: './report-designer.html',
  styleUrls: [
    "../../../node_modules/devextreme/dist/css/dx.light.css",
//#if(EnableClientRichEdit) {
    "../../../node_modules/devexpress-richedit/dist/dx.richedit.css",
//#endif 
    "../../../node_modules/@devexpress/analytics-core/dist/css/dx-analytics.common.css",
    "../../../node_modules/@devexpress/analytics-core/dist/css/dx-analytics.light.css",
    "../../../node_modules/@devexpress/analytics-core/dist/css/dx-querybuilder.css",
    "../../../node_modules/devexpress-reporting/dist/css/dx-webdocumentviewer.css",
    "../../../node_modules/devexpress-reporting/dist/css/dx-reportdesigner.css"
  ]
})

export class ReportDesignerComponent {
  getDesignerModelAction = "DXXRD/GetDesignerModel";
  reportUrl = "TestReport";

  constructor(@Inject('BASE_URL') public hostUrl: string) { }
}