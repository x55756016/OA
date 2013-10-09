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
using System.Windows.Navigation;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.Collections.ObjectModel;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class JoinSurveying : BaseForm, IClient, IEntityEditor
    {
        #region 全局变量定义

        SmtOAPersonOfficeClient client = null;//WCF服务
        string requireID = string.Empty;
        FormTypes actionTypes = 0;
        ObservableCollection<T_OA_REQUIRERESULT> resultList = null;
        V_EmployeeSurveying surveryingView = null;
        List<string> codeList = null;
        int _answerIndex = 0;
        ObservableCollection<V_EmployeeSurveyInformation> subjectList = null;

        #endregion

        #region 构造函数

        public JoinSurveying(FormTypes actionTypes, string requireID)
        {
            InitializeComponent();
            this.requireID = requireID;
            this.actionTypes = actionTypes;
            this.Loaded += new RoutedEventHandler(JoinSatisfactioning_Loaded);
        }

        #endregion

        #region XAML回调函数

        /// <summary>
        /// 新增答案模板列
        /// </summary>
        private void textAnswer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DataGrid dgParent = new DataGrid();
                TextBox _textBox = (TextBox)sender;
                dgParent = Utility.GetParentObject<DataGrid>(_textBox, "dgAnswer");
                if (dgParent != null)
                {
                    var data = dgParent.ItemsSource as ObservableCollection<T_OA_REQUIREDETAIL>;
                    if (data != null)
                    {
                        data.Add(new T_OA_REQUIREDETAIL { CODE = codeList[_answerIndex], CONTENT = "" });
                        _answerIndex++;
                    }

                }
            }
        }

        /// <summary>
        /// 删除答案
        /// </summary>
        private void answerOperation_Click(object sender, RoutedEventArgs e)
        {
            foreach (var items in subjectList)
            {
                if (items.AnswerList.Count() > 0)
                {
                    if (items.AnswerList.Count() > 1)
                    {
                        T_OA_REQUIREDETAIL tempData = ((Button)sender).DataContext as T_OA_REQUIREDETAIL;
                        items.AnswerList.Remove(tempData);
                        _answerIndex--;
                    }
                    else
                    {
                        items.AnswerList.Clear();
                        items.AnswerList.Add(new T_OA_REQUIREDETAIL() { CODE = "A" });
                    }
                    subjectList.ForEach(child => child.AnswerList = items.AnswerList);
                    return;
                }
            }
        }


        #endregion

        #region 事件完成函数

        void JoinSatisfactioning_Loaded(object sender, RoutedEventArgs e)
        {
            EventResgister();
            codeList = new List<string>() { "B", "C", "D", "E", "F", "G" };
            RefreshUI(RefreshedTypes.ShowProgressBar);
            client.GetDataBySurveyingAsync(requireID);
        }

        void client_AddSurveyingCompleted(object sender, AddSurveyingCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Result)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("INVOLVEDINTHEINVESTIGATIONISSUCSSED"), Utility.GetResourceStr("THANKSFORSURVEYING"), Utility.GetResourceStr("CONFIRM"));
                 return;
            }
            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("INVOLVEDINTHEINVESTIGATIONIFAILED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        }

        void client_GetDataBySurveyingCompleted(object sender, GetDataBySurveyingCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("GETDATAFAILED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            if (e.Result == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("NODATA"), Utility.GetResourceStr("DIDNOTFINDRELEVANT"), Utility.GetResourceStr("CONFIRM"));
                return;
            }
            surveryingView = new V_EmployeeSurveying();
            surveryingView = e.Result;
            Bingding();
        }

        #endregion

        #region 其他函数

        private void CreateAnswerDara()
        {
            foreach (var item in subjectList)
            {
              item.AnswerList = new ObservableCollection<T_OA_REQUIREDETAIL>();
              item.AnswerList.Add(new T_OA_REQUIREDETAIL{CONTENT=""});
            }
        }

        private void Bingding()
        {
            this.parentsInfo.DataContext = surveryingView;
            subjectList = new ObservableCollection<V_EmployeeSurveyInformation>();
            subjectList= surveryingView.SubjectList;
            CreateAnswerDara();
            this.dgQuestion.ItemsSource = subjectList;              
        }

        /// <summary>
        /// 保存
        /// </summary>
        private void Save()
        {
            resultList = new ObservableCollection<T_OA_REQUIRERESULT>();
            foreach (var item in subjectList)
            {
                foreach (var items in item.AnswerList)
                {
                    T_OA_REQUIRERESULT result = new T_OA_REQUIRERESULT();

                    result.CONTENT = textSuggest.Text.Trim().ToString();
                    //result.SUBJECTID = item.Subject.SUBJECTID;
                    result.REQUIRERESULTID = Guid.NewGuid().ToString();
                    result.RESULT = items.CONTENT;
                    Utility.CreateUserInfo<T_OA_REQUIRERESULT>(result);
                    resultList.Add(result);
                }             
            }
            RefreshUI(RefreshedTypes.ShowProgressBar);
            client.AddSurveyingAsync(resultList,surveryingView.MasterID,surveryingView.RequireID);
        }

        /// <summary>
        /// 后台事件注册及全局变量初始化
        /// </summary>
        private void EventResgister()
        {
            client = new SmtOAPersonOfficeClient();
            client.GetDataBySurveyingCompleted += new EventHandler<GetDataBySurveyingCompletedEventArgs>(client_GetDataBySurveyingCompleted);
            client.AddSurveyingCompleted += new EventHandler<AddSurveyingCompletedEventArgs>(client_AddSurveyingCompleted);

        }

        /// <summary>
        /// 动画事件
        /// </summary>
        private void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }

        #endregion

        #region IClient资源回收

        public bool CheckDataContenxChange()
        {
            throw new NotImplementedException();
        }

        public void ClosedWCFClient()
        {
            client.DoClose();
        }

        public void SetOldEntity(object entity)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEntityEditor窗体控制


        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    Save();
                    break;
                case "1":
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
                    break;
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> navigateItems = new List<NavigateItem>() { new NavigateItem { Title = Utility.GetResourceStr("InfoDetail"), Tooltip = Utility.GetResourceStr("InfoDetail") } };
            return navigateItems;
        }

        public string GetStatus()
        {
            return "";
        }

        public string GetTitle()
        {
            return Utility.GetResourceStr("INVOLVEDINTHEINVESTIGATION");
        }

        public List<ToolbarItem> GetToolBarItems()
        {
            if (actionTypes == FormTypes.Browse)
            {
                return new List<ToolbarItem>();
            }
            List<ToolbarItem> tooltbalitems = new List<ToolbarItem>()
            {
                new ToolbarItem
           {DisplayType=ToolbarItemDisplayTypes.Image,Title=Utility.GetResourceStr("SAVE"),Key="0",ImageUrl="/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"},
           new ToolbarItem
           {DisplayType=ToolbarItemDisplayTypes.Image,Title=Utility.GetResourceStr("CLOSE"),Key="1",ImageUrl="/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_Close.png"}
             };
            return tooltbalitems;
        }

        public event UIRefreshedHandler OnUIRefreshed;


        #endregion     

    }
}
