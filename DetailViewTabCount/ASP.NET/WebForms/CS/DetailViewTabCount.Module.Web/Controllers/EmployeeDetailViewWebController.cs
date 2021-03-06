﻿using System;
using DetailViewTabCount.Module.BusinessObjects;
using DetailViewTabCount.Module.Helpers;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Web;
using DevExpress.ExpressApp.Web.Editors.ASPx;
using DevExpress.ExpressApp.Web.Layout;
using DevExpress.ExpressApp.Web.Utils;
using DevExpress.Web;

namespace DetailViewTabCount.Module.Web.Controllers {
    public class EmployeeDetailViewWebController : ObjectViewController<DetailView, Employee> {
        private ASPxPageControl pageControl;
        protected override void OnActivated() {
            base.OnActivated();
            View.DelayedItemsInitialization = false;
            View.CurrentObjectChanged += View_CurrentObjectChanged;
            ((WebLayoutManager)View.LayoutManager).PageControlCreated += EmployeeDetailViewWebController_PageControlCreated;
            if(Frame is WebWindow webWindow) {
                webWindow.PagePreRender += CurrentRequestWindow_PagePreRender;
            }
        }
        private void EmployeeDetailViewWebController_PageControlCreated(object sender, PageControlCreatedEventArgs e) {
            // Check this Id in the AppName.Module/Model.DesignedDiffs.xafml file
            if(e.Model.Id == "Tabs") {
                pageControl = e.PageControl;
                pageControl.ClientInstanceName = "pageControl";
            }
        }
        private void CurrentRequestWindow_PagePreRender(object sender, EventArgs e) {
            if(pageControl != null) {
                UpdatePageControl(pageControl);
            }
        }
        private void View_CurrentObjectChanged(object sender, EventArgs e) {
            if(pageControl != null) {
                UpdatePageControl(pageControl);
            }
        }
        private void UpdatePageControl(ASPxPageControl pageControl) {
            //loop through PageControl's tabs
            foreach(TabPage tab in pageControl.TabPages) {
                //remove the item count from the tab caption
                tab.Text = DetailViewControllerHelper.ClearItemCountInTabCaption(tab.Text);
                if(View.FindItem(tab.Name) is ListPropertyEditor listPropertyEditor) {
                    var count = listPropertyEditor.ListView.CollectionSource.GetCount();
                    if(count > 0) {
                        tab.Text = DetailViewControllerHelper.AddItemCountToTabCaption(tab.Text, count);
                    }
                    if(listPropertyEditor.ListView.Editor is ASPxGridListEditor editor && editor.Grid != null) {
                        //Assign the ASPxClientGridView.EndCallback event hander. This is required for inline editing
                        editor.Grid.JSProperties["cpCaption"] = tab.Text;
                        var script =
                        $@"function(s, e) {{
                            if(!s.cpCaption) return;
                            var tab = {pageControl.ClientInstanceName}.GetTabByName('{tab.Name}');
                            tab.SetText(s.cpCaption);
                            delete s.cpCaption
                        }}";
                        ClientSideEventsHelper.AssignClientHandlerSafe(editor.Grid, "EndCallback", script, nameof(EmployeeDetailViewWebController));
                    }
                }
            }
        }
        protected override void OnViewControlsDestroying() {
            pageControl = null;
            base.OnViewControlsDestroying();
        }
        protected override void OnDeactivated() {
            View.CurrentObjectChanged -= View_CurrentObjectChanged;
            ((WebLayoutManager)View.LayoutManager).PageControlCreated -= EmployeeDetailViewWebController_PageControlCreated;
            if(Frame is WebWindow webWindow) {
                webWindow.PagePreRender -= CurrentRequestWindow_PagePreRender;
            }
            base.OnDeactivated();
        }
    }
}
