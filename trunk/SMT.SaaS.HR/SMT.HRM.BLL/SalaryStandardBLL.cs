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
    public class SalaryStandardBLL : BaseBll<T_HR_SALARYSTANDARD>, ILookupEntity
    {
        public void SalaryStandardAdd(T_HR_SALARYSTANDARD obj)
        {
            T_HR_SALARYSTANDARD ent = new T_HR_SALARYSTANDARD();
            Utility.CloneEntity<T_HR_SALARYSTANDARD>(obj, ent);
            if (obj.T_HR_SALARYLEVEL != null)
            {
                ent.T_HR_SALARYLEVELReference.EntityKey =
            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYLEVEL", "SALARYLEVELID", obj.T_HR_SALARYLEVEL.SALARYLEVELID);

            }
            dal.Add(ent);
        }
        /// <summary>
        /// 更新员工入职信息
        /// </summary>
        /// <param name="entity"></param>
        public void SalaryStandardUpdate(T_HR_SALARYSTANDARD obj)
        {

            var ent = from a in dal.GetTable()
                      where a.SALARYSTANDARDID == obj.SALARYSTANDARDID
                      select a;
            if (ent.Count() > 0)
            {
                T_HR_SALARYSTANDARD tmpEnt = ent.FirstOrDefault();

                Utility.CloneEntity<T_HR_SALARYSTANDARD>(obj, tmpEnt);

                if (obj.T_HR_SALARYLEVEL != null)
                {
                    tmpEnt.T_HR_SALARYLEVELReference.EntityKey =
                            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYLEVEL", "SALARYLEVELID", obj.T_HR_SALARYLEVEL.SALARYLEVELID);
                }
                else
                {
                    tmpEnt.T_HR_SALARYLEVELReference.EntityKey = null;
                }
                dal.Update(tmpEnt);
            }

        }
        public int SalaryStandardDelete(string[] IDs)
        {

            foreach (string id in IDs)
            {
                var ents = from e in dal.GetObjects<T_HR_SALARYSTANDARD>()
                           where e.SALARYSTANDARDID == id
                           select e;
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                if (ent != null)
                {
                    dal.DeleteFromContext(ent);
                    //DataContext.DeleteObject(ent);
                }

                //TODO:删除项目所包含的明细
            }

            return dal.SaveContextChanges();
        }
        /// <summary>
        ///根据方案ID删除
        /// </summary>
        /// <param name="IDs"></param>
        /// <returns></returns>
        public int SalaryStandardDeleteBySolutionID(string solutionID)
        {
            SalaryStandardItemBLL bll = new SalaryStandardItemBLL();
            var ents = from e in dal.GetObjects<T_HR_SALARYSTANDARD>()
                       where e.SALARYSOLUTIONID == solutionID
                       select e;
            if (ents == null)
            {
                return 0;
            }

            int iCount = ents.Count();
            if (iCount == 0)
            {
                return 0;
            }

            foreach (var ent in ents)
            {
                if (ent != null)
                {
                    bll.SalaryStandardItemsDeleteByStandID(ent.SALARYSTANDARDID);
                    dal.Delete(ent);
                    //DataContext.DeleteObject(ent);
                }
            }

            return iCount;
        }

        //public EntityObject[] GetLookupData(Dictionary<string, string> args)
        //{
        //    IQueryable<T_HR_SALARYSTANDARD> ents = from a in DataContext.T_HR_SALARYSTANDARD.Include("T_HR_SALARYSOLUTION")
        //                                     select a;
        //    return ents.Count() > 0 ? ents.ToArray() : null;
        //}

        public EntityObject[] GetLookupData(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_SALARYSTANDARD");

            IQueryable<T_HR_SALARYSTANDARD> ents = from a in dal.GetObjects<T_HR_SALARYSTANDARD>()
                                                   select a;
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_SALARYSTANDARD>(ents, pageIndex, pageSize, ref pageCount);
            return ents.Count() > 0 ? ents.ToArray() : null;
        }

        public T_HR_SALARYSTANDARD GetSalaryStandardByID(string ID)
        {
            var ents = from o in dal.GetObjects<T_HR_SALARYSTANDARD>().Include("T_HR_SALARYLEVEL")
                       where o.SALARYSTANDARDID == ID
                       select o;

            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }
        /// <summary>
        /// 获取方案ID 
        /// </summary>
        /// <param name="sType">0 公司 ， 1 部门 ，2 岗位</param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public string GetSolutionIDByIDType(int sType, string ID)
        {
            SalarySolutionAssignBLL bll = new SalarySolutionAssignBLL();
            string solutionID = bll.GetSolutionIDByAssignObjectID(ID);
            if (string.IsNullOrEmpty(solutionID))
            {
                if (sType == 2)
                {
                    PostBLL pbll = new PostBLL();
                    T_HR_POST post = pbll.GetPostById(ID);
                    solutionID = bll.GetSolutionIDByAssignObjectID(post.T_HR_DEPARTMENT.DEPARTMENTID);
                    if (string.IsNullOrEmpty(solutionID))
                    {
                        solutionID = bll.GetSolutionIDByAssignObjectID(post.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID);
                    }

                }

                else if (sType == 1)
                {
                    DepartmentBLL dbll = new DepartmentBLL();
                    T_HR_DEPARTMENT department = dbll.GetDepartmentById(ID);
                    solutionID = bll.GetSolutionIDByAssignObjectID(department.T_HR_COMPANY.COMPANYID);
                }
            }

            return solutionID;
        }

        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        public new IQueryable<T_HR_SALARYSTANDARD> QueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, int sType, string sValue, string CheckState)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            string solutionID = GetSolutionIDByIDType(sType, sValue);
            if (string.IsNullOrEmpty(solutionID))
            {
                return null;
            }
            else
            {
                if (!string.IsNullOrEmpty(filterString))
                {
                    filterString += " and ";
                }
                filterString += " c.SALARYSOLUTIONID== @" + queryParas.Count();
                queryParas.Add(solutionID);
            }
            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_SALARYSTANDARD");
            SetFilterWithflow("SALARYSTANDARDID", "T_HR_SALARYSTANDARD", userID, ref CheckState, ref filterString, ref queryParas);
            if (!string.IsNullOrEmpty(CheckState))
            {
                if (!string.IsNullOrEmpty(filterString))
                {
                    filterString += " and ";
                }
                filterString += " c.CHECKSTATE == @" + queryParas.Count();
                queryParas.Add(CheckState);
            }
            if (!string.IsNullOrEmpty(filterString))
            {
                //if (!string.IsNullOrEmpty(sType))
                //{
                //    filterString += " and ";
                //}
                filterString += " and ";
                switch (sType)
                {
                    case 0:
                        filterString += "COMPANYID==@" + queryParas.Count().ToString();
                        queryParas.Add(sValue);
                        break;
                    case 1:
                        filterString += "DEPARTMENTID==@" + queryParas.Count().ToString();
                        queryParas.Add(sValue);
                        break;
                    case 2:
                        filterString += "POSTID==@" + queryParas.Count().ToString();
                        queryParas.Add(sValue);
                        break;
                }
            }
            //var ents = from c in DataContext.T_HR_SALARYSTANDARD.Include("T_HR_SALARYLEVEL")
            //           join b in DataContext.T_HR_SALARYLEVEL.Include("T_HR_POSTLEVELDISTINCTION") on c.T_HR_SALARYLEVEL.SALARYLEVELID equals b.SALARYLEVELID
            //           join d in DataContext.T_HR_POSTLEVELDISTINCTION on b.T_HR_POSTLEVELDISTINCTION.POSTLEVELID equals d.POSTLEVELID
            //           join e in DataContext.T_HR_POSTDICTIONARY on d.POSTLEVEL equals e.POSTLEVEL
            //           join f in DataContext.T_HR_POST on new { e.POSTDICTIONARYID, b.SALARYLEVEL } equals new { f.T_HR_POSTDICTIONARY.POSTDICTIONARYID, f.SALARYLEVEL } into temptable
            //           from n in temptable
            //           join g in DataContext.T_HR_DEPARTMENT on n.T_HR_DEPARTMENT.DEPARTMENTID equals g.DEPARTMENTID
            //           join h in DataContext.T_HR_COMPANY on g.T_HR_COMPANY.COMPANYID equals h.COMPANYID
            //           select
            //           new
            //           {
            //               c,
            //               POSTID = n.POSTID,
            //               DEPARTMENTID = g.DEPARTMENTID,
            //               COMPANYID = h.COMPANYID,
            //               CREATEUSERID = c.CREATEUSERID,
            //               OWNERCOMPANYID = c.OWNERCOMPANYID,
            //               OWNERPOSTID = c.OWNERPOSTID,
            //               OWNERID = c.OWNERID,
            //               OWNERDEPARTMENT = c.OWNERDEPARTMENTID
            //           };
            var ents = from c in dal.GetObjects<T_HR_SALARYSTANDARD>().Include("T_HR_SALARYLEVEL")
                       join b in dal.GetObjects<T_HR_SALARYLEVEL>().Include("T_HR_POSTLEVELDISTINCTION") on c.T_HR_SALARYLEVEL.SALARYLEVELID equals b.SALARYLEVELID
                       join d in dal.GetObjects<T_HR_POSTLEVELDISTINCTION>() on b.T_HR_POSTLEVELDISTINCTION.POSTLEVELID equals d.POSTLEVELID
                       join f in dal.GetObjects<T_HR_POST>() on d.POSTLEVEL equals f.POSTLEVEL
                       join g in dal.GetObjects<T_HR_DEPARTMENT>() on f.T_HR_DEPARTMENT.DEPARTMENTID equals g.DEPARTMENTID
                       join h in dal.GetObjects<T_HR_COMPANY>() on g.T_HR_COMPANY.COMPANYID equals h.COMPANYID
                       select
                       new
                       {
                           c,
                           SALARYSTANDARDID = c.SALARYSTANDARDID,
                           POSTID = f.POSTID,
                           DEPARTMENTID = g.DEPARTMENTID,
                           SALARYSTANDARDNAME = c.SALARYSTANDARDNAME,
                           COMPANYID = h.COMPANYID,
                           CREATEUSERID = c.CREATEUSERID,
                           OWNERCOMPANYID = c.OWNERCOMPANYID,
                           OWNERPOSTID = c.OWNERPOSTID,
                           OWNERID = c.OWNERID,
                           OWNERDEPARTMENTID = c.OWNERDEPARTMENTID
                       };

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            IQueryable<T_HR_SALARYSTANDARD> ent = (from c in ents
                                                   select c.c).Distinct();
            //ent = ent.OrderBy(sort);
            ent = ent.OrderBy(x => x.PERSONALSIRATIO);


            ent = Utility.Pager<T_HR_SALARYSTANDARD>(ent, pageIndex, pageSize, ref pageCount);

            return ent;
        }

        /// <summary>
        /// 增加薪资标准和它的薪资项
        /// </summary>
        /// <param name="stand"></param>
        /// <param name="salaryItems"></param>
        /// <returns></returns>
        public string AddSalaryStanderAndItems(T_HR_SALARYSTANDARD stand, List<T_HR_SALARYSTANDARDITEM> salaryItems)
        {
            try
            {
                salaryItems.ForEach(item =>
                {

                    stand.T_HR_SALARYSTANDARDITEM.Add(item);
                    Utility.RefreshEntity(item);
                });
                dal.Add(stand);

                return "";
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message);
                dal.RollbackTransaction();
                return "Error";
            }
        }

        /// <summary>
        /// 批量生成薪资标准
        /// </summary>
        /// <param name="solution"></param>
        /// <param name="postLevels"></param>
        /// <param name="standModel"></param>
        /// <returns></returns>
        public string CreateSalaryStandBatch(T_HR_SALARYSOLUTION solution, Dictionary<string, string> postLevels, T_HR_SALARYSTANDARD standModel)
        {
            try
            {
                // 检查方案是否已经使用
                var tmpArchives = from ac in dal.GetObjects<T_HR_SALARYARCHIVE>()
                                  where ac.SALARYSOLUTIONID == solution.SALARYSOLUTIONID
                                  select ac;
                if (tmpArchives.Count() > 0)
                {
                    return "SALARYSOLUTIONINUSED";
                }

                // 薪资方案的体系ID
                string salarySystemID = solution.T_HR_SALARYSYSTEM.SALARYSYSTEMID;
                //薪资体系名
                string salarySystemName = solution.T_HR_SALARYSYSTEM.SALARYSYSTEMNAME;
                // 岗位级别名
                string postLevelName = "";
                EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL();
                SalaryStandardItemBLL itemBLL = new SalaryStandardItemBLL();

                //薪资方案所属的薪资体系
                List<T_HR_SALARYLEVEL> salarylevelList = new List<T_HR_SALARYLEVEL>();
                //薪资方案的薪资项集合
                List<V_SALARYITEM> salaryItems = new List<V_SALARYITEM>();

                salarylevelList = (from c in dal.GetObjects<T_HR_SALARYLEVEL>().Include("T_HR_POSTLEVELDISTINCTION")
                                   where c.T_HR_POSTLEVELDISTINCTION.T_HR_SALARYSYSTEM.SALARYSYSTEMID == salarySystemID
                                   select c).OrderBy(m => m.SALARYLEVEL).OrderBy(x => x.T_HR_POSTLEVELDISTINCTION.POSTLEVEL).ToList();


                //salaryItems = (from n in DataContext.T_HR_SALARYITEM
                //               join m in DataContext.T_HR_SALARYSOLUTIONITEM on n.SALARYITEMID equals m.T_HR_SALARYITEM.SALARYITEMID
                //               where m.T_HR_SALARYSOLUTION.SALARYSOLUTIONID == solution.SALARYSOLUTIONID
                //               select n).ToList();
                salaryItems = (from n in dal.GetObjects<T_HR_SALARYITEM>()
                               join m in dal.GetObjects<T_HR_SALARYSOLUTIONITEM>() on n.SALARYITEMID equals m.T_HR_SALARYITEM.SALARYITEMID
                               where m.T_HR_SALARYSOLUTION.SALARYSOLUTIONID == solution.SALARYSOLUTIONID
                               select new V_SALARYITEM
                              {
                                  T_HR_SALARYITEM = n,
                                  ORDERNUMBER = m.ORDERNUMBER
                              }
                               ).ToList();

                //方案所属地区差异补贴
                List<T_HR_AREAALLOWANCE> areaAllowance = new List<T_HR_AREAALLOWANCE>();
                areaAllowance = (from c in dal.GetObjects<T_HR_AREAALLOWANCE>()
                                 join b in dal.GetObjects<T_HR_SALARYSOLUTION>().Include("T_HR_AREADIFFERENCE") on c.T_HR_AREADIFFERENCE.AREADIFFERENCEID equals b.T_HR_AREADIFFERENCE.AREADIFFERENCEID
                                 where b.SALARYSOLUTIONID == solution.SALARYSOLUTIONID
                                 select c).ToList();

                //新增前删除原有的标准
                SalaryStandardBLL standBLL = new SalaryStandardBLL();
                standBLL.SalaryStandardDeleteBySolutionID(solution.SALARYSOLUTIONID);
                //var enttt = from ccc in salarylevelList where ccc.SALARYSUM>0 select ccc;
                int num = 0;
                #region   薪资标准排序算法
                int numsign = 0;
                int recordsign = 0;
                for (int x = 0; x < salarylevelList.Count; x++)
                {
                    if (numsign == 0) recordsign = x;
                    if ((x + 1 < salarylevelList.Count && salarylevelList[x].T_HR_POSTLEVELDISTINCTION.POSTLEVEL != salarylevelList[x + 1].T_HR_POSTLEVELDISTINCTION.POSTLEVEL) || x == salarylevelList.Count - 1)
                    {
                        numsign++;
                        for (int i = recordsign; i < recordsign + numsign; i++)
                        {
                            for (int j = i + 1; j < recordsign + numsign; j++)
                            {
                                //if (salarylevelList[i].T_HR_POSTLEVELDISTINCTION.POSTLEVEL > 0) break;
                                if (Convert.ToDecimal(salarylevelList[i].SALARYLEVEL) > Convert.ToDecimal(salarylevelList[j].SALARYLEVEL))
                                {
                                    T_HR_SALARYLEVEL t = null;
                                    t = salarylevelList[i];
                                    salarylevelList[i] = salarylevelList[j];
                                    salarylevelList[j] = t;
                                }
                            }
                        }
                        numsign = 0;
                    }
                    else
                        numsign++;

                }
                #endregion
                //dal.BeginTransaction();
                foreach (var sl in salarylevelList)
                {
                    // 薪资体系的每条记录都对应一个标准 根据sl生成薪资标准
                    T_HR_SALARYSTANDARD stand = new T_HR_SALARYSTANDARD();
                    //薪资标准的薪资项集合
                    List<T_HR_SALARYSTANDARDITEM> standSalaryitems = new List<T_HR_SALARYSTANDARDITEM>();
                    //获取岗位级别名
                    var ent = from c in postLevels
                              join b in salarylevelList on c.Value equals b.T_HR_POSTLEVELDISTINCTION.POSTLEVEL.ToString()
                              where b.SALARYLEVELID == sl.SALARYLEVELID
                              select c.Key;
                    if (ent.Count() > 0)
                    {
                        postLevelName = ent.FirstOrDefault().ToString();
                    }

                    //薪资标准名= 薪资体系名+岗位级别名+"-"+薪资级别名
                    stand.SALARYSTANDARDNAME = salarySystemName + postLevelName + "-" + sl.SALARYLEVEL.ToString();
                    stand.SALARYSTANDARDID = Guid.NewGuid().ToString();
                    // stand.T_HR_SALARYLEVEL = new T_HR_SALARYLEVEL();
                    stand.T_HR_SALARYLEVELReference.EntityKey
                          = new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYLEVEL", "SALARYLEVELID", sl.SALARYLEVELID);
                    //  stand.T_HR_SALARYLEVEL.SALARYLEVELID = sl.SALARYLEVELID;
                    stand.SALARYSOLUTIONID = solution.SALARYSOLUTIONID;
                    stand.CHECKSTATE = Convert.ToInt32(CheckStates.Approved).ToString();
                    stand.BASESALARY = sl.SALARYSUM;
                    stand.CREATEDATE = System.DateTime.Now;

                    //有关权限的字段
                    stand.CREATECOMPANYID = standModel.CREATECOMPANYID;
                    stand.CREATEDEPARTMENTID = standModel.CREATEDEPARTMENTID;
                    stand.CREATEPOSTID = standModel.CREATEPOSTID;
                    stand.CREATEUSERID = standModel.CREATEUSERID;
                    stand.OWNERCOMPANYID = standModel.OWNERCOMPANYID;
                    stand.OWNERDEPARTMENTID = standModel.OWNERDEPARTMENTID;
                    stand.OWNERID = standModel.OWNERID;
                    stand.OWNERPOSTID = standModel.OWNERPOSTID;


                    //增加排序号
                    stand.PERSONALSIRATIO = num;
                    num++;


                    //标准对应的地区差异补贴（和岗位级别有关）
                    decimal? allowance = 0;
                    if (areaAllowance != null)
                    {
                        allowance = (from al in areaAllowance
                                     where al.POSTLEVEL == sl.T_HR_POSTLEVELDISTINCTION.POSTLEVEL.ToString()
                                     select al.ALLOWANCE).FirstOrDefault();
                    }
                    //计算标准的基础数据
                    decimal? BasicData = sl.SALARYSUM;

                    //按照方案的薪资项集合生成薪资标准的薪资项
                    foreach (var item in salaryItems)
                    {
                        T_HR_SALARYSTANDARDITEM standItem = new T_HR_SALARYSTANDARDITEM();
                        standItem.STANDRECORDITEMID = Guid.NewGuid().ToString();
                        standItem.T_HR_SALARYITEMReference.EntityKey
                            = new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYITEM", "SALARYITEMID", item.T_HR_SALARYITEM.SALARYITEMID);
                        // standItem.T_HR_SALARYITEM.SALARYITEMID = item.T_HR_SALARYITEM.SALARYITEMID;
                        standItem.CREATEUSERID = solution.CREATEUSERID;
                        standItem.T_HR_SALARYSTANDARDReference.EntityKey
                            = new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSTANDARD", "SALARYSTANDARDID", stand.SALARYSTANDARDID);
                        // standItem.T_HR_SALARYSTANDARD.SALARYSTANDARDID = stand.SALARYSTANDARDID;
                        standItem.CREATEDATE = System.DateTime.Now;
                        standItem.ORDERNUMBER = item.ORDERNUMBER;
                        //计算类型是手动输入的 金额是薪资项设置是输入的值
                        if (item.T_HR_SALARYITEM.CALCULATORTYPE == "1" || (item.T_HR_SALARYITEM.CALCULATORTYPE == "4" && item.T_HR_SALARYITEM.GUERDONSUM != null))
                        {
                            standItem.SUM = item.T_HR_SALARYITEM.GUERDONSUM.ToString();
                        }
                        //计算类型是公式计算 而且不是在生成薪资时计算  
                        else if (item.T_HR_SALARYITEM.CALCULATORTYPE == "3" && item.T_HR_SALARYITEM.ISAUTOGENERATE == "0")
                        {
                            standItem.SUM = bll.AutoCalculateItem(item.T_HR_SALARYITEM.SALARYITEMID, Convert.ToDecimal(BasicData), allowance.ToString()).ToString();
                        }
                        //地区差异补贴
                        else if (item.T_HR_SALARYITEM.ENTITYCOLUMNCODE == "AREADIFALLOWANCE")
                        {
                            standItem.SUM = allowance.ToString();
                        }
                        standSalaryitems.Add(standItem);
                    }
                    //SalaryStandardAdd(stand);
                    //itemBLL.SalaryStandardItemsAdd(standSalaryitems);
                    AddSalaryStanderAndItems(stand, standSalaryitems);
                }
                //dal.CommitTransaction();
                return "SAVESUCCESSED";
            }
            catch (Exception ex)
            {
                //dal.RollbackTransaction();
                SMT.Foundation.Log.Tracer.Debug(ex.Message);
                return ex.Message;
            }
        }
    }
}
