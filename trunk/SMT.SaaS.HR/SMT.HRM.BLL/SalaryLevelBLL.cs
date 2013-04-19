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

namespace SMT.HRM.BLL
{
    public class SalaryLevelBLL : BaseBll<T_HR_SALARYLEVEL>, ILookupEntity
    {

        public List<T_HR_SALARYLEVEL> GetAllSalaryLevel()
        {
            var ents = dal.GetObjects().Include("T_HR_POSTLEVELDISTINCTION");

            return ents.ToList();
        }
        public List<T_HR_SALARYLEVEL> GetSalaryLevelBySystemID(string systemID)
        {
            var ents = from c in dal.GetObjects().Include("T_HR_POSTLEVELDISTINCTION")
                       //join b in dal.GetObjects<T_HR_POSTLEVELDISTINCTION>() on c.T_HR_POSTLEVELDISTINCTION.POSTLEVELID equals b.POSTLEVELID
                       //where b.T_HR_SALARYSYSTEM.SALARYSYSTEMID == systemID
                       where c.T_HR_POSTLEVELDISTINCTION.T_HR_SALARYSYSTEM.SALARYSYSTEMID == systemID
                       select c;
            //if (ents.Count() > 0)
            //{
            //    foreach (var ent in ents)
            //    {
            //        ent.T_HR_POSTLEVELDISTINCTIONReference.Load();
            //    }
            //}
            return ents.Count()>0?ents.ToList():null;
        }

        public IQueryable<T_HR_SALARYLEVEL> GetSalaryLevelPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_SALARYLEVEL");

            IQueryable<T_HR_SALARYLEVEL> ents = dal.GetObjects().Include("T_HR_POSTLEVELDISTINCTION");
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_SALARYLEVEL>(ents, pageIndex, pageSize, ref pageCount);
            T_HR_SALARYLEVEL level = new T_HR_SALARYLEVEL();

            return ents;

        }
        public void GenerateSalaryLevel(int lowSalaryLevel, string userid)
        {
            if (lowSalaryLevel <= 0)
                return;

            PostLevelDistinctionBLL plBll = new PostLevelDistinctionBLL();

            List<T_HR_POSTLEVELDISTINCTION> postlevels = plBll.GetAllPostLevelDistinction();

            foreach (var pl in postlevels)
            {
                int j = 0;

                for (int i = lowSalaryLevel; i > 0; i--)
                {
                    string tmplevel = i.ToString();

                    var tmpEnts = from s in dal.GetTable()
                                  where s.SALARYLEVEL == tmplevel
                                  && s.T_HR_POSTLEVELDISTINCTION.POSTLEVEL == pl.POSTLEVEL
                                  select s;
                    if (tmpEnts != null && tmpEnts.Count() > 0)
                    {
                        var tmpSl = tmpEnts.FirstOrDefault();

                        tmpSl.SALARYSUM = pl.BASICSALARY + pl.LEVELBALANCE * j;
                        tmpSl.UPDATEDATE = System.DateTime.Now;

                        dal.Update(tmpSl);
                    }
                    else
                    {
                        T_HR_SALARYLEVEL tmpSL = new T_HR_SALARYLEVEL();

                        tmpSL.SALARYLEVELID = Guid.NewGuid().ToString();
                        tmpSL.SALARYLEVEL = i.ToString();
                        tmpSL.SALARYSUM = pl.BASICSALARY + (pl.LEVELBALANCE * j);

                        tmpSL.CREATEDATE = System.DateTime.Now;
                        tmpSL.UPDATEDATE = System.DateTime.Now;

                        tmpSL.T_HR_POSTLEVELDISTINCTIONReference.EntityKey =
            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_POSTLEVELDISTINCTION", "POSTLEVELID", pl.POSTLEVELID);

                        dal.Add(tmpSL);

                    }

                    j++;
                }
            }
        }
        public void GenerateSalaryLevel(int lowSalaryLevel, int highSalaryLevel, string systemID, string userid)
        {
            //if (lowSalaryLevel <= 0)
            //    return;
            var ents = from e in dal.GetObjects().Include("T_HR_POSTLEVELDISTINCTION")
                       join c in dal.GetObjects<T_HR_POSTLEVELDISTINCTION>() on e.T_HR_POSTLEVELDISTINCTION.POSTLEVELID equals c.POSTLEVELID
                       where c.T_HR_SALARYSYSTEM.SALARYSYSTEMID == systemID
                       select e;
           
            if (ents.Count()>0)
            {
                foreach (var ent in ents)
                {
                    dal.DeleteFromContext(ent);
                }
            }
            dal.SaveContextChanges();

          


            PostLevelDistinctionBLL plBll = new PostLevelDistinctionBLL();

            //  List<T_HR_POSTLEVELDISTINCTION> postlevels = plBll.GetAllPostLevelDistinction();
            List<T_HR_POSTLEVELDISTINCTION> postlevels = plBll.GetPostLevelDistinctionBySystemID(systemID);

            foreach (var pl in postlevels)
            {
                int j = 0;

                for (int i = lowSalaryLevel; i >= highSalaryLevel; i--)
                {
                    string tmplevel = i.ToString();

                    var tmpEnts = from s in dal.GetTable()
                                  where s.SALARYLEVEL == tmplevel
                                  && s.T_HR_POSTLEVELDISTINCTION.POSTLEVELID == pl.POSTLEVELID
                                  select s;
                    if (tmpEnts != null && tmpEnts.Count() > 0)
                    {
                        var tmpSl = tmpEnts.FirstOrDefault();

                        tmpSl.SALARYSUM = pl.BASICSALARY + pl.LEVELBALANCE * j;
                        tmpSl.UPDATEDATE = System.DateTime.Now;
                        tmpSl.UPDATEUSERID = userid;
                        dal.Update(tmpSl);
                    }
                    else
                    {
                        T_HR_SALARYLEVEL tmpSL = new T_HR_SALARYLEVEL();

                        tmpSL.SALARYLEVELID = Guid.NewGuid().ToString();
                        tmpSL.SALARYLEVEL = i.ToString();
                        tmpSL.SALARYSUM = pl.BASICSALARY + (pl.LEVELBALANCE * j);

                        tmpSL.CREATEDATE = System.DateTime.Now;
                        tmpSL.UPDATEDATE = System.DateTime.Now;
                        tmpSL.CREATEUSERID = userid;
                        tmpSL.T_HR_POSTLEVELDISTINCTIONReference.EntityKey =
            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_POSTLEVELDISTINCTION", "POSTLEVELID", pl.POSTLEVELID);

                        dal.Add(tmpSL);

                    }

                    j++;
                }
            }
        }
        public EntityObject[] GetLookupData(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_SALARYLEVEL");

            IQueryable<T_HR_SALARYLEVEL> ents = dal.GetObjects().Include("T_HR_POSTLEVELDISTINCTION");



            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(" T_HR_POSTLEVELDISTINCTION.POSTLEVEL,SALARYLEVEL");

            ents = Utility.Pager<T_HR_SALARYLEVEL>(ents, pageIndex, pageSize, ref pageCount);
            return ents.Count() > 0 ? ents.ToArray() : null;
        }

        public void SalaryLevelUpdate(T_HR_SALARYLEVEL obj)
        {
            var ent = from a in dal.GetTable()
                      where a.SALARYLEVELID == obj.SALARYLEVELID
                      select a;
            if (ent.Count() > 0)
            {
                T_HR_SALARYLEVEL tmpEnt = ent.FirstOrDefault();

                Utility.CloneEntity<T_HR_SALARYLEVEL>(obj, tmpEnt);

                if (obj.T_HR_POSTLEVELDISTINCTION != null)
                {
                    tmpEnt.T_HR_POSTLEVELDISTINCTIONReference.EntityKey =
                            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_POSTLEVELDISTINCTION", "POSTLEVELID", obj.T_HR_POSTLEVELDISTINCTION.POSTLEVELID);
                }
                else
                {
                    tmpEnt.T_HR_POSTLEVELDISTINCTION.EntityKey = null;
                }
                dal.Update(tmpEnt);
            }
        }
        public void SalaryLevelADD(T_HR_SALARYLEVEL obj)
        {
            PostLevelDistinctionBLL plBll = new PostLevelDistinctionBLL();
            List<T_HR_POSTLEVELDISTINCTION> postlevels = plBll.GetAllPostLevelDistinction();

            T_HR_POSTLEVELDISTINCTION tmpEnts = (from s in postlevels
                                                 where s.POSTLEVEL == obj.T_HR_POSTLEVELDISTINCTION.POSTLEVEL
                                                 select s).First();
            var ent = from a in dal.GetTable()
                      where a.SALARYLEVEL == obj.SALARYLEVEL && a.T_HR_POSTLEVELDISTINCTION.POSTLEVELID == tmpEnts.POSTLEVELID
                      select a;
            //obj.T_HR_POSTLEVELDISTINCTION = new T_HR_POSTLEVELDISTINCTION();
            //obj.T_HR_POSTLEVELDISTINCTION = tmpEnts;
            if (ent.Count() <= 0)
            {
                if (tmpEnts != null)
                {
                    obj.T_HR_POSTLEVELDISTINCTIONReference.EntityKey =
        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_POSTLEVELDISTINCTION", "POSTLEVELID", tmpEnts.POSTLEVELID);
                }

                dal.Add(obj);

            }
        }
        public T_HR_SALARYLEVEL GetSalaryLevelByID(string ID)
        {
            var ents = from o in dal.GetObjects().Include("T_HR_POSTLEVELDISTINCTION")
                       where o.SALARYLEVELID == ID
                       select o;

            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }
    }
}
