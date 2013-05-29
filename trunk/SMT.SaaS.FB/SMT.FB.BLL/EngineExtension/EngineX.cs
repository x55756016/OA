using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data.Objects.DataClasses;
using System.Xml.Linq;
using EngineConfigWS = SMT.SaaS.BLLCommonServices.EngineConfigWS;


namespace SMT.FB.BLL
{
    public class EngineX
    {
    
        private const string  xml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
                                <System>
                                  <CompanyCode></CompanyCode>
                                  <SystemCode>FB</SystemCode>
                                  <ModelCode></ModelCode>
                                  <ApplicationOrderCode></ApplicationOrderCode>
                                  <TaskStartDate></TaskStartDate>
                                  <TaskStartTime></TaskStartTime>
                                  <ProcessCycle></ProcessCycle>
                                  <ReceiveUser></ReceiveUser>
                                  <ReceiveRole></ReceiveRole>
                                  <MessageBody></MessageBody>
                                  <MsgLinkUrl></MsgLinkUrl>
                                  <ProcessWcfUrl></ProcessWcfUrl>
                                  <WcfFuncName></WcfFuncName>
                                  <TriggerType>System</TriggerType>
                                  <WcfFuncParamter></WcfFuncParamter>
                                  <WcfParamSplitChar>Г</WcfParamSplitChar>
                                  <WcfBinding>basicHttpBinding</WcfBinding>
                                </System>
                                ";
        public static bool ConfigCheckDate(DateTime checkDate)
        {
            string startDate = string.Format("{0}/{1}/{2}", checkDate.Year, checkDate.Month, checkDate.Day);
            List<string> paramValues = new List<string>();
            Dictionary<string, string> dictValues = new Dictionary<string,string>();
            dictValues.Add("ApplicationOrderCode", "FB_CHECKDATE");
            dictValues.Add("TaskStartDate", startDate);
            dictValues.Add("TaskStartTime", checkDate.ToString("HH:mm"));
            dictValues.Add("ProcessCycle", "Month");
            dictValues.Add("ProcessWcfUrl", FBCommonBLL.FBServiceUrl);
            dictValues.Add("WcfFuncName", "EventTriggerProcess");
            
            XElement xElement = XElement.Parse(xml);

            xElement.Elements().ToList().ForEach(item =>
                {
                    string value = "";
                    dictValues.TryGetValue(item.Name.LocalName, out value);
                    if (value != null)
                    {
                        item.Value = value; 
                    }                   
                });
            EngineConfigWS.EngineWcfGlobalFunctionClient engineService = new EngineConfigWS.EngineWcfGlobalFunctionClient();

            engineService.SaveEventData(@"<?xml version=""1.0"" encoding=""utf-8"" ?>" + xElement.ToString());

            return true;
        }
    }
}
