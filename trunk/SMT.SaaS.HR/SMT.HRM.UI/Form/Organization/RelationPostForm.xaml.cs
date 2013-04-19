using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Form.Organization
{
    public partial class RelationPostForm : BaseFloatable
    {
        private T_HR_RELATIONPOST relationPost;

        public T_HR_RELATIONPOST RelationPost
        {
            get { return relationPost; }
            set 
            { 
                relationPost = value;
                this.DataContext = value;
            }
        }
        public SMT.SaaS.FrameworkUI.FormTypes FormType { get; set; }
        public T_HR_POST Post { get; set; }

        OrganizationServiceClient client;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="strID"></param>
        /// <param name="PostID">当前岗位ID</param>
        public RelationPostForm(SMT.SaaS.FrameworkUI.FormTypes type, string strID,T_HR_POST post)
        {
            InitializeComponent();
            FormType = type;
            Post = post;
            InitParas(strID);

            if (Post != null)
            {
                txtPostCode.Text = Post.T_HR_POSTDICTIONARY.POSTCODE;
                txtPostName.Text = Post.T_HR_POSTDICTIONARY.POSTNAME;
            }
        }

        private void InitParas(string strID)
        {
            client = new OrganizationServiceClient();
            client.GetRelationPostByIDCompleted += new EventHandler<GetRelationPostByIDCompletedEventArgs>(client_GetRelationPostByIDCompleted);
            client.RelationPostAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_RelationPostAddCompleted);
            client.RelationPostUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_RelationPostUpdateCompleted);

            if (FormType == SMT.SaaS.FrameworkUI.FormTypes.New)
            {
                RelationPost = new T_HR_RELATIONPOST();
                RelationPost.RELATIONPOSTID = Guid.NewGuid().ToString();
            }
            else
            {
                client.GetRelationPostByIDAsync(strID);
            }
        }

        void client_GetRelationPostByIDCompleted(object sender, GetRelationPostByIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message),
                           Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                V_RELATIONPOST ents = e.Result;
                RelationPost = new T_HR_RELATIONPOST();
                RelationPost.RELATIONPOSTID = ents.RelationPostID;
                RelationPost.RELATEPOSTID = ents.Post.POSTID;
                lkPost.DataContext = ents.Post;
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();

            if (validators.Count > 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Error,Utility.GetResourceStr("ERROR"),validators.Count.ToString() + " invalid validators");
            }
            else
            {
                if (FormType == SMT.SaaS.FrameworkUI.FormTypes.New)
                {
                    RelationPost.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    RelationPost.CREATEDATE = DateTime.Now;
                    RelationPost.T_HR_POST = Post;
                    client.RelationPostAddAsync(RelationPost);
                }
                else
                {
                    RelationPost.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    RelationPost.UPDATEDATE = DateTime.Now;
                    client.RelationPostUpdateAsync(RelationPost);
                }
            }
        }

        void client_RelationPostAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message),
                           Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "RELATIONPOST"));
                this.ReloadData();
                this.Close();
            }
        }

        void client_RelationPostUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message),
                           Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "RELATIONPOST"));
                this.ReloadData();
                this.Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            //this.DialogResult = false;
            //this.Result = false; 2010.4.24更新测试
        }

        private void LookUp_FindClick(object sender, EventArgs e)
        {
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("POSTCODE", "T_HR_POSTDICTIONARY.POSTCODE");
            cols.Add("POSTNAME", "T_HR_POSTDICTIONARY.POSTNAME");

            LookupForm lookup = new LookupForm(EntityNames.Post,
                typeof(T_HR_POST[]), cols);

            lookup.SelectedClick += (o, ev) =>
            {
                T_HR_POST ent = lookup.SelectedObj as T_HR_POST;

                if (ent != null)
                {
                    lkPost.DataContext = ent;
                    HandlePostChanged(ent);
                }
            };
            
            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) =>{});
        }

        private void HandlePostChanged(T_HR_POST ent)
        {
            //给关联岗位赋值
            RelationPost.RELATEPOSTID = ent.POSTID;
        }
    }
}

