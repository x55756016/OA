using System;
using System.Windows;
using System.Windows.Controls;


namespace SMT.Workflow.Platform.UI
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {

            InitializeComponent();
            System.Windows.Controls.Window.Parent = windowParent;
            System.Windows.Controls.Window.TaskBar = new StackPanel();
            System.Windows.Controls.Window.Wrapper = this;
            System.Windows.Controls.Window.IsShowtitle = false;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FlowProbar.Start();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            FlowProbar.Stop();       
        }

        string checkid = "0";
        private void lkEmpName_FindClick(object sender, EventArgs e)
        {
            OrganizationControl.OrganizationLookup lookup = new OrganizationControl.OrganizationLookup();
            //固定当前用户ID
            lookup.CurrentUserID = "0276288d-ab8e-41ed-abc5-cee659e0909f";

            try
            {
                lookup.SelectedClick += (obj, ev) =>
                {
                    switch (checkid)
                    {
                        case "1":
                            lookup.SelectedObjType = OrgTreeItemTypes.Company;
                            SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY ent = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;
                            if (ent != null)
                            {
                                tname2.Text = ent.CNAME;
                            }
                            break;
                        case "2":
                            lookup.SelectedObjType = OrgTreeItemTypes.Department;
                           
                            SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT Dep = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT;
                            if (Dep != null)
                            {
                                tname2.Text = Dep.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                            }
                            break;
                        case "3":
                            lookup.SelectedObjType = OrgTreeItemTypes.Post;
                            SMT.Saas.Tools.OrganizationWS.T_HR_POST post = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_POST;
                            if (post != null)
                            {
                                tname2.Text = post.T_HR_POSTDICTIONARY.POSTNAME;
                            }
                            break;
                        default:
                            lookup.SelectedObjType = OrgTreeItemTypes.Company;
                            SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY ent1 = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;
                            if (ent1 != null)
                            {
                                tname2.Text = ent1.CNAME;
                            }
                            break;
                    }
                };
            }
            catch (Exception ex)
            {
                string _text = "";

                MessageWindow.Show<string>("错误信息", ex.ToString(), MessageIcon.Error, result => _text = result, "Default", "确定");
            }

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            checkid = (sender as RadioButton).Tag.ToString();
        }
    }
}
