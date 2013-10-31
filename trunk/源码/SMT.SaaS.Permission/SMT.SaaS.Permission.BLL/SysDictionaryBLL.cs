/*
 * 文件名：SysDictionaryBLL.cs
 * 作  用：
 * 创建人：
 * 创建时间：2010-2-26 14:19:12
 * 修改人：向寒咏
 * 修改说明：增加缓存
 * 修改时间：2010-7-7 14:19:12
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.Permission.DAL;
using SMT_System_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using SMT.Foundation.Log;
using System.Data;

namespace SMT.SaaS.Permission.BLL
{
    public class SysDictionaryBLL : BaseBll<T_SYS_DICTIONARY>, ILookupEntity
    {
        private List<T_SYS_DICTIONARY> listTemp;

        public List<T_SYS_DICTIONARY> ListTemp
        {
            get
            {

                List<T_SYS_DICTIONARY> lsdic;
                if (CacheManager.GetCache("T_SYS_DICTIONARY") != null)
                {
                    lsdic = (List<T_SYS_DICTIONARY>)CacheManager.GetCache("T_SYS_DICTIONARY");
                }
                else
                {

                    var ents = from a in dal.GetObjects<T_SYS_DICTIONARY>().Include("T_SYS_DICTIONARY2")
                               select a;

                    lsdic = ents.ToList();
                    CacheManager.AddCache("T_SYS_DICTIONARY", lsdic);
                }

                return lsdic.Count() > 0 ? lsdic : null;
            }


            set { listTemp = value; }
        }
        /// <summary>
        /// 根据字典类型获取所有的字典值
        /// </summary>
        /// <param name="sysType">字典类型,为空时获取所有类型的字典值</param>
        /// <returns>字典值信息列表</returns>
        //public IQueryable<T_SYS_DICTIONARY> GetSysDictionaryByCategory(string category)
        //{
        //    try
        //    {
        //        //List<T_SYS_DICTIONARY> all = new List<T_SYS_DICTIONARY>();
        //        var ents = from a in ListTemp
        //                   where string.IsNullOrEmpty(category) || a.DICTIONCATEGORY == category
        //                   orderby a.DICTIONARYVALUE
        //                   select a;
        //        //if (ents != null)
        //        //{
        //        //    if (ents.Count() > 0)
        //        //    {
        //        //        ents.ToList().ForEach(item =>
        //        //        {

        //        //            item.DICTIONCATEGORYNAME = "";
        //        //            item.REMARK = "";
        //        //            item.UPDATEDATE = null;
        //        //            item.UPDATEUSER = "";
        //        //            item.CREATEDATE = null;
        //        //            item.CREATEUSER = "";
        //        //            item.SYSTEMNAME = "";
        //        //            //item.SYSTEMCODE = "";
        //        //            item.SYSTEMNEED = "";

        //        //            all.Add(item);

        //        //        });
        //        //    }
        //        //}

        //        //return all.Count() > 0 ? all.AsQueryable() : null;
        //        return ents.AsQueryable();
        //    }
        //    catch (Exception ex)
        //    {
        //        Tracer.Debug("权限系统SysDictionaryBLL-GetSysDictionaryByCategory" + System.DateTime.Now.ToString() + " " + ex.ToString());
        //        return null;
        //    }

        //}

        /// <summary>
        /// 根据字典类型获取所有的字典值
        /// </summary>
        /// <param name="sysType">字典类型,为空时获取所有类型的字典值</param>
        /// <returns>字典值信息列表</returns>
        public IQueryable<T_SYS_DICTIONARY> GetSysDictionaryByCategory(string category)
        {
            try
            {
                //List<T_SYS_DICTIONARY> all = new List<T_SYS_DICTIONARY>();
                if (!string.IsNullOrEmpty(category))
                {
                    var ents = from a in ListTemp
                               where a.DICTIONCATEGORY == category
                               orderby a.DICTIONARYVALUE
                               select a;
                    //where string.IsNullOrEmpty(category) || a.DICTIONCATEGORY == category
                    return ents.AsQueryable();
                }
                else
                {
                    Tracer.Debug("权限系统SysDictionaryBLL-GetSysDictionaryByCategory:种类为空");
                    return null;
                }

                
            }
            catch (Exception ex)
            {
                Tracer.Debug("权限系统SysDictionaryBLL-GetSysDictionaryByCategory" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// 所有的字典值
        /// </summary>
        /// <param name="sysType">所有类型的字典值</param>
        /// <returns>字典值信息列表</returns>
        public IQueryable<T_SYS_DICTIONARY> GetAllSysDictionary()
        {
            try
            {
                //List<T_SYS_DICTIONARY> all = new List<T_SYS_DICTIONARY>();
                var ents = from a in ListTemp
                           orderby a.DICTIONARYVALUE
                           select a;

                return ents.AsQueryable();
            }
            catch (Exception ex)
            {
                Tracer.Debug("权限系统SysDictionaryBLL-GetAllSysDictionary" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// 根据字典类型获取所有的字典值
        /// </summary>
        /// <param name="sysType">字典类型,为空时获取所有类型的字典值</param>
        /// <returns>字典值信息列表</returns>
        public string GetSysDictionaryByCategoryAndValue(string category, string StrValue)
        {
            try
            {
                string StrReturn = "";//返回字符串
                //T_SYS_DICTIONARY dict = new T_SYS_DICTIONARY();
                var ents = from a in ListTemp
                           where (string.IsNullOrEmpty(category) || a.DICTIONCATEGORY == category) && a.DICTIONARYVALUE == System.Convert.ToInt16(StrValue)

                           select a;
                if (ents != null)
                {
                    if (ents.Count() > 0)
                    {
                        StrReturn = ents.FirstOrDefault().DICTIONARYNAME;
                    }
                }

                return StrReturn;

            }
            catch (Exception ex)
            {
                Tracer.Debug("权限系统SysDictionaryBLL-GetSysDictionaryByCategoryAndValue" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return "";
            }

        }


        /// <summary>
        /// 根据字典类型获取所有的字典值,根据时间取值 2011-5-6
        /// </summary>
        /// <param name="sysType">字典类型,为空时获取所有类型的字典值</param>
        /// <returns>字典值信息列表</returns>
        public IQueryable<V_Dictionary> GetSysDictionaryByCategoryByUpdateDate(string category, DateTime dt)
        {
            try
            {
                
                var ents = from a in ListTemp.AsQueryable()
                           //where (string.IsNullOrEmpty(category) || a.DICTIONCATEGORY == category) && (a.UPDATEDATE > dt || a.CREATEDATE > dt)
                           //orderby a.DICTIONARYVALUE
                           select a;
                if (!string.IsNullOrEmpty(category))
                    ents = ents.Where(p=>p.DICTIONCATEGORY == category);
                if (dt != null)
                    ents = ents.Where(p=>p.CREATEDATE > dt || p.UPDATEDATE > dt);


                List<V_Dictionary> listDict = new List<V_Dictionary>();
                if (ents != null)
                {
                    if (ents.Count() > 0)
                    {
                        ents.ToList().ForEach(item => {
                            V_Dictionary ent = new V_Dictionary();
                            ent.DICTIONARYID = item.DICTIONARYID;
                            ent.DICTIONARYNAME = item.DICTIONARYNAME;
                            ent.DICTIONARYVALUE = (decimal)item.DICTIONARYVALUE;
                            ent.DICTIONCATEGORY = item.DICTIONCATEGORY;                            
                            ent.FATHERID = item.T_SYS_DICTIONARY2 == null ? "":item.T_SYS_DICTIONARY2.DICTIONARYID;
                            ent.SYSTEMCODE = item.SYSTEMCODE;
                            ent.ORDERNUMBER = item.ORDERNUMBER;
                            listDict.Add(ent);

                        });
                    }
                }
                return listDict.Count() > 0 ? listDict.AsQueryable() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("权限系统SysDictionaryBLL-GetSysDictionaryByCategory" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// 根据字典类型数组获取字典的列表 2011-5-6
        /// </summary>
        /// <param name="sysType">字典类型,为空时获取所有类型的字典值</param>
        /// <returns>字典值信息列表</returns>
        public IQueryable<V_Dictionary> GetSysDictionaryByCategoryArray(string[] category)
        {
            try
            {

                var ents = from a in ListTemp.AsQueryable()
                           
                           select a;
                if (category.Count() == 0)
                    return null;
                
                string filterstring = "";
                List<object> paras = new List<object>();
                for (int i = 0; i < category.Count(); i++)
                {
                    if (!string.IsNullOrEmpty(filterstring))
                    {
                        filterstring += " or ";
                        filterstring += " DICTIONCATEGORY ==@" + paras.Count().ToString();
                        paras.Add(category[i]);
                    }
                    else
                    {
                        filterstring = "";
                        filterstring = " DICTIONCATEGORY ==@" + paras.Count().ToString();
                        paras.Add(category[i]);
                    }
                }
                

                if (!string.IsNullOrEmpty(filterstring))
                {
                    ents = ents.Where(filterstring, paras.ToArray());
                }
                List<V_Dictionary> listDict = new List<V_Dictionary>();
                if (ents != null)
                {
                    if (ents.Count() > 0)
                    {
                        ents.ToList().ForEach(item =>
                        {
                            V_Dictionary ent = new V_Dictionary();
                            ent.DICTIONARYID = item.DICTIONARYID;
                            ent.DICTIONARYNAME = item.DICTIONARYNAME;
                            ent.DICTIONARYVALUE = (decimal)item.DICTIONARYVALUE;
                            ent.DICTIONCATEGORY = item.DICTIONCATEGORY;
                            ent.FATHERID = item.T_SYS_DICTIONARY2 == null ? "" : item.T_SYS_DICTIONARY2.DICTIONARYID;
                            ent.SYSTEMCODE = item.SYSTEMCODE;
                            ent.ORDERNUMBER = item.ORDERNUMBER;
                            listDict.Add(ent);

                        });
                    }
                }
                return listDict.Count() > 0 ? listDict.AsQueryable() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("权限系统SysDictionaryBLL-GetSysDictionaryByCategory" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// 根据父级字典ID获取子级字典值
        /// </summary>
        /// <param name="sysType">字典类型,为空时获取所有类型的字典值</param>
        /// <returns>字典值信息列表</returns>
        public IQueryable<T_SYS_DICTIONARY> GetSysDictionaryByFatherID(string fatherID)
        {
            //var ents = from a in ListTemp
            //           where a.T_SYS_DICTIONARY2.DICTIONARYID == fatherID
            //           select a;
            //return ents.AsQueryable();
            try
            {
                var ents = from a in GetObjects()
                           where a.T_SYS_DICTIONARY2.DICTIONARYID == fatherID
                           select a;
                return ents.AsQueryable();
            }
            catch (Exception ex)
            {
                Tracer.Debug("权限系统SysDictionaryBLL-GetSysDictionaryByFatherID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

        }
        /// <summary>
        /// 根据字典类型获取所有的字典值
        /// </summary>
        /// <param name="sysType">字典类型,为空时获取所有类型的字典值</param>
        /// <returns>字典值信息列表</returns>
        public List<T_SYS_DICTIONARY> GetSysDictionaryByCategory(List<string> category)
        {
            try
            {
                List<T_SYS_DICTIONARY> cats = new List<T_SYS_DICTIONARY>();
                foreach (string cat in category)
                {
                    var tmp = from a in ListTemp
                              where a.DICTIONCATEGORY == cat
                              select a;
                    cats.AddRange(tmp);
                }
                return cats;
            }
            catch (Exception ex)
            {
                Tracer.Debug("权限系统SysDictionaryBLL-GetSysDictionaryByCategory" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// 根据条件获取字典值
        /// </summary>
        /// <param name="filter">条件</param>
        /// <returns>字典值信息列表</returns>
        public List<T_SYS_DICTIONARY> GetSysDictionaryByFilter(string filter, string sort, string[] paras)
        {
            try
            {
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);

                List<T_SYS_DICTIONARY> cats = new List<T_SYS_DICTIONARY>();

                IQueryable<T_SYS_DICTIONARY> ents = ListTemp.AsQueryable();

                if (!string.IsNullOrEmpty(filter))
                {
                    ents = ents.Where(filter, queryParas.ToArray());
                }
                ents = ents.OrderBy(sort);
                cats = ents.ToList();
                return cats;
            }
            catch (Exception ex)
            {
                Tracer.Debug("权限系统SysDictionaryBLL-GetSysDictionaryByFilter" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// 根据条件获取字典值  服务器端分页
        /// </summary>
        /// <param name="filter">条件</param>
        /// <returns>字典值信息列表</returns>
        public IQueryable<T_SYS_DICTIONARY> GetSysDictionaryByFilterWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            try
            {
                var ents = from ent in ListTemp.AsQueryable()
                           select ent;
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_SYS_DICTIONARY");
                if (queryParas.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.Where(filterString, queryParas.ToArray());
                    }
                }
                ents = ents.OrderBy(sort);
                ents = Utility.Pager<T_SYS_DICTIONARY>(ents, pageIndex, pageSize, ref pageCount);
                return ents;

            }
            catch (Exception ex)
            {
                Tracer.Debug("权限系统SysDictionaryBLL-GetSysDictionaryByFilterWithPaging" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                throw (ex);
            }
        }



        public EntityObject[] GetLookupData(Dictionary<string, string> args, int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount)
        {
            try
            {
                var dicts = base.GetTable();//ListTemp.AsQueryable();
                if (args == null)
                {

                }
                else if (args["SysDictionary"] == "1")
                {

                    if (!string.IsNullOrEmpty(filterString))
                    {
                        dicts = dicts.AsQueryable().Where(filterString, paras.ToArray());
                    }
                    dicts = dicts.OrderBy(sort);


                }
                else if (args["SysDictionary"] == "2")
                {

                    if (!string.IsNullOrEmpty(filterString))
                    {
                        dicts = (dicts.AsQueryable().Where(filterString, paras.ToArray()));
                    }
                    var ents = from a in dicts.AsQueryable()
                               group a by a.DICTIONCATEGORY into temp
                               //   orderby temp.Key
                               select temp;


                    List<T_SYS_DICTIONARY> dict = new List<T_SYS_DICTIONARY>();
                    foreach (var ent in ents)
                    {
                        dict.Add(ent.FirstOrDefault());
                    }
                    return dict.Count() > 0 ? dict.ToArray() : null;
                }

                return dicts.Count() > 0 ? dicts.ToArray() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("权限系统SysDictionaryBLL-GetLookupData" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 获取字典的所有分类
        /// </summary>
        /// <returns></returns>
        public List<T_SYS_DICTIONARY> GetSysDictionaryCategory()
        {
            try
            {
                List<T_SYS_DICTIONARY> dic = new List<T_SYS_DICTIONARY>();
                var ents = from a in ListTemp.AsQueryable()
                           group a by a.DICTIONCATEGORY into temp
                           orderby temp.Key
                           select temp;
                foreach (var ent in ents)
                {
                    dic.Add(ent.FirstOrDefault());
                }

                return dic;
            }
            catch (Exception ex)
            {
                Tracer.Debug("权限系统SysDictionaryBLL-GetSysDictionaryCategory" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 获取字典的系统类型
        /// </summary>
        /// <returns></returns>
        public List<T_SYS_DICTIONARY> GetSysDictionarySysType()
        {
            try
            {
                List<T_SYS_DICTIONARY> dic = new List<T_SYS_DICTIONARY>();
                var ents = from a in ListTemp.AsQueryable()
                           group a by a.SYSTEMCODE into temp
                           orderby temp.Key
                           select temp;
                foreach (var ent in ents)
                {
                    dic.Add(ent.FirstOrDefault());
                }

                return dic;
            }
            catch (Exception ex)
            {
                Tracer.Debug("权限系统SysDictionaryBLL-GetSysDictionarySysType" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        /// <summary>
        ///根据ID获取字典
        /// </summary>
        /// <param name="dictID"></param>
        /// <returns></returns>
        public T_SYS_DICTIONARY GetSysDictionaryByID(string dictID)
        {
            try
            {
                var ent = from a in ListTemp.AsQueryable()
                          where a.DICTIONARYID == dictID
                          select a;
                return ent.Count() > 0 ? ent.FirstOrDefault() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("权限系统SysDictionaryBLL-GetSysDictionaryByID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 更新字典
        /// </summary>
        /// <param name="dict"></param>
        public void SysDictionaryUpdate(T_SYS_DICTIONARY dict, ref string strMsg)
        {
            try
            {
                
                
                var tmp = from c in GetObjects()
                          where c.DICTIONARYID != dict.DICTIONARYID && c.DICTIONCATEGORY == dict.DICTIONCATEGORY && c.DICTIONARYNAME == dict.DICTIONARYNAME
                          select c;
                if (tmp.Count() > 0)
                {
                    strMsg = "REPETITION";
                    return;
                }

                var ents = from ent in GetObjects()
                           where ent.DICTIONARYID == dict.DICTIONARYID
                           select ent;

                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();
                    decimal? dictValue = ent.DICTIONARYVALUE;
                    Utility.CloneEntity<T_SYS_DICTIONARY>(dict, ent);
                    if (ent.DICTIONARYVALUE == null)
                    {
                        ent.DICTIONARYVALUE = dictValue;
                    }
                    if (dict.T_SYS_DICTIONARY2 != null)
                    {
                        ent.T_SYS_DICTIONARY2Reference.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_DICTIONARY", "DICTIONARYID", dict.T_SYS_DICTIONARY2.DICTIONARYID);
                        ent.T_SYS_DICTIONARY2 = new T_SYS_DICTIONARY();
                        ent.T_SYS_DICTIONARY2.EntityKey = new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_DICTIONARY", "DICTIONARYID", dict.T_SYS_DICTIONARY2.DICTIONARYID);
                    }
                    else
                        ent.T_SYS_DICTIONARY2Reference = null;


                    dal.Update(ent);
                    CacheManager.RemoveCache("T_SYS_DICTIONARY");
                    EditVersion("字典");
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("权限系统SysDictionaryBLL-SysDictionaryUpdate" + System.DateTime.Now.ToString() + " " + ex.ToString());
                //throw (ex);
            }
        }

        /// <summary>
        ///新增字典
        /// </summary>
        /// <param name="dict"></param>
        public void SysDictionaryAdd(T_SYS_DICTIONARY dict, ref string strMsg)
        {
            try
            {
                var ents = from ent in GetObjects()
                           where ent.DICTIONCATEGORY == dict.DICTIONCATEGORY && ent.DICTIONARYNAME == dict.DICTIONARYNAME
                           select ent;
                if (ents.Count() > 0)
                {
                    strMsg = "REPETITION";
                    return;
                }
                var maxValue = GetObjects().Where(p => p.DICTIONCATEGORY == dict.DICTIONCATEGORY).Max(p => p.DICTIONARYVALUE);
                if (maxValue == null)
                {
                    maxValue = 1;
                }
                else
                {
                    maxValue += 1;
                }
                dict.DICTIONARYVALUE = maxValue;
                T_SYS_DICTIONARY temp = new T_SYS_DICTIONARY();
                Utility.CloneEntity<T_SYS_DICTIONARY>(dict, temp);
                temp.CREATEDATE = DateTime.Now;
                temp.UPDATEDATE = DateTime.Now;
                temp.DICTIONARYVALUE = maxValue;
                if (dict.T_SYS_DICTIONARY2 != null)
                {
                    temp.T_SYS_DICTIONARY2Reference.EntityKey =
                new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_DICTIONARY", "DICTIONARYID", dict.T_SYS_DICTIONARY2.DICTIONARYID);

                }
                dal.Add(temp);
                CacheManager.RemoveCache("T_SYS_DICTIONARY");
                EditVersion("字典");

            }
            catch (Exception ex)
            {
                Tracer.Debug("权限系统SysDictionaryBLL-SysDictionaryAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                //throw (ex);
            }
        }
        /// <summary>
        /// 删除权字典
        /// </summary>
        /// <param name="IDs">字典ID</param>
        /// <returns>是否删除成功</returns>
        public bool SysDictionaryDelete(string[] IDs)
        {

            try
            {
                dal.BeginTransaction();
                foreach (string id in IDs)
                {
                    var entitys = (from ent in dal.GetObjects()
                                   where ent.DICTIONARYID == id
                                   select ent);
                    if (entitys.Count() > 0)
                    {
                        var entity = entitys.FirstOrDefault();
                        dal.DeleteFromContext(entity);

                    }
                }
                if (dal.SaveContextChanges() > 0)
                {
                    dal.CommitTransaction();
                    CacheManager.RemoveCache("T_SYS_DICTIONARY");
                    EditVersion("字典");
                    return true;
                }
                else
                {
                    dal.RollbackTransaction();
                    return false;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("权限系统SysDictionaryBLL-SysDictionaryDelete" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                //throw (ex);
            }
        }
        /// <summary>
        /// 读取资源版本
        /// </summary>
        /// <returns></returns>
        public string ReadVersion()
        {

            try
            {
                string fileName = System.Web.HttpContext.Current.Server.MapPath("SysResourceVersion\\SysResourceVersion.xml");
                System.IO.FileStream fileStream = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
                byte[] version = new byte[fileStream.Length];
                fileStream.Read(version, 0, version.Length);
                fileStream.Close();
                string ver = System.Text.Encoding.UTF8.GetString(version);
                return ver;
            }
            catch (Exception ex)
            {
                Tracer.Debug("权限系统SysDictionaryBLL-ReadVersion" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return string.Empty;
            }

        }
        /// <summary>
        ///修改版本号
        /// </summary>
        public void EditVersion(string ResourceName)
        {
            try
            {
                string version = "0";
                string fileName = System.Web.HttpContext.Current.Server.MapPath("SysResourceVersion\\SysResourceVersion.xml");
                System.Xml.Linq.XDocument sourceFile = System.Xml.Linq.XDocument.Load(fileName);
                var ent = from xml in sourceFile.Root.Elements("Resource")
                          where xml.Attribute("Name").Value == ResourceName
                          select xml;
                if (ent.Count() > 0)
                {
                    var tmp = ent.FirstOrDefault();
                    if (tmp.Attribute("Version").Value != null)
                    {
                        version = tmp.Attribute("Version").Value;
                    }
                    double ver;
                    double.TryParse(version, out ver);
                    tmp.Attribute("Version").Value = (ver + 1).ToString();
                    tmp.Attribute("EditDate").Value = System.DateTime.Now.ToString();
                    sourceFile.Save(fileName);
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("权限系统SysDictionaryBLL-EditVersion" + ResourceName + System.DateTime.Now.ToString() + " " + ex.ToString());
            }

        }

        /// <summary>
        /// 批量导入的城市信息
        /// </summary>
        /// <param name="strPath"></param>
        /// <param name="empInfoDic"></param>
        /// <param name="strMsg"></param>
        /// <returns></returns>
        public bool ImportCityCSV(string strPath, Dictionary<string, string> empInfoDic, ref string strMsg)
        {
            try
            {
                bool flag = true;
                if (string.IsNullOrWhiteSpace(strPath) || empInfoDic == null)
                {
                    return false;
                }
                List<T_SYS_DICTIONARY> ListDetails = new List<T_SYS_DICTIONARY>();
                //Microsoft.VisualBasic.FileIO.TextFieldParser TF = new Microsoft.VisualBasic.FileIO.TextFieldParser(strPhysicalPath, Encoding.GetEncoding("GB2312"));
                //读取CSV会把数据转为字符串形式，每一行中每一列的数据以“,”逗号分开，所以依次去读取每行的每一列数据即可
                System.IO.StreamReader sr = new System.IO.StreamReader(strPath, Encoding.GetEncoding("gb2312"));
                string line = string.Empty;
                #region 一次读取每行数据添加城市字典值
                while (!string.IsNullOrEmpty(line = sr.ReadLine()))
                {
                    string[] lineTmp = line.Split(',');
                    T_SYS_DICTIONARY entTemp = new T_SYS_DICTIONARY();
                    entTemp.DICTIONARYID = Guid.NewGuid().ToString();
                    entTemp.DICTIONCATEGORY = "CITY";
                    string proName = lineTmp[0];//省份字典值
                    if (string.IsNullOrWhiteSpace(proName) || proName == "所属省份")
                    {
                        //strTemp += "没有要添加的城市";
                        continue;
                    }
                    string cityName = lineTmp[1];//添加城市名称字典
                    entTemp.DICTIONARYNAME = cityName;
                    entTemp.CREATEUSER = empInfoDic["ownerID"];
                    entTemp.CREATEDATE = DateTime.Now;
                    entTemp.UPDATEDATE = DateTime.Now;
                    entTemp.DICTIONCATEGORYNAME = "城市";
                    entTemp.SYSTEMNAME = "办公系统";
                    if (string.IsNullOrWhiteSpace(proName))
                    {
                        //strTemp += "没有要添加的城市的省份";
                        continue;
                    }
                    T_SYS_DICTIONARY dic = GetCitysPro(proName);
                    if (dic != null)
                    {
                        entTemp.T_SYS_DICTIONARY2 = dic;
                        string strTemp=string.Empty;
                        this.SysDictionaryAdd(entTemp, ref strTemp);
                        if (strTemp == "REPETITION")
                        {
                            strMsg += cityName + " 该字典值已存在\n";
                        }
                    }
                    else
                    {
                        strMsg += "没有找到 " + proName + " 的字典值\n";
                    }
                }
                #endregion
                if (!string.IsNullOrWhiteSpace(strMsg))
                {
                    flag = false;
                }
                return flag;
            }
            catch (Exception ex)
            {
                strMsg += ex.ToString();
                SMT.Foundation.Log.Tracer.Debug(" ImportCityCSV批量导入的城市信息错误:" + ex.ToString());
                return false;
            }
        }
        /// <summary>
        /// 根据城市名字获取该字典
        /// </summary>
        /// <param name="ProName"></param>
        /// <returns></returns>
        private T_SYS_DICTIONARY GetCitysPro(string ProName)
        {
            try
            {
                var ent = (from e in dal.GetObjects()
                           where e.DICTIONCATEGORY == "PROVINCE"
                           && (e.DICTIONARYNAME == ProName || e.DICTIONARYNAME.Contains(ProName) || ProName.Contains(e.DICTIONARYNAME))
                           select e).FirstOrDefault();
                if (ent != null)
                {
                    ent.REMARK = string.Empty;//去掉备注信息，因为前面用该字段暂存了城市名称
                    return ent;
                }
                return null;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(" GetCitysPro根据城市名字获取该字典:" + ex.ToString());
                return null;
            }
        }
    }
}
