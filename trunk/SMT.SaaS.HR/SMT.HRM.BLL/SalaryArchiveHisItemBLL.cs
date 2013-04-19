using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using SMT.HRM.CustomModel;

namespace SMT.HRM.BLL
{
    public class SalaryArchiveHisItemBLL : BaseBll<T_HR_SALARYARCHIVEHISITEM>
    {
        /// <summary>
        /// 添加薪资档案历史项
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public void SalaryArchiveHisItemAdd(T_HR_SALARYARCHIVEHISITEM obj)
        {
            T_HR_SALARYARCHIVEHISITEM ent = new T_HR_SALARYARCHIVEHISITEM();
            Utility.CloneEntity<T_HR_SALARYARCHIVEHISITEM>(obj, ent);
            ent.T_HR_SALARYARCHIVEHISReference.EntityKey =
                new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYARCHIVEHIS", "SALARYARCHIVEID", obj.T_HR_SALARYARCHIVEHIS.SALARYARCHIVEID);
            dal.Add(ent);
        }

        /// <summary>
        /// 添加薪资档案历史项
        /// </summary>
        /// <param name="salaryarchiveID">薪资档案ID</param>
        /// <param name="salarystandardID">薪资标准ID</param>
        /// <param name="createuserID">创建用户ID</param>
        /// <returns></returns>
        public void SalaryArchiveHisItemsAdd(string salaryarchiveID, string salarystandardID, string createuserID)
        {
            List<T_HR_SALARYITEM> salaryItems = new List<T_HR_SALARYITEM>();
            List<T_HR_SALARYSTANDARDITEM> standardItems = new List<T_HR_SALARYSTANDARDITEM>();
            SalaryStandardItemBLL bll = new SalaryStandardItemBLL();
            standardItems = bll.GetSalaryStandardItemsByStandardID(salarystandardID);
            foreach (var item in standardItems)
            {
                T_HR_SALARYARCHIVEHISITEM archivehisitem = new T_HR_SALARYARCHIVEHISITEM();
                archivehisitem.SALARYARCHIVEITEMID = Guid.NewGuid().ToString();
                archivehisitem.T_HR_SALARYARCHIVEHISReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYARCHIVEHIS", "SALARYARCHIVEID", salaryarchiveID);
                archivehisitem.SALARYSTANDARDID = salarystandardID;
                archivehisitem.SUM = string.IsNullOrEmpty(item.SUM) ? string.Empty : AES.AESEncrypt(item.SUM);
                archivehisitem.CALCULATEFORMULA = item.T_HR_SALARYITEM.CALCULATEFORMULA;
                archivehisitem.CALCULATEFORMULACODE = item.T_HR_SALARYITEM.CALCULATEFORMULACODE;
                archivehisitem.SALARYITEMID = item.T_HR_SALARYITEM.SALARYITEMID;
                archivehisitem.CREATEUSERID = createuserID;
                archivehisitem.CREATEDATE = System.DateTime.Now;
                archivehisitem.ORDERNUMBER = item.ORDERNUMBER;  
                dal.AddToContext(archivehisitem);
            }
            dal.SaveContextChanges();
        }

        /// <summary>
        /// 根据ID获取薪资档案历史项实体集合
        /// </summary>
        /// <param name="ID">薪资档案ID</param>
        /// <returns></returns>
        public IQueryable<V_SALARYARCHIVEITEM> GetSalaryArchiveHisItemByID(string ID)
        {
            IQueryable<V_SALARYARCHIVEITEM> ents = from a in dal.GetObjects<T_HR_SALARYARCHIVEHISITEM>()
                                                   join b in dal.GetObjects<T_HR_SALARYITEM>() on a.SALARYITEMID equals b.SALARYITEMID
                                                   where a.T_HR_SALARYARCHIVEHIS.SALARYARCHIVEID == ID
                                                   select new V_SALARYARCHIVEITEM
                                                   {
                                                       SALARYITEMNAME = b.SALARYITEMNAME,
                                                       SALARYARCHIVEID = a.T_HR_SALARYARCHIVEHIS.SALARYARCHIVEID,
                                                       SALARYARCHIVEITEM = a.SALARYARCHIVEITEMID,
                                                       SALARYSTANDARDID = a.SALARYSTANDARDID,
                                                       SALARYITEMID = b.SALARYITEMID,
                                                       SUM = a.SUM,
                                                       CALCULATEFORMULA = b.CALCULATEFORMULA,
                                                       CALCULATEFORMULACODE = b.CALCULATEFORMULACODE,
                                                       ORDERNUMBER = a.ORDERNUMBER,
                                                       REMARK = a.REMARK
                                                   };
            return ents;
        }
    }
}
