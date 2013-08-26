using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SMT.Workflow.Platform.Designer.Class;
using SMT.Workflow.Platform.Designer.Common;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Workflow.Platform.Designer.Utils;

namespace SMT.Workflow.Platform.Designer.Dialogs
{
    public partial class FlowTypeDialog : ChildWindow
    {
        public event EventHandler SelectedClick;
        private PlatformService.FlowCategoryClient client = new PlatformService.FlowCategoryClient();
        public PlatformService.FLOW_FLOWCATEGORY type = new PlatformService.FLOW_FLOWCATEGORY();
        public ActionType actionType ;
        public FlowTypeDialog(PlatformService.FLOW_FLOWCATEGORY item, ActionType action)
        {

            InitializeComponent();
            actionType = action;
            if (item != null)
            {
                type = item;
                this.TypeName.Text = type.FLOWCATEGORYDESC;
            }
            client.AddFlowCategoryCompleted += new EventHandler<PlatformService.AddFlowCategoryCompletedEventArgs>(client_AddFlowCategoryCompleted);
            client.GetExistsFlowCategoryCompleted += new EventHandler<PlatformService.GetExistsFlowCategoryCompletedEventArgs>(client_GetExistsFlowCategoryCompleted);
            client.UpdateFlowCategoryCompleted += new EventHandler<PlatformService.UpdateFlowCategoryCompletedEventArgs>(client_UpdateFlowCategoryCompleted);

        }

        void client_UpdateFlowCategoryCompleted(object sender, PlatformService.UpdateFlowCategoryCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result == 1)
                {
                    //MessageBox.Show("流程分类修改成功！");
                    ComfirmWindow.ConfirmationBox("提示信息", "流程分类修改成功！", "确定");

                    if (this.SelectedClick != null)
                    {
                        this.SelectedClick(this, null);
                    }
                    this.Close();
                }
                else
                {
                    ComfirmWindow.ConfirmationBox("提示信息", "流程分类修改失败！", "确定");
                    //MessageBox.Show("流程分类修改失败！");
                }
            }
            else
            {
                //MessageBox.Show("流程分类修改失败！");
                ComfirmWindow.ConfirmationBox("提示信息", "流程分类修改失败！", "确定");
            }
        }

        void client_GetExistsFlowCategoryCompleted(object sender, PlatformService.GetExistsFlowCategoryCompletedEventArgs e)
        {
            if (e.Result < 1)
            {
                if (actionType == ActionType.Add)
                {
                    client.AddFlowCategoryAsync(type);
                }
                else if (actionType == ActionType.Update)
                {
                    client.UpdateFlowCategoryAsync(type);
                }
            }
            else
            {
                //MessageBox.Show("已存在流程分类，请重新输入！");
                ComfirmWindow.ConfirmationBox("提示信息", "流程分类已存在，请重新输入流程分类！", "确定");
            }
        }

        void client_AddFlowCategoryCompleted(object sender, PlatformService.AddFlowCategoryCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result == 1)
                {
                    //MessageBox.Show("流程分类保存成功！");
                    ComfirmWindow.ConfirmationBox("提示信息", "流程分类保存成功！", "确定");

                    if (this.SelectedClick != null)
                    {
                        this.SelectedClick(this, null);
                    }
                    this.Close();
                }
                else
                {
                    //MessageBox.Show("流程分类保存失败！");
                    ComfirmWindow.ConfirmationBox("提示信息", "流程分类保存失败！", "确定");
                }
            }
            else
            {
                //MessageBox.Show("流程分类保存失败！");
                ComfirmWindow.ConfirmationBox("提示信息", "流程分类保存失败！", "确定");
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (actionType == ActionType.Add)
            {
                type.FLOWCATEGORYID = Guid.NewGuid().ToString();
                type.FLOWCATEGORYDESC = this.TypeName.Text.Trim();
                type.COMPANYID = Utility.CurrentUser.OWNERCOMPANYID;
                if (string.IsNullOrEmpty(type.FLOWCATEGORYDESC))
                {
                    //MessageBox.Show("请输入流程分类描述！");
                    ComfirmWindow.ConfirmationBox("提示信息", "流程分类描述不能为空，请输入流程分类！", "确定");
                    this.TypeName.Focus();
                    return;
                }
                client.GetExistsFlowCategoryAsync(this.TypeName.Text.Trim());
            }
            else if (actionType == ActionType.Update)
            {
                type.FLOWCATEGORYDESC = this.TypeName.Text.Trim();
                if (string.IsNullOrEmpty(type.FLOWCATEGORYDESC))
                {
                    //MessageBox.Show("请输入流程分类描述！");
                    ComfirmWindow.ConfirmationBox("提示信息", "流程分类描述不能为空，请输入流程分类！", "确定");
                    this.TypeName.Focus();
                    return;
                }
                client.GetExistsFlowCategoryAsync(this.TypeName.Text.Trim());
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
