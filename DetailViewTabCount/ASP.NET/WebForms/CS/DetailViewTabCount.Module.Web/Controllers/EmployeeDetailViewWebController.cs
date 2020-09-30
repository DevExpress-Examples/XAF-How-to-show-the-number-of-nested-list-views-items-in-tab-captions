﻿using System;
using System.Collections.Generic;
using DetailViewTabCount.Module.Helpers;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Web.Editors.ASPx;
using DevExpress.ExpressApp.Web.Layout;
using DevExpress.Web;

namespace DetailViewTabCount.Module.Web.Controllers
{
    public partial class EmployeeDetailViewWebController : ViewController<DetailView>
    {
        ASPxPageControl pageControl;
        Dictionary<string, string> gridsInTabs;
        public EmployeeDetailViewWebController()
        {
            InitializeComponent();
            gridsInTabs = new Dictionary<string, string>();
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            View.DelayedItemsInitialization = false;
            ((WebLayoutManager)View.LayoutManager).PageControlCreated += EmployeeDetailViewWebController_PageControlCreated;
            View.CustomizeViewItemControl<ListPropertyEditor>(this, (editor) => {
                editor.ListView.ControlsCreated += ListView_ControlsCreated;
            });
        }

        protected override void OnDeactivated()
        {
            ((WebLayoutManager)View.LayoutManager).PageControlCreated -= EmployeeDetailViewWebController_PageControlCreated;
            pageControl.Dispose();
            base.OnDeactivated();
        }

        private void EmployeeDetailViewWebController_PageControlCreated(object sender, PageControlCreatedEventArgs e)
        {
            // Check this Id in the AppName.Module/Model.DesignedDiffs.xafml file
            if (e.Model.Id == "Tabs")
            {
                pageControl = e.PageControl;
                pageControl.ClientInstanceName = "pageControl";
            }
        }

        private void ListView_ControlsCreated(object sender, EventArgs e)
        {
            ASPxGridListEditor gridListEditor = ((ListView)sender).Editor as ASPxGridListEditor;
            if (gridListEditor != null)
            {
                if (gridListEditor.Grid != null)
                {
                    if (!gridsInTabs.ContainsKey(gridListEditor.Grid.ID))
                    {
                        gridsInTabs.Add(gridListEditor.Grid.ID, gridListEditor.Name);
                    }
                    gridListEditor.Grid.DataBound += Grid_DataBound;
                }
            }
        }

        private void Grid_DataBound(object sender, EventArgs e)
        {
            var grid = sender as ASPxGridView;

            if (pageControl != null)
            {

                foreach (TabPage tab in pageControl.TabPages)
                {
                    bool isBold = false;
                    var tabCaption = gridsInTabs[grid.ID];
                    var count = 0;

                    if (DetailViewControllerHelper.ClearItemCountInTabCaption(tab.Text) != tabCaption) continue;

                    var listPropertyEditor = View.FindItem(tabCaption.Replace(" ", "")) as ListPropertyEditor;
                    if (listPropertyEditor != null)
                        count = listPropertyEditor.ListView.CollectionSource.GetCount();

                    tab.Text = DetailViewControllerHelper.ClearItemCountInTabCaption(tab.Text);

                    if (count > 0)
                    {
                        isBold = true;
                        tab.Text += " (" + count + ")";
                        grid.JSProperties["cpCaption"] = tab.Text;
                        grid.ClientSideEvents.EndCallback = $"function(s, e) {{var tab = {pageControl.ClientInstanceName}.GetTabByName('{tab.Name}'); tab.SetText(s.cpCaption); delete s.cpCaption;}}";
                    }
                    tab.TabStyle.Font.Bold = isBold;
                }
            }
        }
    }
}