using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Objects.DataClasses;
using SMT_FB_EFModel;
using SMT.FB.BLL;
using System.Drawing;
using System.Text;
using SMT.FB.DAL;
using System.Threading;

namespace SMT.FB.Services
{
    public partial class ReDoForm : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            entityTypeList.Add(typeof(T_FB_COMPANYBUDGETSUMMASTER).Name);
            entityTypeList.Add(typeof(T_FB_COMPANYBUDGETMODMASTER).Name);
            entityTypeList.Add(typeof(T_FB_COMPANYTRANSFERMASTER).Name);

            entityTypeList.Add(typeof(T_FB_DEPTBUDGETSUMMASTER).Name);
            entityTypeList.Add(typeof(T_FB_DEPTBUDGETADDMASTER).Name);
            entityTypeList.Add(typeof(T_FB_DEPTTRANSFERMASTER).Name);

            entityTypeList.Add(typeof(T_FB_PERSONMONEYASSIGNMASTER).Name);

            entityTypeList.Add(typeof(T_FB_BORROWAPPLYMASTER).Name);
            entityTypeList.Add(typeof(T_FB_REPAYAPPLYMASTER).Name);

            entityTypeList.Add(typeof(T_FB_CHARGEAPPLYMASTER).Name);
            entityTypeList.Add(typeof(T_FB_TRAVELEXPAPPLYMASTER).Name);
            FBCommonService fs = new FBCommonService();
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            
            StringBuilder sb = new StringBuilder();
            BudgetAccountBLL bll = new BudgetAccountBLL();
            try
            {
               
                var data = GetEntities();

                bll.BeginTransaction();
                entityTypeList.ForEach(item =>
                {

                    data[item].ForEach(entity =>
                    {
                       
                        try
                        {
                            sb.AppendLine(item + " : " + entity.GetOrderID());
                            CheckStates cstates = (CheckStates)int.Parse(entity.GetValue(FieldName.CheckStates).ToString());

                            var master = (entity as T_FB_DEPTBUDGETSUMMASTER);
                            if (master != null )
                            {

                                if (!master.T_FB_DEPTBUDGETSUMDETAIL.IsLoaded) { master.T_FB_DEPTBUDGETSUMDETAIL.Load(); }
                                master.T_FB_DEPTBUDGETSUMDETAIL.ToList().ForEach(itemd =>
                                    {
                                        if (!itemd.T_FB_DEPTBUDGETAPPLYMASTERReference.IsLoaded) { itemd.T_FB_DEPTBUDGETAPPLYMASTERReference.Load(); }
                                        bll.UpdateAccount(itemd.T_FB_DEPTBUDGETAPPLYMASTER.ToFBEntity(), CheckStates.Approving);
                                    });
                            }
                            bll.UpdateAccount(entity.ToFBEntity(), CheckStates.Approving);
  
                            if (cstates == CheckStates.Approved)
                            {
                                bll.UpdateAccount(entity.ToFBEntity(), CheckStates.Approved);
                            }
                            
                        }
                        catch (Exception exx)
                        {
                            throw exx;
                        }

                    });
                });
                bll.CommitTransaction();
            }
            catch (Exception ex)
            {
                bll.RollbackTransaction();
                sb.AppendLine();
                sb.AppendLine(ex.ToString());
            }
            finally
            {
                this.tbRemark.Text = sb.ToString();
            }
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            var data = GetEntities();
            string msg = "";
            foreach (var item in data)
            {
                msg += item.Key + " 记录数 : " + item.Value.Count.ToString() + "\r\n";
            }
            this.tbRemark.Text = msg;
        }

        List<string> entityTypeList = new List<string>();
        public Dictionary<string, List<EntityObject>> GetEntities()
        {
            Dictionary<string, List<EntityObject>> result = new Dictionary<string, List<EntityObject>>();
            entityTypeList.AsParallel().ForAll(item =>
                {
                    var data = GetEntities(item);
                    result.Add(item, data);
                });
            return result;
        }
        public List<EntityObject> GetEntities(string entityType)
        {
            using (BudgetAccountBLL bll = new BudgetAccountBLL())
            {
                DateTime dtStart = DateTime.Parse(this.tbStart.Text);
                DateTime dtEnd = DateTime.Parse(this.tbEnd.Text);

                QueryExpression qeStart = new QueryExpression();
                qeStart.PropertyName = FieldName.UpdateDate;
                qeStart.PropertyValue = this.tbStart.Text;
                qeStart.Operation = QueryExpression.Operations.GreaterThanOrEqual;

                QueryExpression qeEnd = new QueryExpression();
                qeEnd.PropertyName = FieldName.UpdateDate;
                qeEnd.PropertyValue = this.tbEnd.Text;
                qeEnd.Operation = QueryExpression.Operations.LessThanOrEqual;

                QueryExpression qeCheckStatesPass = QueryExpression.Equal(FieldName.CheckStates, "2");
                QueryExpression qeCheckStatesPassing = QueryExpression.Equal(FieldName.CheckStates, "1");

                qeStart.RelatedExpression = qeEnd;
                qeEnd.RelatedExpression = qeCheckStatesPass;
                qeCheckStatesPass.RelatedType = QueryExpression.RelationType.Or;
                qeCheckStatesPass.RelatedExpression = qeCheckStatesPassing;

                qeStart.QueryType = entityType;

                qeStart.OrderByExpression = new OrderByExpression { OrderByType = OrderByType.Asc, PropertyName = FieldName.UpdateDate };
                qeStart.IsNoTracking = true;
                return qeStart.Query(bll);

            }
        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            
            FBService fs = new FBService();
            fs.CloseBudget();
        }
    }
}