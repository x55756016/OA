using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Text;
using System.Collections.Generic;
using System.Linq;



namespace SMT.HRM.UI.Active
{
    public class WfUtils
    {
        #region 生成Xoml
        public static string ToXoml(List<RuleLine> Rules, string WorkFlowName)
        {

            StringBuilder xml = new StringBuilder(@"<ns0:SMTStateMachineWorkflowActivity x:Name=""" + WorkFlowName + @"""  InitialStateName=""StartFlow""
              CompletedStateName=""EndFlow"" DynamicUpdateCondition=""{x:Null}"" xmlns:ns0=""clr-namespace:SMT.WFLib;Assembly=SMT.WFLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null""
              xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/workflow"">");


            List<RuleLine> UsedRule = new List<RuleLine>();
            for (int i = 0; i < Rules.Count; i++)
            {

                if (!UsedRule.Contains(Rules[i]))
                {
                    xml.Append(Environment.NewLine);
                    xml.Append(@"<StateActivity x:Name=""" + Rules[i].StrStartActive + @""">");
                    xml.Append(Environment.NewLine);
                    xml.Append(@"<EventDrivenActivity x:Name=""Eda" + Rules[i].StrStartActive + i.ToString() + @""">");
                    xml.Append(Environment.NewLine);
                    xml.Append(@"<ns0:SMTSubmitEvent ApproveInfo=""{x:Null}"" x:Name=""Event" + Rules[i].StrStartActive + i.ToString() + @""" EventName=""DoFlow"" InterfaceType=""{x:Type ns0:IFlowEvent}"" />");

                    //检索是否有跳转条件，返回集合大于1为有条件

                    List<RuleLine> tmpRule = Rules.Where(c => c.StrStartActive == Rules[i].StrStartActive).ToList();

                    #region 有跳转条件时
                    if (tmpRule.Count > 1)
                    {
                        xml.Append(Environment.NewLine);
                        xml.Append(@"<IfElseActivity x:Name=""Coditions" + Rules[i].StrStartActive + i.ToString() + @""">");
                        string tmpString = "";
                        for (int j = 0; j < tmpRule.Count; j++)
                        {
                            //默认跳转,默认跳转放在跳转条件节点最后
                            if (tmpRule[j].ruleCoditions==null || tmpRule[j].ruleCoditions.ConditionObject == null || tmpRule[j].ruleCoditions.subCondition.Count ==0)
                            {
                                tmpString = Environment.NewLine;
                                tmpString += @"<IfElseBranchActivity x:Name=""CompareCondition" + Rules[i].StrStartActive + i.ToString() + j.ToString() + @""">";
                                tmpString += @"<SetStateActivity x:Name=""Ts" + Rules[i].StrStartActive + i.ToString() + j.ToString() + @""" TargetStateName=""" + tmpRule[j].StrEndActive + @""" />";
                                tmpString += Environment.NewLine;
                                tmpString += @"</IfElseBranchActivity>";
                            }
                            //有条件
                            else
                            {
                                xml.Append(Environment.NewLine);
                                xml.Append(@"<IfElseBranchActivity x:Name=""CompareCondition" + Rules[i].StrStartActive + i.ToString() + j.ToString() + @""">");
                                xml.Append(Environment.NewLine);
                                xml.Append(@"<IfElseBranchActivity.Condition>");
                                xml.Append(Environment.NewLine);
                                xml.Append(@"<RuleConditionReference ConditionName=""" + tmpRule[j].ruleCoditions.Name + @"""/>");
                                xml.Append(Environment.NewLine);
                                xml.Append(@"</IfElseBranchActivity.Condition>");
                                xml.Append(Environment.NewLine);
                                xml.Append(@"<SetStateActivity x:Name=""Ts" + Rules[i].StrStartActive + i.ToString() + j.ToString() + @""" TargetStateName=""" + tmpRule[j].StrEndActive + @""" />");
                                xml.Append(Environment.NewLine);
                                xml.Append(@"</IfElseBranchActivity>");
                            }
                            //xml.Append(Environment.NewLine);
                            //xml.Append(@"<SetStateActivity x:Name=""Ts" + Rules[i].StrStartActive + i.ToString() + @""" TargetStateName=""" + tmpRule[j].StrEndActive + @""" />");



                            UsedRule.Add(tmpRule[j]);
                        }
                        if (tmpString != "")
                            xml.Append(tmpString);
                        xml.Append(Environment.NewLine);
                        xml.Append(@"</IfElseActivity>");
                    }

                    #endregion

                    #region 没有跳转条件时
                    else
                    {
                        xml.Append(Environment.NewLine);
                        xml.Append(@"<SetStateActivity x:Name=""Ts" + Rules[i].StrStartActive + i.ToString() + @""" TargetStateName=""" + Rules[i].StrEndActive + @""" />");
                        UsedRule.Add(Rules[i]);
                    }
                    #endregion

                    xml.Append(Environment.NewLine);
                    xml.Append(@"</EventDrivenActivity>");
                    xml.Append(Environment.NewLine);
                    xml.Append(@"</StateActivity>");


                }
            }

            xml.Append(Environment.NewLine);
            xml.Append(@"<StateActivity x:Name=""EndFlow"" />");
            xml.Append(Environment.NewLine);
            xml.Append(@"</ns0:SMTStateMachineWorkflowActivity>");
            return xml.ToString();

        }
        #endregion

        #region 生成Rule

        public static string ToRule(List<RuleLine> Rules,string SystemName)
        {
           // return RuleConst("==","OA","Test","Name","string","user2");

            List<RuleLine> RuleList = Rules.Where(c => c.ruleCoditions != null && c.ruleCoditions.ConditionObject != null && c.ruleCoditions.subCondition.Count > 0).ToList();
            
            if (RuleList.Count > 0)
            {
                StringBuilder xml = new StringBuilder(@"<RuleDefinitions xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/workflow"">");
                xml.Append(Environment.NewLine);
                xml.Append(@"<RuleDefinitions.Conditions>");

                for (int i = 0; i < RuleList.Count; i++)
                {
                    xml.Append(Environment.NewLine);
                    xml.Append(@"<RuleExpressionCondition Name=""" + RuleList[i].ruleCoditions.Name  + @""">");
                    xml.Append(Environment.NewLine);
                    xml.Append(@"<RuleExpressionCondition.Expression>");
     
                   string tmp= ProcessRule2(RuleList[i].ruleCoditions.subCondition,SystemName, RuleList[i].ruleCoditions.ConditionObject, RuleList[i].ruleCoditions.subCondition.Count -1);
                    xml.Append(Environment.NewLine);
                    xml.Append(tmp);
                    xml.Append(Environment.NewLine);
                    xml.Append(@"</RuleExpressionCondition.Expression>");
                    xml.Append(Environment.NewLine);
                    xml.Append(@"</RuleExpressionCondition>");

                }
                xml.Append(Environment.NewLine);
                xml.Append(@"</RuleDefinitions.Conditions>");
                xml.Append(Environment.NewLine);
                xml.Append(@"</RuleDefinitions>");
                return xml.ToString();
            }
            else
                return null;

        }
        


        private static string ProcessRule(List<CompareCondition> ComCondition, string Object)
        {
            string RuleXml = "", Operate="dddd";

            for (int i = 0; i < ComCondition.Count; i++)
            {
                if (ComCondition.Count == 1)
                    RuleXml = "<ns0:CodeBinaryOperatorExpression Operator=\"ValueEquality\" xmlns:ns0=\"clr-namespace:System.CodeDom;Assembly=System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\">";
                else if (ComCondition.Count == i)
                    RuleXml = "<ns0:CodeBinaryOperatorExpression Operator=\"BooleanAnd\" xmlns:ns0=\"clr-namespace:System.CodeDom;Assembly=System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\">";
                else
                {
                    RuleXml = "<ns0:CodeBinaryOperatorExpression." + (i % 2 == 0 ? "Right" : "Left") + ">";
                    RuleXml += Environment.NewLine + "<ns0:CodeBinaryOperatorExpression Operator=\"" + ComCondition[i].Operate + "\">";
                  //  RuleXml += Environment.NewLine + ProcessRule(ComCondition, Object, i - 1);
                    RuleXml += Environment.NewLine + RuleConst("", ComCondition[i].Name, Object, ComCondition[i].CompAttr,ComCondition[i].DataType , ComCondition[i].CompareValue,false );
                    RuleXml = Environment.NewLine + "</ns0:CodeBinaryOperatorExpression>";
                    RuleXml = "</ns0:CodeBinaryOperatorExpression." + (i % 2 == 0 ? "Right" : "Left") + ">";
                }
            }
            return RuleXml;
        }

        /// <summary>
        /// 构成规则结构
        /// </summary>
        /// <param name="ComCondition"></param>
        /// <param name="SystemName"></param>
        /// <param name="Object"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private static string ProcessRule2(List<CompareCondition> ComCondition,string SystemName, string Object,int i)
        {
            string RuleXml = "";


            if (ComCondition.Count == 1)
            {
                RuleXml = RuleConst(ComCondition[i].Operate, SystemName, Object, ComCondition[i].CompAttr, ComCondition[i].DataType, ComCondition[i].CompareValue, true); 
            }
            else if (i == 0)
            {
                RuleXml = RuleConst(ComCondition[i].Operate, SystemName, Object, ComCondition[i].CompAttr, ComCondition[i].DataType, ComCondition[i].CompareValue, false);
            }
            else
            {
                if (ComCondition.Count - 1 == i)
                RuleXml = "<ns0:CodeBinaryOperatorExpression Operator=\"BooleanAnd\" xmlns:ns0=\"clr-namespace:System.CodeDom;Assembly=System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\">";
                else
                RuleXml = Environment.NewLine + "<ns0:CodeBinaryOperatorExpression Operator=\"BooleanAnd\">";

                RuleXml += "<ns0:CodeBinaryOperatorExpression.Left>";
                RuleXml += ProcessRule2(ComCondition,SystemName, Object, i - 1);
                RuleXml += "</ns0:CodeBinaryOperatorExpression.Left>";

                RuleXml += "<ns0:CodeBinaryOperatorExpression.Right>";
                RuleXml += RuleConst(ComCondition[i].Operate, SystemName, Object, ComCondition[i].CompAttr, ComCondition[i].DataType, ComCondition[i].CompareValue, false);
                RuleXml += "</ns0:CodeBinaryOperatorExpression.Right>";
                RuleXml += "</ns0:CodeBinaryOperatorExpression>";
            }
          
            return RuleXml;
        }

        /// <summary>
        /// 生成对比节点
        /// </summary>
        /// <param name="Operate"></param>
        /// <param name="SystemName"></param>
        /// <param name="Object"></param>
        /// <param name="Attribute"></param>
        /// <param name="DataType"></param>
        /// <param name="DataValue"></param>
        /// <param name="IsOne"></param>
        /// <returns></returns>
        private static string RuleConst(string Operate,string SystemName,string Object,string Attribute, string DataType, string DataValue,Boolean IsOne)
        {

            string GetData = "",strDataType="";

            switch (Operate)
            {
                case "==":
                    {
                        Operate = "ValueEquality";
                        break;
                    }
                case ">":
                    {
                        Operate = "GreaterThan";
                        break;
                    }
                case ">=":
                    {
                        Operate = "GreaterThanOrEqual";
                        break;
                    }
                case "<":
                    {
                        Operate = "LessThan";
                        break;
                    }
                case "<=":
                    {
                        Operate = "LessThanOrEqual"; 
                        break;
                    }
                default:
                    {
                        Operate = null;
                        break;
                    }
            }


            switch (DataType)
            {
                case "string":
                    {
                        GetData = "GetString";
                        strDataType = "String";
                        break;
                    }
                case "decimal":
                    {
                        GetData = "GetDecimal";
                        strDataType = "Int32";
                        break;
                    }
                case "datetime":
                    {
                        GetData = "GetString"; //日期型暂以字符型处理
                        strDataType = "String";
                        break;
                    }
                default:
                    {
                        GetData = null;
                        strDataType = null;
                        break;
                    }
            }

            string RuleConst ="";

            if(IsOne)
             RuleConst = Environment.NewLine + "<ns0:CodeBinaryOperatorExpression Operator=\"" + Operate + "\" xmlns:ns0=\"clr-namespace:System.CodeDom;Assembly=System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\">";
            else
             RuleConst = Environment.NewLine + "<ns0:CodeBinaryOperatorExpression Operator=\"" + Operate + "\">";

            RuleConst += Environment.NewLine + "<ns0:CodeBinaryOperatorExpression.Left>";
            RuleConst += Environment.NewLine + "<ns0:CodeMethodInvokeExpression>" ;
            RuleConst += Environment.NewLine + "<ns0:CodeMethodInvokeExpression.Parameters>";
            RuleConst += Environment.NewLine + "<ns0:CodeFieldReferenceExpression FieldName=\"xml\">";
            RuleConst += Environment.NewLine + "<ns0:CodeFieldReferenceExpression.TargetObject>";
            RuleConst += Environment.NewLine + "<ns0:CodePropertyReferenceExpression PropertyName=\"FlowData\">";
            RuleConst += Environment.NewLine + "<ns0:CodePropertyReferenceExpression.TargetObject>";
            RuleConst += Environment.NewLine + "<ns0:CodeThisReferenceExpression />";
            RuleConst += Environment.NewLine + "</ns0:CodePropertyReferenceExpression.TargetObject>";
            RuleConst += Environment.NewLine + "</ns0:CodePropertyReferenceExpression>";
            RuleConst += Environment.NewLine + "</ns0:CodeFieldReferenceExpression.TargetObject>";
            RuleConst += Environment.NewLine + "</ns0:CodeFieldReferenceExpression>";
            RuleConst += Environment.NewLine + "<ns0:CodePrimitiveExpression>";
            RuleConst += Environment.NewLine + "<ns0:CodePrimitiveExpression.Value>";
            RuleConst += Environment.NewLine + "<ns1:String xmlns:ns1=\"clr-namespace:System;Assembly=mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\">" + SystemName  + "</ns1:String>";
            RuleConst += Environment.NewLine + "</ns0:CodePrimitiveExpression.Value>";
            RuleConst += Environment.NewLine + "</ns0:CodePrimitiveExpression>";
            RuleConst += Environment.NewLine + "<ns0:CodePrimitiveExpression>";
            RuleConst += Environment.NewLine + "<ns0:CodePrimitiveExpression.Value>";
            RuleConst += Environment.NewLine +"<ns1:String xmlns:ns1=\"clr-namespace:System;Assembly=mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\">"+ Object +"</ns1:String>";
            RuleConst += Environment.NewLine + "</ns0:CodePrimitiveExpression.Value>";
            RuleConst += Environment.NewLine + "</ns0:CodePrimitiveExpression>";
            RuleConst += Environment.NewLine + "<ns0:CodePrimitiveExpression>";
            RuleConst += Environment.NewLine + "<ns0:CodePrimitiveExpression.Value>";
            RuleConst += Environment.NewLine + "<ns1:String xmlns:ns1=\"clr-namespace:System;Assembly=mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\">"+ Attribute +"</ns1:String>";
            RuleConst += Environment.NewLine + "</ns0:CodePrimitiveExpression.Value>";
            RuleConst += Environment.NewLine + " </ns0:CodePrimitiveExpression>";
            RuleConst += Environment.NewLine + "</ns0:CodeMethodInvokeExpression.Parameters>";
            RuleConst += Environment.NewLine + "<ns0:CodeMethodInvokeExpression.Method>";
            RuleConst += Environment.NewLine + "<ns0:CodeMethodReferenceExpression MethodName=\"" + GetData + "\">";
            RuleConst += Environment.NewLine + "<ns0:CodeMethodReferenceExpression.TargetObject>";
            RuleConst += Environment.NewLine +"<ns0:CodePropertyReferenceExpression PropertyName=\"FlowData\">";
            RuleConst += Environment.NewLine + "<ns0:CodePropertyReferenceExpression.TargetObject>";
            RuleConst += Environment.NewLine + "<ns0:CodeThisReferenceExpression />";
            RuleConst += Environment.NewLine + "</ns0:CodePropertyReferenceExpression.TargetObject>";
            RuleConst += Environment.NewLine + "</ns0:CodePropertyReferenceExpression>";
            RuleConst += Environment.NewLine + "</ns0:CodeMethodReferenceExpression.TargetObject>";
            RuleConst += Environment.NewLine + "</ns0:CodeMethodReferenceExpression>";
            RuleConst += Environment.NewLine + "</ns0:CodeMethodInvokeExpression.Method>";
            RuleConst += Environment.NewLine + "</ns0:CodeMethodInvokeExpression>";
            RuleConst += Environment.NewLine + "</ns0:CodeBinaryOperatorExpression.Left>";
            RuleConst += Environment.NewLine + "<ns0:CodeBinaryOperatorExpression.Right>";
            RuleConst += Environment.NewLine + "<ns0:CodePrimitiveExpression>";
            RuleConst += Environment.NewLine + "<ns0:CodePrimitiveExpression.Value>";
            RuleConst += Environment.NewLine + "<ns1:" + strDataType + " xmlns:ns1=\"clr-namespace:System;Assembly=mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\">" + DataValue + "</ns1:" + strDataType + ">";
            RuleConst += Environment.NewLine +"</ns0:CodePrimitiveExpression.Value>";
            RuleConst += Environment.NewLine +"</ns0:CodePrimitiveExpression>";
            RuleConst += Environment.NewLine +"</ns0:CodeBinaryOperatorExpression.Right>";
            RuleConst += Environment.NewLine +"</ns0:CodeBinaryOperatorExpression>";

            return RuleConst;
        }
      
        #endregion
    }
        
}
