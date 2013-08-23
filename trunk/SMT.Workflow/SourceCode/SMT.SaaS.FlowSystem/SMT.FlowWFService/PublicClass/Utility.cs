using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Linq;

namespace SMT.FlowWFService.PublicClass
{
    public class Utility
    {
        /// <summary>
        /// 通过节点设置获取节点对应子模块代码
        /// </summary>
        /// <param name="ActiveRule"></param>
        /// <param name="StateCode"></param>
        /// <returns></returns>
        public static string GetSubModelCode(string ActiveRule, string StateCode)
        {
            XmlReader xmlreader;
            string SubModelCode = "";
            StringReader tmpLayout;
            try
            {


                tmpLayout = new StringReader(ActiveRule);
                xmlreader = XmlReader.Create(tmpLayout);
                XElement XElementS = XElement.Load(xmlreader);
                var a = from c in XElementS.Descendants("Activity")
                        where c.Attribute("Name").Value == StateCode
                        select c.Attribute("ModelCode").Value;
                if (a.Count() > 0)
                {
                    SubModelCode = a.First().ToString();
                }
                else
                    SubModelCode = null;

                return SubModelCode;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                xmlreader = null;
                SubModelCode = null;
                tmpLayout = null;
            }
        }

        /// <summary>
        /// 通过节点设置获取节点对应处理角色
        /// </summary>
        /// <param name="ActiveRule"></param>
        /// <param name="StateCode"></param>
        /// <returns></returns>
        public static Role_UserType GetRlueName(string ActiveRule, string StateCode)
        {
            XmlReader xmlreader;
            Role_UserType RuleName = null;
            StringReader tmpLayout = null;
            try
            {
                if (StateCode.ToUpper() == "ENDFLOW")
                {
                    RuleName = new Role_UserType();
                    RuleName.RoleName = StateCode;
                    RuleName.UserType = StateCode;
                }
                else
                {
                    tmpLayout = new StringReader(ActiveRule);
                    xmlreader = XmlReader.Create(tmpLayout);
                    XElement XElementS = XElement.Load(xmlreader);

                    #region beyond
                    //var a = from c in XElementS.Descendants("Activity")
                    //        where c.Attribute("Name").Value == StateCode
                    //        select new Role_UserType { RoleName = c.Attribute("RoleName").Value, UserType = c.Attribute("UserType").Value };

                    //var a = from c in XElementS.Descendants("Activity")
                    //        where c.Attribute("Name").Value == StateCode
                    //        select new Role_UserType
                    //        {
                    //            RoleName = c.Attribute("RoleName").Value,
                    //            UserType = c.Attribute("UserType").Value,
                    //            IsOtherCompany = c.Attribute("IsOtherCompany") == null ? false : bool.Parse(c.Attribute("IsOtherCompany").Value),
                    //            OtherCompanyID = c.Attribute("OtherCompanyID") == null ? "" : c.Attribute("OtherCompanyID").Value
                    //        };
                    List<Role_UserType> a = new List<Role_UserType>();
                    XElementS.Descendants("Activity").Where(xestate => xestate.Attribute("Name").Value == StateCode).ToList().ForEach(c =>
                    {
                        Role_UserType ru = new Role_UserType();
                        ru.RoleName = c.Attribute("RoleName").Value;
                        ru.UserType = c.Attribute("UserType").Value;
                        XAttribute xIsOtherCompany = c.Attribute("IsOtherCompany");
                        XAttribute xOtherCompanyID = c.Attribute("OtherCompanyID");
                        if (xIsOtherCompany == null)
                        {
                            ru.IsOtherCompany = null;
                        }
                        else
                        {
                            ru.IsOtherCompany = bool.Parse(xIsOtherCompany.Value);
                            ru.OtherCompanyID = xOtherCompanyID.Value;
                        }
                        a.Add(ru);
                    });
                    #endregion 
                    if (a.Count() > 0)
                    {
                        RuleName = a.First();
                    }
                }
                return RuleName;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                xmlreader = null;
                RuleName = null;
                tmpLayout = null;
            }
        }

        public static void IsCountersign(string ActiveRule, string StateCode, ref bool isCountersign, ref string CountersignType)
        {
            XmlReader xmlreader;
           
            StringReader tmpLayout = null;
           
            try
            {
                if (StateCode.ToUpper() == "ENDFLOW")
                {
                  
                    isCountersign = false;
                }
                else
                {
                    #region
                    tmpLayout = new StringReader(ActiveRule);
                    xmlreader = XmlReader.Create(tmpLayout);
                    XElement XElementS = XElement.Load(xmlreader);
                    XElement xActivity = XElementS.Descendants("Activity").FirstOrDefault(xestate => xestate.Attribute("Name").Value == StateCode);
                    if (xActivity == null)
                    {
                        isCountersign = false;
                        return;
                    }
                    XElement xeCountersigns = xActivity.Element("Countersigns");
                    if (xeCountersigns == null)
                    {
                        #region
                       
                        isCountersign = false;
                        #endregion

                    }
                    else
                    {
                        #region
                        isCountersign = true;
                        CountersignType = xeCountersigns.Attribute("CountersignType").Value;
                       
                        #endregion
                    }
                    #endregion
                }
                
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                xmlreader = null;
                
                tmpLayout = null;
            }

        }     
        /// <summary>
        /// 根活动的字符串查找角色
        /// </summary>
        /// <param name="ActiveRule">活动的字符串</param>
        /// <param name="StateCode">状态码(即活动Name属性)</param>
        /// <param name="isCountersign">是否是会签</param>
        /// <param name="CountersignType">会签类型:0全部同意通过才能能过,1只有一个人通过即通过</param>
        /// <returns></returns>
        public static List<Role_UserType> GetRlueName2(string ActiveRule, string StateCode, ref bool isCountersign,ref string CountersignType)
        {
            XmlReader xmlreader;
            Role_UserType RuleName = null;
            StringReader tmpLayout = null;
            List<Role_UserType> list = new List<Role_UserType>();
            try
            {
                if (StateCode.ToUpper() == "ENDFLOW")
                {
                    RuleName = new Role_UserType();
                    RuleName.RoleName = StateCode;
                    RuleName.UserType = StateCode;
                    list.Add(RuleName);
                    isCountersign = false;
                }
                else
                {
                    #region 
                    tmpLayout = new StringReader(ActiveRule);
                    xmlreader = XmlReader.Create(tmpLayout);
                    XElement XElementS = XElement.Load(xmlreader);
                    XElement xActivity = XElementS.Descendants("Activity").FirstOrDefault(xestate => xestate.Attribute("Name").Value == StateCode);
                    if (xActivity == null)
                    {
                        return list;
                    }
                    XElement xeCountersigns = xActivity.Element("Countersigns");
                    if (xeCountersigns == null)
                    {
                        #region 非会签
                        RuleName = new Role_UserType();
                        RuleName.RoleName = xActivity.Attribute("RoleName").Value;
                        RuleName.UserType = xActivity.Attribute("UserType").Value;
                        RuleName.Remark =xActivity.Attribute("Remark")!=null? xActivity.Attribute("Remark").Value:"";
                        XAttribute xIsOtherCompany = xActivity.Attribute("IsOtherCompany");
                        XAttribute xOtherCompanyID = xActivity.Attribute("OtherCompanyID");
                        if (xIsOtherCompany == null)
                        {
                            RuleName.IsOtherCompany = null;
                        }
                        else
                        {
                            RuleName.IsOtherCompany = bool.Parse(xIsOtherCompany.Value);
                            RuleName.OtherCompanyID = xOtherCompanyID.Value;
                        }
                        list.Add(RuleName);
                        isCountersign = false;
                        #endregion 

                    }
                    else
                    {
                        #region  会签
                        isCountersign = true;
                        CountersignType=xeCountersigns.Attribute("CountersignType").Value;
                        xeCountersigns.Elements().ToList().ForEach(item =>
                        {
                            RuleName = new Role_UserType();
                            RuleName.RoleName = item.Attribute("StateType").Value;
                            RuleName.Remark = item.Attribute("RoleName").Value;
                            RuleName.UserType = item.Attribute("UserType").Value;
                            XAttribute xIsOtherCompany = item.Attribute("IsOtherCompany");
                            XAttribute xOtherCompanyID = item.Attribute("OtherCompanyID");
                            if (xIsOtherCompany == null)
                            {
                                RuleName.IsOtherCompany = null;
                            }
                            else
                            {
                                RuleName.IsOtherCompany = bool.Parse(xIsOtherCompany.Value);
                                RuleName.OtherCompanyID = xOtherCompanyID.Value;
                            }
                            //RuleName.IsOtherCompany = item.Attribute("IsOtherCompany") == null ? false : bool.Parse(item.Attribute("IsOtherCompany").Value);
                            RuleName.OtherCompanyID = item.Attribute("OtherCompanyID") == null ? "" : item.Attribute("OtherCompanyID").Value;
                            list.Add(RuleName);
                        });
                        #endregion 
                    }
                    #endregion 
                }
                return list;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                xmlreader = null;
                RuleName = null;
                tmpLayout = null;
            }
        }

        public static string GetActiveRlue(string Layout)
        {
            XmlReader xmlreader;
            string ActiveRlue = "";
            StringReader tmpLayout = null;
            try
            {


                tmpLayout = new StringReader(Layout);
                xmlreader = XmlReader.Create(tmpLayout);
                XElement XElementS = XElement.Load(xmlreader);
                var a = from c in XElementS.Elements("Activitys")

                        select c;
                if (a.Count() > 0)
                {
                    ActiveRlue = a.First().ToString();
                }
                else
                    ActiveRlue = null;

                return ActiveRlue;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                xmlreader = null;
                ActiveRlue = null;
                tmpLayout = null;
            }
        }

        public static string GetString(string parm)
        {
            return parm == null ? "" : parm;
        }

        public static List<string> FlowTypeListToStringList(List<FlowType> FlowTypeList)
        {
            if (FlowTypeList == null) return null;
            List<string> tmpStringList = new List<string>();
            for (int i = 0; i < FlowTypeList.Count; i++)
            {
                tmpStringList.Add(((int)FlowTypeList[i]).ToString());
            }
            return tmpStringList;
        }
    }
}

