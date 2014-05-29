/********************************************************************************
//出差申请form，alter by ken 2013/3/27
*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.OA.UI.Class;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.PermissionWS;
using System.Windows.Data;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using System.Windows.Input;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.SelectCityControl;
using System.Windows.Media;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Platform.Logging;
using SMT.SaaS.MobileXml;

namespace SMT.SaaS.OA.UI.Views.Travelmanagement
{
    public partial class  TravelRequestForm
    {
        #region "获取出差报销补助3"
        public List<T_OA_AREAALLOWANCE> areaallowance = new List<T_OA_AREAALLOWANCE>();
        public List<T_OA_AREACITY> areacitys;
        /// <summary>
        /// 根据岗位级别获取出差报销补助
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OaPersonOfficeClient_GetTravleAreaAllowanceByPostValueCompleted(object sender, GetTravleAreaAllowanceByPostValueCompletedEventArgs e)
        {
            try
            {

                if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);

                }
                else
                {
                    if (e.Result != null)
                    {
                        areaallowance = e.Result.ToList();
                        areacitys = e.citys.ToList();
                    }
                    if (e.Result.Count() == 0)
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "您公司的出差方案没有对应的出差补贴", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
            finally
            {
                RefreshUI(RefreshedTypes.HideProgressBar);

                if (formType != FormTypes.New && Master_Golbal.T_OA_BUSINESSTRIPDETAIL.Count > 0)
                {
                    BindDataGrid(Master_Golbal.T_OA_BUSINESSTRIPDETAIL);
                }
                else
                {
                    RefreshUI(RefreshedTypes.All);
                }
            }
        }

        #endregion

        #region 根据城市设置出差报销标准并显示
        /// <summary>
        /// 获取出差报销补助
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private T_OA_AREAALLOWANCE StandardsMethod(int i)
        {
            string noAllowancePostlevelName = string.Empty;
            if (travelsolutions_Golbal == null) return null;
            double noAllowancePostLevel = Convert.ToDouble(travelsolutions_Golbal.NOALLOWANCEPOSTLEVEL);
            if (!string.IsNullOrEmpty(travelsolutions_Golbal.NOALLOWANCEPOSTLEVEL))
            {

                var ents = from a in Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>
                           where a.DICTIONCATEGORY == "POSTLEVEL" && a.DICTIONARYVALUE == Convert.ToDecimal(travelsolutions_Golbal.NOALLOWANCEPOSTLEVEL)
                           select new
                           {
                               DICTIONARYNAME = a.DICTIONARYNAME,
                               DICTIONARYVALUE = a.DICTIONARYVALUE
                           };

                noAllowancePostlevelName = ents.FirstOrDefault().DICTIONARYNAME;
            }


            T_OA_AREAALLOWANCE entareaallowance = new T_OA_AREAALLOWANCE();
            textStandards.Text = string.Empty;
            if (TraveDetailList_Golbal.Count() == 1)   //只有一条记录的情况
            {
                string cityend = TraveDetailList_Golbal[0].DESTCITY.Replace(",", "");//目标城市值
                entareaallowance = this.GetAllowanceByCityValue(cityend);
                if (Master_Golbal.POSTLEVEL.ToInt32() <= noAllowancePostLevel)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                {
                    textStandards.Text = textStandards.Text + "出差城市：" + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityend)
                            + "  您的岗位级别≥'" + noAllowancePostlevelName + "'级，无各项差旅补贴。";
                    if (entareaallowance == null)
                    {
                        textStandards.Text = textStandards.Text + "住宿标准：未获取到。"
                             + "\n";
                    }
                    else
                    {
                        if (entareaallowance.ACCOMMODATION == null)
                        {
                            textStandards.Text = textStandards.Text + "住宿标准：未获取到。"
                               + "\n";
                        }
                        else
                        {
                            textStandards.Text = textStandards.Text + "住宿标准：" + entareaallowance.ACCOMMODATION + "元。"
                                + "\n";
                        }
                    }
                    //detail.TRANSPORTATIONSUBSIDIES = 0;
                    //detail.MEALSUBSIDIES = 0;
                    return null;
                }
                if (textStandards.Text.Contains(SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityend)))
                {
                    //已经包含，直接跳过
                    return entareaallowance;
                }

                if (TraveDetailList_Golbal[0].PRIVATEAFFAIR == "1")//如果是私事
                {
                    textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityend)
                        + "的出差报销标准是：交通补贴：" + "无" + "，餐费补贴："
                        + "无" + "，住宿标准：无。"
                        + "\n";
                }
                else if (TraveDetailList_Golbal[0].GOOUTTOMEET == "1")//如果是内部会议及培训
                {
                    textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityend)
                        + "的出差为《内部会议、培训》，无各项差旅补贴。"
                        + "\n";
                }
                else if (TraveDetailList_Golbal[0].COMPANYCAR == "1")//如果是公司派车
                {
                    textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityend)
                        + "的出差报销标准是：交通补贴：" + "无" + "餐费补贴：" + entareaallowance.MEALSUBSIDIES.ToString()
                        + "元，住宿标准：" + entareaallowance.ACCOMMODATION + "元。"
                        + "\n";
                    //textStandards.Text += "(以上为员工现岗位级别的补贴，仅供参考)";
                }
                else if (Master_Golbal.POSTLEVEL.ToInt32() <= noAllowancePostLevel)//当前用户的岗位级别小于副部长及以上级别的无各项补贴
                {
                    //textStandards.Text = "您的岗位级别≥'I'级，无各项差旅补贴。";
                    textStandards.Text = textStandards.Text + "出差城市：" + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityend)
                        + "  您的岗位级别≥'" + noAllowancePostlevelName + "'级，无各项差旅补贴。";
                    textStandards.Text = textStandards.Text + "住宿标准：" + entareaallowance.ACCOMMODATION + "元。"
                        + "\n";
                    //textStandards.Text += "(以上为员工现岗位级别的补贴，仅供参考)";
                }
                else
                {
                    textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityend)
                        + "的出差报销标准是：交通补贴：" + entareaallowance.TRANSPORTATIONSUBSIDIES
                        + "元，餐费补贴：" + entareaallowance.MEALSUBSIDIES.ToString()
                        + "元，住宿标准：" + entareaallowance.ACCOMMODATION + "元。"
                        + "\n";
                    //textStandards.Text += "(以上为员工现岗位级别的补贴，仅供参考)";
                }
            }
            else
            {
                for (int j = 0; j < TraveDetailList_Golbal.Count() - 1; j++)//最后一条记录没有补贴
                {
                    if (string.IsNullOrEmpty(TraveDetailList_Golbal[j].DESTCITY)) continue;
                    string city = TraveDetailList_Golbal[j].DESTCITY.Replace(",", "");//目标城市值
                    entareaallowance = this.GetAllowanceByCityValue(city);
                    if (Master_Golbal.POSTLEVEL.ToInt32() <= noAllowancePostLevel)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                    {
                        textStandards.Text = textStandards.Text + "出差城市：" + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(city)
                                + "  您的岗位级别≥'" + noAllowancePostlevelName + "'级，无各项差旅补贴。";
                        if (entareaallowance == null)
                        {
                            textStandards.Text = textStandards.Text + "住宿标准：未获取到。"
                                 + "\n";
                        }
                        else
                        {
                            if (entareaallowance.ACCOMMODATION == null)
                            {
                                textStandards.Text = textStandards.Text + "住宿标准：未获取到。"
                                   + "\n";
                            }
                            else
                            {
                                textStandards.Text = textStandards.Text + "住宿标准：" + entareaallowance.ACCOMMODATION + "元。"
                                    + "\n";
                            }
                        }
                        //detail.TRANSPORTATIONSUBSIDIES = 0;
                        //detail.MEALSUBSIDIES = 0;
                        //return null;
                    }
                    if (textStandards.Text.Contains(SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(city)))
                    {
                        //已经包含，直接跳过
                        continue;
                    }
                    if (entareaallowance != null)//根据出差的城市及出差人的级别，将当前出差人的标准信息显示在备注中
                    {
                        if (TraveDetailList_Golbal[j].PRIVATEAFFAIR == "1")//如果是私事
                        {
                            textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(city)
                                + "的出差报销标准是：交通补贴：" + "无" + ",餐费补贴：" + "无" + ",住宿标准：" + "无。"
                                + "\n";
                        }
                        else if (TraveDetailList_Golbal[j].GOOUTTOMEET == "1")//如果是内部会议及培训
                        {
                            //textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityValue) + "的出差为《内部会议、培训》，无各项差旅补贴。\n";
                            textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(city)
                                + "的出差为《内部会议、培训》，无各项差旅补贴。"
                                + "\n";
                        }
                        else if (TraveDetailList_Golbal[j].COMPANYCAR == "1")//如果是公司派车
                        {
                            textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(city)
                                + "的出差报销标准是：交通补贴：" + "无" + ",餐费补贴：" + entareaallowance.MEALSUBSIDIES.ToString()
                                + "元,住宿标准：" + entareaallowance.ACCOMMODATION + "元。"
                                + "\n";
                            //textStandards.Text += "(以上为员工现岗位级别的补贴，仅供参考)";
                        }
                        else if (Master_Golbal.POSTLEVEL.ToInt32() <= noAllowancePostLevel)//当前用户的岗位级别小于副部长及以上级别的无各项补贴
                        {
                            //textStandards.Text = "您的岗位级别≥'I'级,无各项差旅补贴。";
                            textStandards.Text = textStandards.Text + "出差城市：" + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(city)
                                + "  您的岗位级别≥'" + noAllowancePostlevelName + "'级，无各项差旅补贴。";
                            textStandards.Text = textStandards.Text + "住宿标准：" + entareaallowance.ACCOMMODATION + "元。"
                                + "\n";
                            //textStandards.Text += "(以上为员工现岗位级别的补贴，仅供参考)";
                        }
                        else
                        {
                            textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(city)
                                + "的出差报销标准是：交通补贴：" + entareaallowance.TRANSPORTATIONSUBSIDIES
                                + "元，餐费补贴：" + entareaallowance.MEALSUBSIDIES.ToString()
                                + "元，住宿标准：" + entareaallowance.ACCOMMODATION + "元。"
                                + "\n";
                            //textStandards.Text += "(以上为员工现岗位级别的补贴，仅供参考)";
                        }
                    }
                    else
                    {
                        textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(city) + "没有相应的出差标准。"
                            + "\n";
                    }
                }
            }


            string cityValue = TraveDetailList_Golbal[i].DESTCITY.Replace(",", "");//目标城市值
            entareaallowance = this.GetAllowanceByCityValue(cityValue);
            if (textStandards.Text.Contains(SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityValue)))
            {
                //已经包含，直接返回
                return entareaallowance;
            }
            if (i == TraveDetailList_Golbal.Count)
            {
                //出差结束城市无补贴
                return entareaallowance;
            }

            return entareaallowance;
        }

        /// <summary>
        /// 根据城市值  获取相应的出差补贴
        /// </summary>
        /// <param name="CityValue"></param>
        private T_OA_AREAALLOWANCE GetAllowanceByCityValue(string CityValue)
        {
            CityValue = CityValue.Replace(",", "");
            var q = from ent in areaallowance
                    join ac in areacitys on ent.T_OA_AREADIFFERENCE.AREADIFFERENCEID equals ac.T_OA_AREADIFFERENCE.AREADIFFERENCEID
                    where ac.CITY == CityValue && ent.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == travelsolutions_Golbal.TRAVELSOLUTIONSID
                    select ent;

            if (q.Count() > 0)
            {
                return q.FirstOrDefault();
            }
            return null;
        }

        #endregion  
    }
}
