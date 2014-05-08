using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;
using Smt.Global.IContract;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using SMT_OA_EFModel;
using EngineWS = SMT.SaaS.BLLCommonServices.EngineConfigWS;
using EmployeeWS = SMT.SaaS.BLLCommonServices.PersonnelWS;
using SMT.Foundation.Log;

namespace SMT.SaaS.OA.Services
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class EngineEventServices : IEventTriggerProcess
    {

        #region IEventTriggerProcess 成员
        public void EventTriggerProcess(string param)
        {
            //param = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + "<Paras><Para FuncName=\"DelayTravelreimbursmentAdd\" Name=\"OWNERID\" Description=\"strOwnerID\" Value=\"\"></Para><Para FuncName=\"DelayTravelreimbursmentAdd\" Name=\"OWNERDEPARTMENTID\" Description=\"OWNERDEPARTMENTID\" Value=\"c1f72286-eee5-45bd-bded-5993e8a317c9\"></Para><Para FuncName=\"DelayTravelreimbursmentAdd\" Name=\"OWNERPOSTID\" Description=\"strOwnerID\" Value=\"0c7a189f-fdbe-4632-a092-52c3463e0c7b\"></Para><Para FuncName=\"DelayTravelreimbursmentAdd\" Name=\"OWNERCOMPANYID\" Description=\"strOwnerID\" Value=\"cafdca8a-c630-4475-a65d-490d052dca36\"></Para><Para FuncName=\"DelayTravelreimbursmentAdd\"  Name=\"BUSINESSTRIPID\"  Description=\"出差申请ID\" Value=\"b719f1e2-3894-4ad0-b326-f68e8a618f94\" ValueName=\"出差申请ID\" ></Para></Paras>";
            //Foundation.Log.Tracer.Debug("测试调用");
            string strXml = string.Empty;
            if (param.IndexOf("<?xml version=") < 0)
            {
                strXml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" +
                         "<WcfFuncParamter>" + param +
                         "</WcfFuncParamter>";
            }
            else
            {
                strXml = param;
            }
            try
            {
                Byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(strXml);
                XElement xele = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(b)));
                var eGFunc = from c in xele.Descendants("Para")
                             select c;
                string funcName = eGFunc.FirstOrDefault().Attribute("FuncName").Value;

                switch (funcName)
                {
                    case "HouseHireRecordTrigger"://产生租房费用
                        HouseHireRecordTrigger(eGFunc);
                        break;
                    case "LicenseBorrowReturn":
                        //CreateLevelDayCountTrigger(eGFunc);
                        break;
                    case "CalculateEmployeeAttendanceMonthly":
                        //CalculateAttendMonthlyTrigger(eGFunc);
                        break;
                    case "UpdateEmployeeWorkAgeByID":
                        //UpdateEmployeeWorkAgeByID(eGFunc);
                        break;
                    case "SalarySolutionRemind":
                        //SalarySolutionRemindTrigger(eGFunc);
                        break;
                    case "DelayTravelreimbursmentAdd":
                            if (!isExistTravelReimbursement(xele))
                            {
                                string newid = System.Guid.NewGuid().ToString();
                                TravelmanagementAddFromEngine(strXml, newid);
                            }
                        break;
                    case "":
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("EngineEventServices:" + ex.ToString());
            }
        }

        #endregion

        /// <summary>
        /// 自动生成房租费用
        /// </summary>
        /// <param name="eGFunc"></param>
        private void HouseHireRecordTrigger(IEnumerable<XElement> eGFunc)
        {
            if (eGFunc.Count() == 0)
            {
                return;
            }

            string strHireAppId = string.Empty;

            foreach (var item in eGFunc)
            {
                if (item.Attribute("Name").Value == "HIREAPPID")
                {
                    strHireAppId = item.Attribute("Value").Value;
                    break;
                }
            }

            SmtOACommonAdmin svcHireApp = new SmtOACommonAdmin();
            svcHireApp.FromEngineToAddHireRecord(strHireAppId);
        }


        /// <summary>
        /// 证照归还提醒
        /// </summary>
        /// <param name="eGFunc"></param>
        private void LiscenBorrowReturnTrigger(IEnumerable<XElement> eGFunc)
        {
            if (eGFunc.Count() == 0)
            {
                return;
            }

            string strHireAppId = string.Empty;

            foreach (var item in eGFunc)
            {
                if (item.Attribute("Name").Value == "HIREAPPID")
                {
                    strHireAppId = item.Attribute("Value").Value;
                    break;
                }
            }

            SmtOACommonAdmin svcHireApp = new SmtOACommonAdmin();
            svcHireApp.FromEngineToAddHireRecord(strHireAppId);
        }

        static string postLevel=string.Empty;
        static string solutionID = string.Empty;
        static SmtOAPersonOffice doc;
        #region 根据传回的XML，添加出差申请信息
        /// <summary>
        /// 根据传回的XML，添加出差申请信息
        /// </summary>
        /// <param name="xele"></param>
        public static string TravelmanagementAddFromEngine(string strXml,string newTravelreimbursementID)
        { 
            try
            {

                string strEmployeeID = string.Empty;
                string strOwnerID = string.Empty;
                string strOwnerPostID = string.Empty;
                string strOwnerDepartmentID = string.Empty;
                string strOwnerCompanyID = string.Empty;
                string strClaimsWereName = string.Empty;
                string strCheckState = string.Empty;
                string strTEL = string.Empty;
                string strBusinesStripId = string.Empty;
                string strTravelreimbursementId = string.Empty;
                StringReader strRdr = new StringReader(strXml);
                XmlReader xr = XmlReader.Create(strRdr);
                while (xr.Read())
                {
                    if (xr.NodeType == XmlNodeType.Element)
                    {
                        string elementName = xr.Name;
                        if (elementName == "Paras" || elementName == "System")
                        {
                            while (xr.Read())
                            {
                                string type = xr.NodeType.ToString();
                                #region                               
                                if (xr["Name"] != null)
                                {
                                    if (xr["Name"].ToUpper() == "OWNERPOSTID")
                                    {
                                        strOwnerPostID = xr["Value"];
                                    }
                                    if (xr["Name"].ToUpper() == "OWNERID")
                                    {
                                        strOwnerID = xr["Value"];
                                    }
                                    if (xr["Name"].ToUpper() == "OWNERDEPARTMENTID")
                                    {
                                        strOwnerDepartmentID = xr["Value"];
                                    }
                                    if (xr["Name"].ToUpper() == "OWNERCOMPANYID")
                                    {
                                        strOwnerCompanyID = xr["Value"];
                                    }
                                    if (xr["Name"].ToUpper() == "BUSINESSTRIPID")
                                    {
                                        strBusinesStripId = xr["Value"];
                                    }
                                    if (xr["Name"].ToUpper() == "TRAVELREIMBURSEMENTID")
                                    {
                                        strTravelreimbursementId = xr["Value"];
                                    }
                                }                              
                            
                                #endregion
                            }
                        }
                    }
                }

                doc = new SmtOAPersonOffice();
                //string employeeid = strEmployeeID.Replace("{", "").Replace("}", "");
                T_OA_BUSINESSTRIP buip = doc.GetTravelmanagementById(strBusinesStripId);
                T_OA_TRAVELREIMBURSEMENT entity = new T_OA_TRAVELREIMBURSEMENT();
                entity.TRAVELREIMBURSEMENTID = newTravelreimbursementID;//Guid.NewGuid().ToString();
                entity.T_OA_BUSINESSTRIP = buip;
                entity.T_OA_BUSINESSTRIP.BUSINESSTRIPID = buip.BUSINESSTRIPID;
                entity.CLAIMSWERE = buip.OWNERID;
                entity.CLAIMSWERENAME = buip.OWNERNAME;
                entity.REIMBURSEMENTTIME = DateTime.Now;
                entity.CHECKSTATE = "0";
                entity.TEL = buip.TEL;
                entity.CREATEDATE = buip.UPDATEDATE;
                entity.OWNERID = buip.OWNERID;
                entity.OWNERNAME = buip.OWNERNAME;
                entity.OWNERPOSTID = buip.OWNERPOSTID;
                entity.OWNERDEPARTMENTID = buip.OWNERDEPARTMENTID;
                entity.OWNERCOMPANYID = buip.OWNERCOMPANYID;
                entity.CREATEUSERID = buip.CREATEUSERID;
                entity.CREATEUSERNAME = buip.CREATEUSERNAME;
                entity.CREATEPOSTID = buip.CREATEPOSTID;
                entity.CREATEDEPARTMENTID = buip.CREATEDEPARTMENTID;
                entity.CREATECOMPANYID = buip.CREATECOMPANYID;
                entity.OWNERPOSTNAME = buip.OWNERPOSTNAME;
                entity.OWNERDEPARTMENTNAME = buip.OWNERDEPARTMENTNAME;
                entity.OWNERCOMPANYNAME = buip.OWNERCOMPANYNAME;
                entity.POSTLEVEL = buip.POSTLEVEL;
                entity.STARTCITYNAME = buip.STARTCITYNAME;
                entity.ENDCITYNAME = buip.ENDCITYNAME;
                Tracer.Debug("出差终审自动生成出差报销：" + entity.OWNERNAME
                    + "-" + entity.OWNERPOSTNAME
                    + "-" + entity.OWNERDEPARTMENTNAME
                    + "-" + entity.OWNERCOMPANYNAME
                    + "-岗位级别：" + entity.POSTLEVEL
                    + "-开始城市：" + entity.STARTCITYNAME
                    + "-结束城市：" + entity.ENDCITYNAME);
                //添加子表数据
                EmployeeWS.V_EMPLOYEEDETAIL emp = new EmployeeWS.V_EMPLOYEEDETAIL();
                EmployeeWS.PersonnelServiceClient cinet = new EmployeeWS.PersonnelServiceClient();//人事服务(查询员工岗位级别用)
                List<T_OA_CANTAKETHEPLANELINE> PlaneObj = new List<T_OA_CANTAKETHEPLANELINE>();
                List<T_OA_TAKETHESTANDARDTRANSPORT> StandardObj = new List<T_OA_TAKETHESTANDARDTRANSPORT>();

                emp = cinet.GetEmployeeDetailViewByID(entity.OWNERID);//根据员工ID查询出岗位级别
                postLevel = emp.EMPLOYEEPOSTS.Where(s => s.POSTID == buip.OWNERPOSTID).FirstOrDefault().POSTLEVEL.ToString();//获取出差人的岗位级别
                var companyId = emp.EMPLOYEEPOSTS.Where(s => s.CompanyID == buip.OWNERCOMPANYID).FirstOrDefault().CompanyID.ToString();//获取出差人的所属公司
                T_OA_TRAVELSOLUTIONS travelsolutions = doc.GetTravelSolutionByCompanyID(entity.OWNERCOMPANYID, ref PlaneObj, ref StandardObj);//出差方案
                if (travelsolutions != null)
                {
                    solutionID = travelsolutions.TRAVELSOLUTIONSID;//出差方案ID
                }
                List<T_OA_BUSINESSTRIPDETAIL> TravelDetail = doc.GetBusinesstripDetail(strBusinesStripId);
                List<T_OA_REIMBURSEMENTDETAIL> TrDetail = new List<T_OA_REIMBURSEMENTDETAIL>();//出差报销子表
                List<string> cityscode = new List<string>();
                double BusinessDays = 0;
                int i = 0;
                double total = 0;
                #region
                //foreach (var detail in TravelDetail)
                for (int j = 0; j < TravelDetail.Count();j++ )
                {
                    var detail = TravelDetail[i];
                    i++;
                    double toodays = 0;

                    //计算本次出差的时间
                    List<string> list = new List<string>
                        {
                             detail.BUSINESSDAYS
                        };
                    if (detail.BUSINESSDAYS != null)
                    {
                        double totalHours = System.Convert.ToDouble(list[0]);

                        BusinessDays += totalHours;//总天数
                        toodays = totalHours;//单条数据的天数
                    }
                    double tresult = toodays;//计算本次出差的总天数

                    T_OA_REIMBURSEMENTDETAIL TrListInfo = new T_OA_REIMBURSEMENTDETAIL();
                    TrListInfo.REIMBURSEMENTDETAILID = Guid.NewGuid().ToString();

                    TrListInfo.STARTDATE = detail.STARTDATE;//开始时间
                    TrListInfo.ENDDATE = detail.ENDDATE;//结束时间
                    TrListInfo.BUSINESSDAYS = detail.BUSINESSDAYS;//出差天数
                    TrListInfo.DEPCITY = detail.DEPCITY;//出发城市
                    TrListInfo.DESTCITY = detail.DESTCITY;//目标城市
                    TrListInfo.PRIVATEAFFAIR = detail.PRIVATEAFFAIR;//是否私事
                    TrListInfo.GOOUTTOMEET = detail.GOOUTTOMEET;//外出开会
                    TrListInfo.COMPANYCAR = detail.COMPANYCAR;//公司派车
                    TrListInfo.TYPEOFTRAVELTOOLS = detail.TYPEOFTRAVELTOOLS;//交通工具类型
                    TrListInfo.TAKETHETOOLLEVEL = detail.TAKETHETOOLLEVEL;//交通工具级别
                    TrListInfo.CREATEDATE = Convert.ToDateTime(buip.UPDATEDATE);//创建时间
                    TrListInfo.CREATEUSERNAME = buip.CREATEUSERNAME;//创建人
                    cityscode.Add(TrListInfo.DESTCITY);

                    T_OA_AREAALLOWANCE entareaallowance = new T_OA_AREAALLOWANCE();
                    string cityValue = cityscode[i - 1];//目标城市值
                    entareaallowance = GetAllowanceByCityValue(cityValue);

                    #region 根据本次出差的总天数,根据天数获取相应的补贴
                    if (travelsolutions != null)
                    {
                        if (tresult <= int.Parse(travelsolutions.MINIMUMINTERVALDAYS))//本次出差总时间小于等于设定天数的报销标准
                        {
                            if (entareaallowance != null)
                            {
                                if (detail.BUSINESSDAYS != null)
                                {
                                    if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                    {
                                        TrListInfo.TRANSPORTATIONSUBSIDIES = 0;//交通补贴
                                    }
                                    else if (detail.GOOUTTOMEET == "1" || detail.COMPANYCAR == "1")//如果是开会或者是公司派车，交通费没有
                                    {
                                        TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                    }
                                    else
                                    {
                                        if (int.Parse(postLevel) > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                        {
                                            if (entareaallowance.TRANSPORTATIONSUBSIDIES != null)
                                            {
                                                TrListInfo.TRANSPORTATIONSUBSIDIES = decimal.Parse((Convert.ToDouble(entareaallowance.TRANSPORTATIONSUBSIDIES) * toodays).ToString());
                                            }
                                        }
                                        else
                                        {
                                            TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                        }
                                    }
                                }

                                if (detail.BUSINESSDAYS != null)
                                {
                                    if (detail.PRIVATEAFFAIR == "1")//餐费补贴
                                    {
                                        TrListInfo.MEALSUBSIDIES = 0;
                                    }
                                    else if (detail.GOOUTTOMEET == "1")//如果是开会
                                    {
                                        TrListInfo.MEALSUBSIDIES = 0;
                                    }
                                    else
                                    {
                                        if (int.Parse(postLevel) > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                        {
                                            TrListInfo.MEALSUBSIDIES = decimal.Parse((Convert.ToDouble(entareaallowance.MEALSUBSIDIES) * toodays).ToString());
                                        }
                                        else
                                        {
                                            TrListInfo.MEALSUBSIDIES = 0;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (int.Parse(postLevel) <= 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                            {
                                TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                TrListInfo.MEALSUBSIDIES = 0;
                            }
                        }
                    }
                    #endregion

                    #region 如果出差天数大于设定的最大天数,按驻外标准获取补贴
                    if (travelsolutions != null)
                    {
                        if (tresult > int.Parse(travelsolutions.MAXIMUMRANGEDAYS))
                        {
                            if (entareaallowance != null)
                            {
                                double DbTranceport = Convert.ToDouble(entareaallowance.TRANSPORTATIONSUBSIDIES);
                                double DbMeal = Convert.ToDouble(entareaallowance.MEALSUBSIDIES);
                                double tfSubsidies = Convert.ToDouble(entareaallowance.TRANSPORTATIONSUBSIDIES) * (Convert.ToDouble(travelsolutions.INTERVALRATIO) / 100);
                                double mealSubsidies = Convert.ToDouble(entareaallowance.MEALSUBSIDIES) * (Convert.ToDouble(travelsolutions.INTERVALRATIO) / 100);

                                if (detail.BUSINESSDAYS != null)
                                {
                                    if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                    {
                                        TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                    }
                                    else if (detail.GOOUTTOMEET == "1" || detail.COMPANYCAR == "1")//如果是开会或者是公司派车，交通费没有
                                    {
                                        TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                    }
                                    else
                                    {
                                        if (int.Parse(postLevel) > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                        {
                                            double minmoney = Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS) * DbTranceport;
                                            double middlemoney = (Convert.ToDouble(travelsolutions.MAXIMUMRANGEDAYS) - Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS)) * tfSubsidies;
                                            double lastmoney = (tresult - Convert.ToDouble(travelsolutions.MAXIMUMRANGEDAYS)) * Convert.ToDouble(entareaallowance.OVERSEASSUBSIDIES);
                                            TrListInfo.TRANSPORTATIONSUBSIDIES = decimal.Parse((minmoney + middlemoney + lastmoney).ToString());
                                        }
                                        else
                                        {
                                            TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                        }
                                    }
                                }

                                if (detail.BUSINESSDAYS != null)
                                {
                                    if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                    {
                                        TrListInfo.MEALSUBSIDIES = 0;
                                    }
                                    else if (detail.GOOUTTOMEET == "1")//如果是开会
                                    {
                                        TrListInfo.MEALSUBSIDIES = 0;
                                    }
                                    else
                                    {
                                        if (int.Parse(postLevel) > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                        {
                                            double minmoney = Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS) * DbMeal;
                                            double middlemoney = (Convert.ToDouble(travelsolutions.MAXIMUMRANGEDAYS) - Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS)) * mealSubsidies;
                                            double lastmoney = (tresult - Convert.ToDouble(travelsolutions.MAXIMUMRANGEDAYS)) * Convert.ToDouble(entareaallowance.OVERSEASSUBSIDIES);
                                            TrListInfo.MEALSUBSIDIES = decimal.Parse((minmoney + middlemoney + lastmoney).ToString());

                                        }
                                        else
                                        {
                                            TrListInfo.MEALSUBSIDIES = 0;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (int.Parse(postLevel) <= 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                            {
                                TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                TrListInfo.MEALSUBSIDIES = 0;
                            }
                        }
                    }
                    #endregion



                #region 如果出差时间大于设定的最小天数并且小于设定的最大天数的报销标准
                if (travelsolutions != null)
                {
                    if (tresult >= Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS) && tresult <= Convert.ToDouble(travelsolutions.MAXIMUMRANGEDAYS))
                    {
                        if (entareaallowance != null)
                        {
                            double DbTranceport = Convert.ToDouble(entareaallowance.TRANSPORTATIONSUBSIDIES);
                            double DbMeal = Convert.ToDouble(entareaallowance.MEALSUBSIDIES);
                            double tfSubsidies = Convert.ToDouble(entareaallowance.TRANSPORTATIONSUBSIDIES) * (Convert.ToDouble(travelsolutions.INTERVALRATIO) / 100);
                            double mealSubsidies = Convert.ToDouble(entareaallowance.MEALSUBSIDIES) * (Convert.ToDouble(travelsolutions.INTERVALRATIO) / 100);

                            if (detail.BUSINESSDAYS != null)
                            {
                                if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                {
                                    TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                }
                                else if (detail.GOOUTTOMEET == "1" || detail.COMPANYCAR == "1")//如果是开会或者是公司派车，交通费没有
                                {
                                    TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                }
                                else
                                {
                                    if (int.Parse(postLevel) > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                    {
                                        double minmoney = Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS) * DbTranceport;
                                        double middlemoney = (tresult - Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS)) * tfSubsidies;
                                        TrListInfo.TRANSPORTATIONSUBSIDIES = decimal.Parse((minmoney + middlemoney).ToString());
                                    }
                                    else
                                    {
                                        TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                    }
                                }
                            }

                            if (detail.BUSINESSDAYS != null)
                            {
                                if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                {
                                    TrListInfo.MEALSUBSIDIES = 0;
                                }
                                else if (detail.GOOUTTOMEET == "1")//如果是开会
                                {
                                    TrListInfo.MEALSUBSIDIES = 0;
                                }
                                else
                                {
                                    if (int.Parse(postLevel) > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                    {
                                        //最小区间段金额
                                        double minmoney = Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS) * DbMeal;
                                        //中间区间段金额
                                        double middlemoney = (tresult - Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS)) * mealSubsidies;
                                        TrListInfo.MEALSUBSIDIES = decimal.Parse((minmoney + middlemoney).ToString());
                                    }
                                    else
                                    {
                                        TrListInfo.MEALSUBSIDIES = 0;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (int.Parse(postLevel) <= 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                        {
                            TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                            TrListInfo.MEALSUBSIDIES = 0;
                        }
                    }
                }
                total += Convert.ToDouble(TrListInfo.TRANSPORTATIONSUBSIDIES + TrListInfo.MEALSUBSIDIES);
                entity.THETOTALCOST = decimal.Parse(total.ToString());//差旅费用总和
                entity.REIMBURSEMENTOFCOSTS = decimal.Parse(total.ToString());//报销费用总和

                #endregion

                TrDetail.Add(TrListInfo);
                }
                #endregion

                string result = BusinessDays.ToString(); //计算本次出差的总时间,超过24小时天数加1
                entity.COMPUTINGTIME = result;//总时间

                //doc.TravelReimbursementAdd(entity, TrDetail);
                doc.TravelReimbursementAddSimple(entity, TrDetail,strBusinesStripId);


                return null;//entity.TRAVELREIMBURSEMENTID;
            }
            catch (Exception e)
            {
                string abc = "<OA>Message=[" + e.Message + "]" + "<OA>Source=[" + e.Source + "]<OA>StackTrace=[" + e.StackTrace + "]<OA>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;
            }
        }
        #endregion

        #region
        private static T_OA_AREAALLOWANCE GetAllowanceByCityValue(string CityValue)
        {
            SmtOAPersonOffice doc = new SmtOAPersonOffice();
            List<T_OA_AREACITY> citys = new List<T_OA_AREACITY>();
            List<T_OA_AREAALLOWANCE> areaallowance = doc.GetTravleAreaAllowanceByPostValue(postLevel, solutionID, ref citys);//出差补贴

            var q = from ent in areaallowance
                    join ac in citys on ent.T_OA_AREADIFFERENCE.AREADIFFERENCEID equals ac.T_OA_AREADIFFERENCE.AREADIFFERENCEID
                    where ac.CITY == CityValue && ent.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == solutionID
                    select ent;

            if (q.Count() > 0)
            {
                return q.FirstOrDefault();
            }
            return null;
        }
        #endregion

        #region 检测是否有相关初查申请已存在的出差报销
        private bool isExistTravelReimbursement( XElement xele)
        {
            bool isExist = false;
            if (xele != null)
            {
                var businessid = from e in xele.Descendants("Para")
                                 where e.Attribute("Name").Value.ToUpper() == "BUSINESSTRIPID"
                                 select e;
                if (businessid != null)
                {
                    doc = new SmtOAPersonOffice();
                    string busntpId = businessid.FirstOrDefault().Attribute("Value").Value;
                    isExist = doc.CheckTravelReimbursementByBusinesstrip(busntpId);
                }
            }
            return isExist;
        }
        #endregion
    }
}
