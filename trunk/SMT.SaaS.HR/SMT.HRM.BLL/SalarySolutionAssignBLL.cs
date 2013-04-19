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
    public class SalarySolutionAssignBLL : BaseBll<T_HR_SALARYSOLUTIONASSIGN>, IOperate
    {
        //public void SalarySolutionAssignAdd(T_HR_SALARYSOLUTIONASSIGN obj)
        //{
        //    T_HR_SALARYSOLUTIONASSIGN tmpEnt = new T_HR_SALARYSOLUTIONASSIGN();
        //    Utility.CloneEntity<T_HR_SALARYSOLUTIONASSIGN>(obj, tmpEnt);

        //    tmpEnt.T_HR_SALARYSOLUTIONReference.EntityKey
        //        = new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSOLUTION", "SALARYSOLUTIONID", obj.T_HR_SALARYSOLUTION.SALARYSOLUTIONID);
        //    //tmpEnt.T_HR_SALARYSTANDARDReference.EntityKey
        //    //    = new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSTANDARD", "SALARYSTANDARDID", obj.T_HR_SALARYSTANDARD.SALARYSTANDARDID);

        //    dal.Add(tmpEnt);
        //}
        public void SalarySolutionAssignAdd(T_HR_SALARYSOLUTIONASSIGN obj)
        {
            T_HR_SALARYSOLUTIONASSIGN tmpEnt = new T_HR_SALARYSOLUTIONASSIGN();
            bool isExist = false;
            var ent = from c in dal.GetObjects<T_HR_SALARYSOLUTIONASSIGN>()
                      where c.ASSIGNEDOBJECTID == obj.ASSIGNEDOBJECTID
                      select c;
            if (ent.Count() > 0)
            {
                tmpEnt = ent.FirstOrDefault();
                isExist = true;
            }
            Utility.CloneEntity<T_HR_SALARYSOLUTIONASSIGN>(obj, tmpEnt);

            tmpEnt.T_HR_SALARYSOLUTIONReference.EntityKey
                = new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSOLUTION", "SALARYSOLUTIONID", obj.T_HR_SALARYSOLUTION.SALARYSOLUTIONID);
            //tmpEnt.T_HR_SALARYSTANDARDReference.EntityKey
            //    = new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSTANDARD", "SALARYSTANDARDID", obj.T_HR_SALARYSTANDARD.SALARYSTANDARDID);
            if (isExist == true)
            {
                dal.Update(tmpEnt);
            }
            else
            {
                dal.Add(tmpEnt);
            }
        }
        public void SalarySolutionAssignUpdate(T_HR_SALARYSOLUTIONASSIGN obj)
        {

            var ent = from a in dal.GetTable()
                      where a.SALARYSOLUTIONASSIGNID == obj.SALARYSOLUTIONASSIGNID
                      select a;
            if (ent.Count() > 0)
            {
                T_HR_SALARYSOLUTIONASSIGN tmpEnt = ent.FirstOrDefault();

                Utility.CloneEntity<T_HR_SALARYSOLUTIONASSIGN>(obj, tmpEnt);

                tmpEnt.T_HR_SALARYSOLUTIONReference.EntityKey
                    = new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSOLUTION", "SALARYSOLUTIONID", obj.T_HR_SALARYSOLUTION.SALARYSOLUTIONID);
                //tmpEnt.T_HR_SALARYSTANDARDReference.EntityKey
                //    = new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSTANDARD", "SALARYSTANDARDID", obj.T_HR_SALARYSTANDARD.SALARYSTANDARDID);

                dal.Update(tmpEnt);
            }

        }
        public int SalarySolutionAssignDelete(string[] IDs)
        {

            foreach (string id in IDs)
            {
                var ents = from e in dal.GetObjects<T_HR_SALARYSOLUTIONASSIGN>()
                           where e.SALARYSOLUTIONASSIGNID == id
                           select e;
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                if (ent != null)
                {
                    dal.DeleteFromContext(ent);
                    //DataContext.DeleteObject(ent);
                }

            }

            return dal.SaveContextChanges();
        }
        /// <summary>
        /// 根据分配对象的ID获取方案ID
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public string GetSolutionIDByAssignObjectID(string ID)
        {
            var ents = from o in dal.GetObjects<T_HR_SALARYSOLUTIONASSIGN>().Include("T_HR_SALARYSOLUTION")
                       where o.ASSIGNEDOBJECTID == ID
                       select o;

            return ents.Count() > 0 ? ents.FirstOrDefault().T_HR_SALARYSOLUTION.SALARYSOLUTIONID : null;
        }

        public T_HR_SALARYSOLUTIONASSIGN GetSalarySolutionAssignByID(string ID)
        {
            var ents = from o in dal.GetObjects<T_HR_SALARYSOLUTIONASSIGN>().Include("T_HR_SALARYSOLUTION")
                       where o.SALARYSOLUTIONASSIGNID == ID
                       select o;

            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }

        public V_SALARYSOLUTIONASSIGN GetSalarySolutionAssignViewByID(string ID)
        {
            var ents = from o in dal.GetObjects<T_HR_SALARYSOLUTIONASSIGN>().Include("T_HR_SALARYSOLUTION")
                       where o.SALARYSOLUTIONASSIGNID == ID
                       select o;

            T_HR_SALARYSOLUTIONASSIGN ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

            V_SALARYSOLUTIONASSIGN vent = new V_SALARYSOLUTIONASSIGN();
            vent.SalarySolutionAssign = ent;
            vent.AssignObjectName = GetAssignObjectName(ent.ASSIGNEDOBJECTTYPE, ent.ASSIGNEDOBJECTID);

            return vent;
        }

        public List<V_SALARYSOLUTIONASSIGN> SalarySolutionAssignPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            List<object> queryParas = new List<object>();
            List<object> qParas = new List<object>();
            string filterStrings = string.Empty;
            if(!string.IsNullOrEmpty(filterString))
            {
                var entsol = from a in dal.GetObjects<T_HR_SALARYSOLUTION>()
                             select a;
                entsol = entsol.Where(filterString, paras.ToArray());
                if (entsol.Count() > 0)
                {
                    foreach (var entt in entsol)
                    {
                        if (!string.IsNullOrEmpty(filterStrings)) filterStrings += " OR ";
                        filterStrings += "T_HR_SALARYSOLUTION.SALARYSOLUTIONID==@" + qParas.Count().ToString();
                        qParas.Add(entt.SALARYSOLUTIONID);
                    }

                }
                else
                    return null;
            }
            else
            {
               filterStrings = filterString;
               qParas.AddRange(paras);
            }
            queryParas.AddRange(qParas);
            SetOrganizationFilter(ref filterStrings, ref queryParas, userID, "T_HR_SALARYSOLUTIONASSIGN");
            IQueryable<T_HR_SALARYSOLUTIONASSIGN> ents = dal.GetObjects<T_HR_SALARYSOLUTIONASSIGN>().Include("T_HR_SALARYSOLUTION");
            if (!string.IsNullOrEmpty(filterStrings))
            {
                ents = ents.Where(filterStrings, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_SALARYSOLUTIONASSIGN>(ents, pageIndex, pageSize, ref pageCount);

            List<T_HR_SALARYSOLUTIONASSIGN> tmplist = ents.ToList();


            List<V_SALARYSOLUTIONASSIGN> vents = new List<V_SALARYSOLUTIONASSIGN>();
            var tmps = from e in tmplist
                       select new V_SALARYSOLUTIONASSIGN
                       {
                           SalarySolutionAssign = e,
                           AssignObjectName = GetAssignObjectName(e.ASSIGNEDOBJECTTYPE, e.ASSIGNEDOBJECTID)
                       };
            vents = tmps.ToList();
            return vents;
        }
        private string GetAssignObjectName(string type, string objectID)
        {
            string name = "";

            int objectType = -1;

            int.TryParse(type, out objectType);

            switch ((AssignObjectType)objectType)
            {
                case AssignObjectType.Company:
                    CompanyBLL cbll = new CompanyBLL();
                    T_HR_COMPANY company = cbll.GetCompanyById(objectID);
                    name = (company == null) ? "" : company.CNAME;
                    break;

                case AssignObjectType.Department:
                    DepartmentBLL dbll = new DepartmentBLL();
                    T_HR_DEPARTMENT depart = dbll.GetDepartmentById(objectID);
                    name = (depart == null) ? "" : depart.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                    break;

                case AssignObjectType.Post:
                    PostBLL pbll = new PostBLL();
                    T_HR_POST post = pbll.GetPostById(objectID);
                    name = (post == null) ? "" : post.T_HR_POSTDICTIONARY.POSTNAME;
                    break;

                case AssignObjectType.Employee:
                    EmployeeBLL ebll = new EmployeeBLL();
                    T_HR_EMPLOYEE employee = ebll.GetEmployeeByID(objectID);
                    name = (employee == null) ? "" : employee.EMPLOYEECNAME;
                    break;
            }

            return name;
        }


        #region   //建立时间2010-03-13 17:00  作者:喻建华
        /// <summary>
        /// 新增薪资方案分配
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        public string AddSalarySolutionAssign(T_HR_SALARYSOLUTIONASSIGN entTemp)
        {
            string strMsg = string.Empty;
            try
            {
                if (entTemp == null)
                {
                    return "{REQUIREDFIELDS}";
                }

                T_HR_SALARYSOLUTIONASSIGN ent = new T_HR_SALARYSOLUTIONASSIGN();
                bool isExist = false;
                var ents = from c in dal.GetObjects<T_HR_SALARYSOLUTIONASSIGN>()
                           where c.ASSIGNEDOBJECTID == entTemp.ASSIGNEDOBJECTID
                           select c;

                string strOldID = string.Empty;
                if (ents.Count() > 0)
                {
                    ent = ents.FirstOrDefault();
                    strOldID = string.Copy(ent.SALARYSOLUTIONASSIGNID);
                    isExist = true;
                }
                Utility.CloneEntity<T_HR_SALARYSOLUTIONASSIGN>(entTemp, ent);
                ent.T_HR_SALARYSOLUTIONReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSOLUTION", "SALARYSOLUTIONID", entTemp.T_HR_SALARYSOLUTION.SALARYSOLUTIONID);
                //ent.T_HR_SALARYSTANDARDReference.EntityKey =
                //    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSTANDARD", "SALARYSTANDARDID", entTemp.T_HR_SALARYSTANDARD.SALARYSTANDARDID);
                Utility.RefreshEntity(ent);
                if (isExist)
                {
                    ent.SALARYSOLUTIONASSIGNID = strOldID;
                    dal.Update(ent);

                }
                else
                {
                    dal.Add(ent);
                }

                strMsg = "{SAVESUCCESSED}";

            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message);
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }
        #endregion

        /// <summary>
        /// 引擎更新单据状态专用
        /// </summary>
        /// <param name="strEntityName"></param>
        /// <param name="EntityKeyName"></param>
        /// <param name="EntityKeyValue"></param>
        /// <param name="CheckState"></param>
        /// <returns></returns>
        public int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            try
            {
                int i = 0;
                string strMsg = string.Empty;
                var entity = (from c in dal.GetObjects()
                              where c.SALARYSOLUTIONASSIGNID == EntityKeyValue
                              select c).FirstOrDefault();
                if (entity != null)
                {
                    entity.CHECKSTATE = CheckState;
                    SalarySolutionAssignUpdate(entity);
                    i = 1;
                }
                return i;
            }
            catch (Exception e)
            {
                SMT.Foundation.Log.Tracer.Debug("FormID:" + EntityKeyValue + " UpdateCheckState:" + e.Message);
                return 0;
            }
        }
    }
}
