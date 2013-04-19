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
    public class CustomGuerdonBLL:BaseBll<T_HR_CUSTOMGUERDON>
    {

        /// <summary>
        /// 根据自定义薪资ID查询实体
        /// </summary>
        /// <param name="CustomGuerdonSetID">自定义薪资ID</param>
        /// <returns>返回自定义薪资实体</returns>
        public T_HR_CUSTOMGUERDON GetCustomGuerdonByID(string CustomGuerdonID)
        {
            var ents = from a in dal.GetTable()
                       where a.CUSTOMGUERDONID == CustomGuerdonID
                       select a;
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }

        public List<V_CUSTOMGUERDON> GetCustomGuerdon(string SalaryStandardID)
        {
            var ents = from a in dal.GetObjects<T_HR_CUSTOMGUERDON>().Include("T_HR_SALARYSTANDARD").Include("T_HR_CUSTOMGUERDONSET")
                       where a.T_HR_SALARYSTANDARD.SALARYSTANDARDID == SalaryStandardID
                       select a;
            List<V_CUSTOMGUERDON> CustomGuerdonlist = new List<V_CUSTOMGUERDON>();
            foreach(T_HR_CUSTOMGUERDON en in ents)
            {
                V_CUSTOMGUERDON ent = new V_CUSTOMGUERDON();
                ent.CUSTOMGUERDONID = en.CUSTOMGUERDONID;
                ent.CUSTOMGUERDONSETID = en.T_HR_CUSTOMGUERDONSET.CUSTOMGUERDONSETID;
                ent.CALCULATORTYPE = en.T_HR_CUSTOMGUERDONSET.CALCULATORTYPE;
                ent.GUERDONCATEGORY = en.T_HR_CUSTOMGUERDONSET.GUERDONCATEGORY;
                ent.GUERDONNAME = en.T_HR_CUSTOMGUERDONSET.GUERDONNAME;
                ent.GUERDONSUM = en.T_HR_CUSTOMGUERDONSET.GUERDONSUM;
                ent.SALARYSTANDARDID = en.T_HR_SALARYSTANDARD.SALARYSTANDARDID;
                ent.SALARYSTANDARDNAME = en.T_HR_SALARYSTANDARD.SALARYSTANDARDNAME;
                CustomGuerdonlist.Add(ent);
            }
            return CustomGuerdonlist;
        }


        /// <summary>
        /// 删除自定义薪资记录，可同时删除多行记录
        /// </summary>
        /// <param name="CustomGuerdons">自定义薪资设置ID数组</param>
        /// <returns></returns>
        public int CustomGuerdonDelete(string[] CustomGuerdons)
        {
            foreach (string id in CustomGuerdons)
            {
                var ents = from e in dal.GetObjects<T_HR_CUSTOMGUERDON>()
                           where e.CUSTOMGUERDONID == id
                           select e;
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                if (ent != null)
                {
                    dal.DeleteFromContext(ent);
                }
            }
            return dal.SaveContextChanges(); 
        }

        /// <summary>
        /// 新增定义薪资信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        public string CreateCustomGuerdon(T_HR_CUSTOMGUERDON entTemp)
        {
            string strMsg = string.Empty;
            try
            {
                if (entTemp == null)
                {
                    return "REQUIREDFIELDS";
                }

                var ents = from a in dal.GetObjects<T_HR_SALARYSTANDARD>()
                           where a.SALARYSTANDARDID == entTemp.T_HR_SALARYSTANDARD.SALARYSTANDARDID
                           select a;
                //var ents = from a in DataContext.T_HR_SALARYSTANDARD
                //           join b in DataContext.T_HR_CUSTOMGUERDON on a.SALARYSTANDARDID equals b.T_HR_SALARYSTANDARD.SALARYSTANDARDID
                //           where a.SALARYSTANDARDID == entTemp.T_HR_SALARYSTANDARD.SALARYSTANDARDID
                //           select a;
                if(ents.Count()<=0)
                {
                   return "NOSALARYSTANDARDID";
                }

                T_HR_CUSTOMGUERDON ent = new T_HR_CUSTOMGUERDON();
                Utility.CloneEntity<T_HR_CUSTOMGUERDON>(entTemp, ent);
                ent.T_HR_CUSTOMGUERDONSETReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_CUSTOMGUERDONSET", "CUSTOMGUERDONSETID", entTemp.T_HR_CUSTOMGUERDONSET.CUSTOMGUERDONSETID);
                ent.T_HR_SALARYSTANDARDReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSTANDARD", "SALARYSTANDARDID", entTemp.T_HR_SALARYSTANDARD.SALARYSTANDARDID);

                Utility.RefreshEntity(ent);

                dal.Add(ent);

                strMsg = "SAVESUCCESSED";

            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

    }
}
