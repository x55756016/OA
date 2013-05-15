using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using SMT.HRM.CustomModel;

namespace SMT.HRM.BLL
{
    public class SalaryItemSetBLL : BaseBll<T_HR_SALARYITEM>, ILookupEntity
    {
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
        public IQueryable<T_HR_SALARYITEM> GetSalaryItemSetPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string strCheckState, string userid)
        {
            List<object> queryParas = new List<object>();

            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userid, "T_HR_SALARYITEM");


            IQueryable<T_HR_SALARYITEM> ents = dal.GetObjects<T_HR_SALARYITEM>();

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
                //ents = ents.Where(filterString, paras.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_SALARYITEM>(ents, pageIndex, pageSize, ref pageCount);



            return ents;
        }
        public List<T_HR_SALARYITEM> GetSalaryItemSetByFilter(string filterString, IList<object> paras, string userid)
        {
            List<object> queryParas = new List<object>();

            queryParas.AddRange(paras);



            SetOrganizationFilter(ref filterString, ref queryParas, userid, "T_HR_SALARYITEM");

            IQueryable<T_HR_SALARYITEM> ents = dal.GetObjects<T_HR_SALARYITEM>();

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
                //ents = ents.Where(filterString, paras.ToArray());
            }
            return ents.ToList();
        }
        /// <summary>
        /// 根据薪资项设置ID查询实体
        /// </summary>
        /// <param name="SalaryItemSetID">薪资项设置ID</param>
        /// <returns>返回薪资项设置实体</returns>
        public T_HR_SALARYITEM GetSalaryItemSetByID(string SalaryItemSetID)
        {
            var ents = from a in dal.GetTable()
                       where a.SALARYITEMID == SalaryItemSetID
                       select a;
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }
        /// <summary>
        /// 根据薪资项设置ID查询实体
        /// </summary>
        /// <param name="SalaryItemSetID">薪资项设置ID</param>
        /// <returns>返回薪资项设置实体</returns>
        public T_HR_SALARYITEM GetSalaryItemSetByStandardID(string StandardID)
        {
            var ents = from a in dal.GetObjects<T_HR_SALARYITEM>()
                       join b in dal.GetObjects<T_HR_SALARYSTANDARDITEM>() on a.SALARYITEMID equals b.T_HR_SALARYITEM.SALARYITEMID
                       where b.T_HR_SALARYSTANDARD.SALARYSTANDARDID == StandardID
                       select a;
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }
        /// <summary>
        /// 根据薪资项类型查询
        /// </summary>
        /// <param name="SalaryItemType">薪资项类型名称</param>
        /// <returns>返回薪资项检索结果</returns>
        public List<T_HR_SALARYITEM> GetSalaryItemSets(string SalaryItemType)
        {
            var ents = from a in dal.GetTable()
                       where a.SALARYITEMTYPE == SalaryItemType
                       select a;
            return ents.Count() > 0 ? ents.ToList() : null;
        }

        /// <summary>
        /// 获取所有薪资项
        /// </summary>
        /// <returns>返回所有薪资项结果</returns>
        public List<T_HR_SALARYITEM> GetSalaryItemSets()
        {
            var ents = dal.GetObjects<T_HR_SALARYITEM>();
            return ents.Count() > 0 ? ents.ToList() : null;
        }

        /// <summary>
        /// 根据薪资项名称查询
        /// </summary>
        /// <param name="SalaryItemSetName">薪资项名</param>
        /// <returns>返回bool类型</returns>
        public bool GetSalaryItemSetName(string SalaryItemSetName)
        {
            var ents = from a in dal.GetTable()
                       where a.SALARYITEMNAME == SalaryItemSetName
                       select a;
            return ents.Count() > 0 ? true : false;
        }

        //public EntityObject[] GetLookupData(Dictionary<string, string> args)
        //{
        //    IQueryable<T_HR_SALARYITEM> ents = from a in DataContext.T_HR_SALARYITEM
        //                                           select a;
        //    return ents.Count() > 0 ? ents.ToArray() : null;
        //}

        public EntityObject[] GetLookupData(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            IQueryable<T_HR_SALARYITEM> ents = from a in dal.GetObjects<T_HR_SALARYITEM>()
                                               select a;
            ents = ents.Where(filterString, paras.ToArray());
            return ents.Count() > 0 ? ents.ToArray() : null;
        }

        /// <summary>
        /// 更新薪资项设置
        /// </summary>
        /// <param name="entity">薪资项设置实体</param>
        public void SalaryItemSetUpdate(T_HR_SALARYITEM entity)
        {
            try
            {
                var ents = from a in dal.GetTable()
                           where a.SALARYITEMID == entity.SALARYITEMID
                           select a;
                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();
                    Utility.CloneEntity<T_HR_SALARYITEM>(entity, ent);
                    dal.Update(ent);
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// 按薪资项模版自动生成薪资项
        /// </summary>
        /// <param name="entitys">薪资项设置实体集</param>
        public bool FormulaTemplateAdd(T_HR_SALARYITEM[] entitys)
        {
            string companyid = entitys[0].CREATECOMPANYID;
            var ents = from a in dal.GetObjects<T_HR_SALARYITEM>()
                       where a.CREATECOMPANYID == companyid
                       select a;
            if (ents.Count() > 0)
            {
                foreach (var ent in ents)
                {
                    dal.DeleteFromContext(ent);
                    // DataContext.DeleteObject(ent);  
                }
                dal.SaveContextChanges();
                //DataContext.SaveChanges();
            }
            foreach (T_HR_SALARYITEM salaryitem in entitys)
            {
                dal.AddToContext(salaryitem);
                //DataContext.AddObject("T_HR_SALARYITEM", salaryitem);
            }
            //if (DataContext.SaveChanges() > 0) return true; 
            if (dal.SaveContextChanges() > 0) return true;
            else return false;
        }

        /// <summary>
        /// 检查计算项的值
        /// </summary>
        /// <param name="itemid">薪资项的ID</param>
        /// <returns>返回计算项的值检查结果</returns>
        public decimal CheckCalItem(string itemid, string stitemid, ref bool ret)
        {
            decimal temp = 0;
            var ents = from a in dal.GetObjects<T_HR_SALARYITEM>()
                       where a.SALARYITEMID == itemid
                       select a;
            if (ents.Count() > 0)
            {
                int i = 0;
                if (ents.FirstOrDefault().CALCULATORTYPE == "4" || ents.FirstOrDefault().CALCULATORTYPE == "2")
                {
                    ret = true;
                    return 0;
                }
                if (string.IsNullOrEmpty(ents.FirstOrDefault().CALCULATEFORMULACODE))
                {
                    if (!string.IsNullOrEmpty(ents.FirstOrDefault().GUERDONSUM.ToString()))
                    {
                        return 0;
                    }
                    else
                    {
                        ret = true;
                        return 0;
                    }
                }
                string[] ent = ents.FirstOrDefault().CALCULATEFORMULACODE.Split(',');
                foreach (string e in ent)
                {
                    if (ret) return 0;
                    if (!CalPartValue(e) && !string.IsNullOrEmpty(e))
                    {
                        if (e != stitemid)
                        {
                            if (e.Length > (31 + 1))    //1主要是为了过滤标准薪资
                            {
                                temp = CheckCalItem(e, stitemid, ref ret);
                            }
                        }
                        else
                        {
                            ret = true;
                            return 0;
                        }
                    }
                    i++;
                }
                return temp;
            }
            return 0;
        }

        public bool CalPartValue(string symbol)
        {
            bool result = false;
            switch (symbol)
            {
                case "+":
                case "-":
                case "X":
                case "/":
                    result = true;
                    break;
                case "":
                    break;
                //case "(":
                //    break;
                //case ")":
                //    break;
            }
            return result;
        }


        /// <summary>
        /// 删除薪资设置记录，可同时删除多行记录
        /// </summary>
        /// <param name="SalaryItemSetIDs">薪资设置ID数组</param>
        /// <returns></returns>
        public int SalaryItemSetDelete(string[] SalaryItemSetIDs)
        {
            int flag = 0;
            foreach (string id in SalaryItemSetIDs)
            {
                var ents = from e in dal.GetObjects<T_HR_SALARYITEM>()
                           where e.SALARYITEMID == id
                           select e;
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;


                var temp = from e in dal.GetObjects<T_HR_SALARYSOLUTIONITEM>()
                           where e.T_HR_SALARYITEM.SALARYITEMID == ent.SALARYITEMID
                           select e;
                if (temp.Count() > 0)
                {
                    flag = 0;

                }
                else
                {
                    dal.DeleteFromContext(ent);
                    flag = dal.SaveContextChanges();
                }
                //if (ent != null)
                //{
                //    dal.DeleteFromContext(ent);
                //   // DataContext.DeleteObject(ent);
                //}


            }

            return flag;
            //DataContext.SaveChanges();
        }

        //xiedx
        //2012-8-23
        //SalaryItem.xml文件读取sql语句并执行
        public void ExecuteSalaryItemSql(T_HR_SALARYITEM salaryItem)
        {
            try
            {
                List<string> Items = new List<string>();
                string xmlPath = System.Web.HttpContext.Current.Server.MapPath("SalaryItem.xml");
                Dictionary<string, string> dicUpdate = new Dictionary<string, string>();
                string strCompanyID = string.Copy(salaryItem.OWNERCOMPANYID);

                XmlDocument xmlSalaryItem = new XmlDocument();
                xmlSalaryItem.Load(xmlPath);

                //XmlNodeList xmlNodeList = xmlSalaryItem.SelectSingleNode("Item").ChildNodes;
                XmlNodeList xmlNodeList = xmlSalaryItem.GetElementsByTagName("Items");

                foreach (XmlNode Itemnode in xmlNodeList)
                {
                    string strSalaryItemName = Itemnode.Attributes["SALARYITEMNAME"].Value;
                    string strInsertSql = Itemnode.InnerText;
                    if (string.IsNullOrWhiteSpace(strSalaryItemName) || string.IsNullOrWhiteSpace(strInsertSql))
                    {
                        Utility.SaveLog("执行初始化薪资函数ExecuteSalaryItemSql失败，请检查服务层的SalaryItem.xml是否某个节点的SALARYITEMNAME或者节点内的插入语句为空,执行此次初始化薪资项目的公司ID为："
                            + salaryItem.OWNERCOMPANYID + "，执行人为：" + salaryItem.OWNERID);
                        break;
                    }

                    var ent = (from s in dal.GetObjects()
                               where s.SALARYITEMNAME == strSalaryItemName && s.OWNERCOMPANYID == salaryItem.OWNERCOMPANYID
                               select s).FirstOrDefault();

                    salaryItem.SALARYITEMID = Guid.NewGuid().ToString();
                    strInsertSql = strInsertSql.Replace("@SALARYITEMID", salaryItem.SALARYITEMID);
                    strInsertSql = strInsertSql.Replace("@OWNERID", salaryItem.OWNERID);
                    strInsertSql = strInsertSql.Replace("@OWNERPOSTID", salaryItem.OWNERPOSTID);
                    strInsertSql = strInsertSql.Replace("@OWNERDEPARTMENTID", salaryItem.OWNERDEPARTMENTID);
                    strInsertSql = strInsertSql.Replace("@OWNERCOMPANYID", salaryItem.OWNERCOMPANYID);
                    strInsertSql = strInsertSql.Replace("@UPDATEUSERID", salaryItem.UPDATEUSERID);
                    string strDateTime = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                    string strRemarkDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    strInsertSql = strInsertSql.Replace("@UPDATEDATE", strDateTime);
                    strInsertSql = strInsertSql.Replace("初始化", "初始化" + strRemarkDateTime);
                    strInsertSql = strInsertSql.Replace("@CREATECOMPANYID", salaryItem.OWNERCOMPANYID);
                    //读取的xml文件里面的SQL语句然后执行记得千万不要有“;”,会出错...
                    dal.ExecuteCustomerSql(strInsertSql);   //插入记录

                    string strID = string.Copy(salaryItem.SALARYITEMID);

                    if (ent != null)
                    {
                        strID = ent.SALARYITEMID;

                        var entCur = (from s in dal.GetObjects()
                                      where s.SALARYITEMID == salaryItem.SALARYITEMID
                                      select s).FirstOrDefault();

                        ent.CALCULATEFORMULA = entCur.CALCULATEFORMULA;
                        ent.CALCULATEFORMULACODE = entCur.CALCULATEFORMULACODE;
                        ent.UPDATEDATE = DateTime.Now;
                        ent.UPDATEUSERID = entCur.UPDATEUSERID;
                        ent.REMARK = strRemarkDateTime + "初始化变更";

                        dal.Update(ent);

                        dal.Delete(entCur);
                    }

                    if (!dicUpdate.Keys.Contains(strSalaryItemName))
                    {
                        dicUpdate.Add(strSalaryItemName, strID);
                    }
                    //string strTempSql = string.Copy(strUpdateSql);
                    //strTempSql = string.Format(strTempSql, strSalaryItemName, strID, salaryItem.OWNERCOMPANYID);
                    //strTempSql = strTempSql.Replace("*[", "{");
                    //strTempSql = strTempSql.Replace("]*", "}");
                    //dal.ExecuteCustomerSql(strTempSql); //更新记录中计算公式内使用薪资项目的ID;
                }

                if (dicUpdate.Count() > 0)
                {
                    foreach (KeyValuePair<string, string> strItem in dicUpdate)
                    {
                        string strText = strItem.Key;
                        string strValue = strItem.Value;

                        var ents = from s in dal.GetObjects()
                                   where s.OWNERCOMPANYID == strCompanyID && strText.Contains(s.CALCULATEFORMULACODE)
                                   select s;

                        if (ents == null)
                        {
                            return;
                        }

                        foreach (T_HR_SALARYITEM item in ents)
                        {
                            item.CALCULATEFORMULACODE = item.CALCULATEFORMULACODE.Replace("[" + strText + "]", "{" + strValue + "}");
                            dal.Update(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " ExecuteSalaryItemSql:" + ex.Message);
                throw ex;
            }

        }


    }
}
