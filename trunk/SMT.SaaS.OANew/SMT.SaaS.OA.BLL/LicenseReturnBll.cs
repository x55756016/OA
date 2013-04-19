using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;
using SMT.SaaS.OA.DAL.Views;
using System.Linq.Dynamic;

namespace SMT.SaaS.OA.BLL
{
    public class LicenseReturnBll : BaseBll<T_OA_LICENSEUSER>
    {
        //SMT_OA_EFModelContext context = new SMT_OA_EFModelContext();


        public IQueryable<T_OA_LICENSEUSER> GetLicenseBorrowAppListQueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string lendFlag, string userID)
        {

            string checkState = ((int)CheckStates.Approved).ToString();

            var entity = from q in dal.GetObjects().Include("T_OA_LICENSEMASTER")
                         where q.CHECKSTATE == checkState && q.HASRETURN == "0"
                         select q;
            if (lendFlag != "5")
            {
                entity = entity.Where(p => p.T_OA_LICENSEMASTER.LENDFLAG == lendFlag);
            }
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "OAORGANLICENUSER");//  OAORGANLICENUSERRETURN
            if (!string.IsNullOrEmpty(filterString))
            {
                entity = entity.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
            }
            entity = entity.OrderBy(sort);
            entity = Utility.Pager<T_OA_LICENSEUSER>(entity, pageIndex, pageSize, ref pageCount);
            return entity;


        }



        public bool LendOrReturn(T_OA_LICENSEUSER licenseObj, string action)
        {

            try
            {

                T_OA_LICENSEUSER entity = dal.GetObjectByEntityKey(licenseObj.EntityKey) as T_OA_LICENSEUSER;
                entity.T_OA_LICENSEMASTER = dal.GetObjectByEntityKey(licenseObj.T_OA_LICENSEMASTER.EntityKey) as T_OA_LICENSEMASTER;
                //context.ApplyPropertyChanges(licenseObj.EntityKey.EntitySetName, licenseObj);
                int i = dal.Update(licenseObj);

                var ent = dal.GetObjects<T_OA_LICENSEMASTER>().FirstOrDefault(s => s.LICENSEMASTERID == entity.T_OA_LICENSEMASTER.LICENSEMASTERID);

                if (action == SMT.SaaS.OA.DAL.Action.Lend.ToString())
                {
                    ent.LENDFLAG = "1";

                }
                else
                {
                    ent.LENDFLAG = "0";
                }
                ent.UPDATEDATE = licenseObj.UPDATEDATE;
                ent.UPDATEUSERID = licenseObj.UPDATEUSERID;
                ent.UPDATEUSERNAME = licenseObj.UPDATEUSERNAME;
                int k = dal.Update(ent);
                if (k > 0 && i > 0)
                {
                    return true;
                    //引擎通知归还证件
                    if (ent.LENDFLAG == "1") //证件被借出
                    {
                        List<object> objArds = new List<object>();
                        T_OA_LICENSEUSER record = new T_OA_LICENSEUSER();
                        objArds.Add(record.LICENSEUSERID);
                        objArds.Add("OA");
                        objArds.Add("hireAppObj.HIREAPPID");
                        objArds.Add(record.LICENSEUSERID);
                        objArds.Add(DateTime.Now.AddDays((int)ent.DAY).ToString("yyyy/MM/d"));
                        objArds.Add(DateTime.Now.ToString("HH:mm"));
                        objArds.Add("Day");
                        objArds.Add("");
                        objArds.Add(ent.LICENSENAME + "证照归还时间：" + Convert.ToDateTime(entity.ENDDATE).ToString("yyyy-MM-dd") + ",归还");
                        objArds.Add("");
                        objArds.Add(Utility.strEngineFuncWSSite);
                        objArds.Add("EventTriggerProcess");
                        objArds.Add("<Para FuncName=\"UpdateEmployeeWorkAgeByID\" Name=\"LICENSEUSERID\" Value=\"" + record.LICENSEUSERID + "\"></Para>");
                        objArds.Add("Г");
                        objArds.Add("CustomBinding");

                        Utility.SendEngineEventTriggerData(objArds);

                    }
                }
                return false;

            }
            catch (Exception ex)
            {
                //dal.RollbackTransaction();
                //return false;
                throw (ex);
            }
            //}
            //return false;
        }
    }
}
