import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { DxReportViewerModule, DxReportDesignerModule } from 'devexpress-reporting-angular';
//#if(add-viewer) {
import { ReportViewerComponent } from './reportviewer/report-viewer';
//#endif
//#if(add-designer) {
import { ReportDesignerComponent } from './reportdesigner/report-designer';
//#endif

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
      HomeComponent,
//#if(add-viewer) {
    ReportViewerComponent,
//#endif
//#if(add-designer) {
    ReportDesignerComponent
//#endif
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
      FormsModule,
//#if(add-viewer) {
    DxReportViewerModule,
//#endif
//#if(add-designer) {
    DxReportDesignerModule,
//#endif
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
//#if(add-designer) {
      { path: 'designer', component: ReportDesignerComponent },
//#endif
//#if(add-viewer) {
      { path: 'viewer', component: ReportViewerComponent }
//#endif
    ])
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
