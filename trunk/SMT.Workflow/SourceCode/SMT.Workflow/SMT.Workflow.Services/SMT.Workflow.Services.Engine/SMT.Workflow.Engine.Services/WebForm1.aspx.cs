using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SMT.Workflow.Engine.Services.BLL.SMTEngine;

namespace SMT.Workflow.Engine.Services
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            EngineWcfGlobalFunction dd = new EngineWcfGlobalFunction();
            string xml=@"<System>
  <Name>VM</Name>
  <Version>1.0</Version>
  <System>
    <Function Description="""" Address="""" FuncName="""" Binding="""" SplitChar=""Г"">
      <ParaStr>
        <Para TableName="""" Name="""" Description="""" Value="""" />
      </ParaStr>
    </Function>
  </System>
  <MsgOpen>
    <AssemblyName>SMT.VM..UI</AssemblyName>
    <PublicClass>SMT.VM.UI.Class.Utility</PublicClass>
    <ProcessName>CreateFormFromEngine</ProcessName>
    <PageParameter>VM/VehicleOilCPayCfm/Details</PageParameter>
    <ApplicationOrder>{PAYCFMID}</ApplicationOrder>
    <FormTypes>Details</FormTypes>
  </MsgOpen>
  <Object Name=""T_VM_VEHICLEOILCPAYCFM"" Description=""车辆油卡充值确认表"" Key=""PAYCFMID"" id=""05cd6f06-4e25-4590-966f-31e0b1e94cbc"">
    <Attribute Name=""PAYCFMID"" LableResourceID=""PAYCFMID"" Description=""充值确认主键ID"" DataType=""string"" DataValue=""05cd6f06-4e25-4590-966f-31e0b1e94cbc"" DataText=""05cd6f06-4e25-4590-966f-31e0b1e94cbc"" />
    <Attribute Name=""PAYAPPLYID"" LableResourceID=""PAYAPPLYID"" Description=""借款申请主键ID"" DataType=""string"" DataValue=""05a86d41-38fa-499a-8973-d2b1877a2c70"" DataText=""05a86d41-38fa-499a-8973-d2b1877a2c70"" />
    <Attribute Name=""OILCARDID"" LableResourceID=""OILCARDID"" Description=""加油卡主键ID"" DataType=""string"" DataValue=""6aeebb1f-8024-45d5-b3ae-d4725aa803a9"" DataText=""6aeebb1f-8024-45d5-b3ae-d4725aa803a9"" />
    <Attribute Name=""OILCARDNO"" LableResourceID=""OILCARDNO"" Description=""加油卡编号"" DataType=""string"" DataValue=""VM_20130318145948000003"" DataText=""VM_20130318145948000003"" />
    <Attribute Name=""VEHICLEID"" LableResourceID=""VEHICLEID"" Description=""车辆主键ID"" DataType=""string"" DataValue=""540e6a2c-8030-4bb3-97d4-63dd80c9db5d"" DataText=""540e6a2c-8030-4bb3-97d4-63dd80c9db5d"" />
    <Attribute Name=""MARKNO"" LableResourceID=""MARKNO"" Description=""车牌号"" DataType=""string"" DataValue=""粤B53B33"" DataText=""粤B53B33"" />
    <Attribute Name=""OILBALANCE"" LableResourceID=""OILBALANCE"" Description=""油卡余额"" DataType=""decimal"" DataValue=""0"" DataText=""0"" />
    <Attribute Name=""PAYMODE"" LableResourceID=""PAYMODE"" Description=""充值方式"" DataType=""string"" DataValue=""2"" DataText=""2"" />
    <Attribute Name=""PAYCOST"" LableResourceID=""PAYCOST"" Description="""" DataType=""decimal"" DataValue=""3466"" DataText=""3466"" />
    <Attribute Name=""PAYBALANCE"" LableResourceID=""PAYBALANCE"" Description=""充值后余额"" DataType=""decimal"" DataValue=""0"" DataText=""0"" />
    <Attribute Name=""EFFECTDATE"" LableResourceID=""EFFECTDATE"" Description=""充值日期"" DataType=""datetime"" DataValue=""2013/3/28 15:58:01"" DataText=""2013/3/28 15:58:01"" />
    <Attribute Name=""REFUNDID"" LableResourceID=""REFUNDID"" Description=""报销单主键ID"" DataType=""string"" DataValue="""" DataText="""" />
    <Attribute Name=""REFUNDNO"" LableResourceID=""REFUNDNO"" Description=""报销单号"" DataType=""string"" DataValue="""" DataText="""" />
    <Attribute Name=""PAYITEMID"" LableResourceID=""PAYITEMID"" Description=""油卡充值科目ID"" DataType=""string"" DataValue="""" DataText="""" />
    <Attribute Name=""PAYITEMNAME"" LableResourceID=""PAYITEMNAME"" Description=""油卡充值科目名称"" DataType=""string"" DataValue="""" DataText="""" />
    <Attribute Name=""LOANID"" LableResourceID=""LOANID"" Description=""充值借款单主键ID"" DataType=""string"" DataValue="""" DataText="""" />
    <Attribute Name=""LOANNO"" LableResourceID=""LOANNO"" Description=""充值借款单号"" DataType=""string"" DataValue="""" DataText="""" />
    <Attribute Name=""EDITSTATE"" LableResourceID=""EDITSTATE"" Description=""对象编辑状态"" DataValue="""" DataText="""" />
    <Attribute Name=""CHECKSTATES"" LableResourceID=""CHECKSTATES"" Description=""审核状态"" DataType=""string"" DataValue="""" DataText="""" />
    <Attribute Name=""OWNERCOMPANYID"" LableResourceID=""OWNERCOMPANYID"" Description=""记录所属公司ID"" DataType=""string"" DataValue=""c34d0840-47f9-4450-9197-3a49227eef22"" DataText=""c34d0840-47f9-4450-9197-3a49227eef22"" />
    <Attribute Name=""OWNERDEPARTMENTID"" LableResourceID=""OWNERDEPARTMENTID"" Description=""记录所属部门ID"" DataType=""string"" DataValue="""" DataText="""" />
    <Attribute Name=""OWNERPOSTID"" LableResourceID=""OWNERPOSTID"" Description=""记录所属岗位ID"" DataType=""string"" DataValue="""" DataText="""" />
    <Attribute Name=""OWNERID"" LableResourceID=""OWNERID"" Description=""记录所属用户ID"" DataType=""string"" DataValue="""" DataText="""" />
    <Attribute Name=""COMPANYNAME"" LableResourceID=""COMPANYNAME"" Description=""使用公司名称"" DataType=""string"" DataValue=""神州通车辆管理公司"" DataText=""神州通车辆管理公司"" />
    <Attribute Name=""DEPARTMENTNAME"" LableResourceID=""DEPARTMENTNAME"" Description=""部门名称"" DataType=""string"" DataValue="""" DataText="""" />
    <Attribute Name=""EMPLOYEENAME"" LableResourceID=""EMPLOYEENAME"" Description=""员工名称"" DataType=""string"" DataValue="""" DataText="""" />
    <Attribute Name=""POSTNAME"" LableResourceID=""POSTNAME"" Description=""岗位名称"" DataType=""string"" DataValue="""" DataText="""" />
    <Attribute Name=""CREATECOMPANYID"" LableResourceID=""CREATECOMPANYID"" Description=""创建公司ID"" DataType=""string"" DataValue="""" DataText="""" />
    <Attribute Name=""CREATEDEPARTMENTID"" LableResourceID=""CREATEDEPARTMENTID"" Description=""创建部门ID"" DataType=""string"" DataValue="""" DataText="""" />
    <Attribute Name=""CREATEPOSTID"" LableResourceID=""CREATEPOSTID"" Description=""创建岗位ID"" DataType=""string"" DataValue="""" DataText="""" />
    <Attribute Name=""CREATEUSERNAME"" LableResourceID=""CREATEUSERNAME"" Description=""创建人名"" DataType=""string"" DataValue="""" DataText="""" />
    <Attribute Name=""CREATEUSERID"" LableResourceID=""CREATEUSERID"" Description=""创建人ID"" DataType=""string"" DataValue="""" DataText="""" />
    <Attribute Name=""CREATEDATE"" LableResourceID=""CREATEDATE"" Description=""创建时间"" DataType=""datetime"" DataValue=""0001/1/1 0:00:00"" DataText=""0001/1/1 0:00:00"" />
    <Attribute Name=""UPDATEUSERID"" LableResourceID=""UPDATEUSERID"" Description=""修改人ID"" DataType=""string"" DataValue="""" DataText="""" />
    <Attribute Name=""UPDATEDATE"" LableResourceID=""UPDATEDATE"" Description=""修改时间"" DataType=""datetime"" DataValue=""0001/1/1 0:00:00"" DataText=""0001/1/1 0:00:00"" />
    <Attribute Name=""REMARK"" LableResourceID=""REMARK"" Description=""备注"" DataType=""string"" DataValue="""" DataText="""" />
  </Object>
</System>";
            xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<System>
  <Name>HR</Name>
  <Version>1.0</Version>
  <System>
    <Function Description=""EmployeesalaryrecordFlow"" Address=""EngineService.svc"" FuncName=""CallApplicationService"" Binding=""customBinding"" SplitChar=""Г"">
      <ParaStr>
        <Para TableName=""T_HR_SALARYRECORDBATCH"" Name=""MONTHLYBATCHID"" Description=""月薪批量审核ID"" Value=""""></Para>
      </ParaStr>
    </Function>
    <Function Description=""薪资发放"" Address=""EngineService.svc"" FuncName=""CallWaitAppService"" Binding=""customBinding"" SplitChar=""Г"">
      <ParaStr>
        <Para TableName=""T_HR_EMPLOYEESALARYRECORDPAYMENT"" Name=""EMPLOYEEID"" Description=""员工ID"" Value=""""></Para>
      </ParaStr>
    </Function>
  </System>
  <MsgOpen>
    <AssemblyName>SMT.HRM.UI</AssemblyName>
    <PublicClass>SMT.HRM.UI.Utility</PublicClass>
    <ProcessName>CreateFormFromEngine</ProcessName>
    <PageParameter>SMT.HRM.UI.Form.Salary.SalaryRecordMassAudit</PageParameter>
    <ApplicationOrder>{MONTHLYBATCHID}</ApplicationOrder>
    <FormTypes>Audit</FormTypes>
  </MsgOpen>
  <Object Name=""T_HR_SALARYRECORDBATCH"" Description=""月薪批量审核"" Key=""MONTHLYBATCHID"" id=""6b1618dc-79aa-485e-b623-60438eec49a2"">
    <Attribute Name=""CURRENTEMPLOYEENAME"" LableResourceID=""CURRENTEMPLOYEENAME"" Description=""提交者"" DataType=""string"" DataValue="""" DataText=""""></Attribute>
    <Attribute Name=""MONTHLYBATCHID"" LableResourceID=""MONTHLYBATCHID"" Description=""MONTHLYBATCHID"" DataType=""string"" DataValue=""6b1618dc-79aa-485e-b623-60438eec49a2"" DataText=""""></Attribute>
    <Attribute Name=""SALARYYEAR"" LableResourceID=""BALANCEYEAR"" Description=""结算年份"" DataType=""decimal"" DataValue=""2011"" DataText=""""></Attribute>
    <Attribute Name=""SALARYMONTH"" LableResourceID=""BALANCEMONTH"" Description=""结算月份"" DataType=""decimal"" DataValue=""12"" DataText=""""></Attribute>
    <Attribute Name=""PAYDATE"" LableResourceID=""PAYDATE"" Description=""发薪年月"" DataType=""string"" DataValue=""2011年12月"" DataText=""""></Attribute>
    <Attribute Name=""BALANCEDATE"" LableResourceID=""BALANCEDATE"" Description=""结算日期"" DataType=""DateTime"" DataValue="""" DataText=""""></Attribute>
    <Attribute Name=""PAYTOTALMONEY"" LableResourceID=""PAYTOTALMONEY"" Description=""发薪总额"" DataType=""decimal"" DataValue=""175570"" DataText=""""></Attribute>
    <Attribute Name=""PAYAVERAGE"" LableResourceID=""PAYAVERAGE"" Description=""人均薪水"" DataType=""decimal"" DataValue=""9240.53"" DataText=""""></Attribute>
    <Attribute Name=""BALANCEOBJECTTYPE"" LableResourceID=""BALANCEOBJECTTYPE"" Description=""BALANCEOBJECTTYPE"" DataType=""string"" DataValue=""0"" DataText=""""></Attribute>
    <Attribute Name=""BALANCEOBJECTID"" LableResourceID=""PAYOBJECT"" Description=""发放对象"" DataType=""string"" DataValue=""4da803fc-08ad-426b-82e2-f25f3192f438"" DataText=""深圳市神州通地产置业有限公司""></Attribute>
    <Attribute Name=""BALANCEOBJECTNAME"" LableResourceID=""BALANCEOBJECTNAME"" Description=""BALANCEOBJECTNAME"" DataType=""string"" DataValue=""0"" DataText=""""></Attribute>
    <Attribute Name=""CHECKSTATE"" LableResourceID=""CHECKSTATE"" Description=""审批状态"" DataType=""string"" DataValue=""1"" DataText=""审核中""></Attribute>
    <Attribute Name=""EDITSTATE"" LableResourceID=""EDITSTATE"" Description=""状态"" DataType=""string"" DataValue=""0"" DataText=""""></Attribute>
    <Attribute Name=""OWNERCOMPANYID"" LableResourceID=""OWNERCOMPANYID"" Description=""所属公司"" DataType=""string"" DataValue=""4da803fc-08ad-426b-82e2-f25f3192f438"" DataText=""""></Attribute>
    <Attribute Name=""OWNERDEPARTMENTID"" LableResourceID=""OWNERDEPARTMENTID"" Description=""所属部门"" DataType=""string"" DataValue=""97933a10-99e3-40d7-8c60-12aed2a6b331"" DataText=""""></Attribute>
    <Attribute Name=""OWNERPOSTID"" LableResourceID=""OWNERPOSTID"" Description=""所属岗位"" DataType=""string"" DataValue=""0e30fea9-085c-4be3-8c4e-fc9512dc122e"" DataText=""""></Attribute>
    <Attribute Name=""OWNERID"" LableResourceID=""OWNERID"" Description=""所属员工"" DataType=""string"" DataValue=""5caac067-a8ad-4084-836e-db6806ffd0fe"" DataText=""""></Attribute>
    <Attribute Name=""CREATEPOSTID"" LableResourceID=""CREATEPOSTID"" Description=""创建人岗位"" DataType=""string"" DataValue=""0e30fea9-085c-4be3-8c4e-fc9512dc122e"" DataText=""""></Attribute>
    <Attribute Name=""CREATEDEPARTMENTID"" LableResourceID=""CREATEDEPARTMENTID"" Description=""创建人部门"" DataType=""string"" DataValue=""97933a10-99e3-40d7-8c60-12aed2a6b331"" DataText=""""></Attribute>
    <Attribute Name=""CREATECOMPANYID"" LableResourceID=""CREATECOMPANYID"" Description=""创建公司"" DataType=""string"" DataValue=""4da803fc-08ad-426b-82e2-f25f3192f438"" DataText=""""></Attribute>
    <Attribute Name=""CREATEUSERID"" LableResourceID=""CREATEUSERID"" Description=""创建人"" DataType=""string"" DataValue=""5caac067-a8ad-4084-836e-db6806ffd0fe"" DataText=""""></Attribute>
    <Attribute Name=""CREATEDATE"" LableResourceID=""CREATEDATE"" Description=""创建日期"" DataType=""DateTime"" DataValue=""2012/1/11 16:29:23"" DataText=""""></Attribute>
    <Attribute Name=""REMARK"" LableResourceID=""REMARK"" Description=""备注"" DataType=""string"" DataValue="""" DataText=""""></Attribute>
    <Attribute Name=""UPDATEUSERID"" LableResourceID=""UPDATEUSERID"" Description=""修改人"" DataType=""string"" DataValue="""" DataText=""""></Attribute>
    <Attribute Name=""UPDATEDATE"" LableResourceID=""UPDATEDATE"" Description=""修改时间"" DataType=""DateTime"" DataValue="""" DataText=""""></Attribute>
    <Attribute Name=""ACTUALLYPAY"" LableResourceID=""ACTUALLYPAY""  Description=""实发金额"" DataType=""string""  DataValue=""175570"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM6"" LableResourceID=""SALARYITEM6""  Description=""应发小计"" DataType=""string""  DataValue=""200776"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM7"" LableResourceID=""SALARYITEM7""  Description=""基本工资"" DataType=""string""  DataValue=""60179.00"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM8"" LableResourceID=""SALARYITEM8""  Description=""岗位工资"" DataType=""string""  DataValue=""51582"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM9"" LableResourceID=""SALARYITEM9""  Description=""保密津贴"" DataType=""string""  DataValue=""17194"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM10"" LableResourceID=""SALARYITEM10""  Description=""住房补贴"" DataType=""string""  DataValue=""42985.0"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM11"" LableResourceID=""SALARYITEM11""  Description=""地区差异补贴"" DataType=""string""  DataValue=""34800"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM12"" LableResourceID=""SALARYITEM12""  Description=""餐费补贴"" DataType=""string""  DataValue=""1900"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM13"" LableResourceID=""SALARYITEM13""  Description=""固定收入合计"" DataType=""string""  DataValue=""208640"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM14"" LableResourceID=""SALARYITEM14""  Description=""缺勤天数"" DataType=""string""  DataValue=""2.75"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM15"" LableResourceID=""SALARYITEM15""  Description=""加班费"" DataType=""string""  DataValue=""0"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM16"" LableResourceID=""SALARYITEM16""  Description=""值班津贴"" DataType=""string""  DataValue=""0"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM17"" LableResourceID=""SALARYITEM17""  Description=""出勤工资"" DataType=""string""  DataValue=""200776"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM18"" LableResourceID=""SALARYITEM18""  Description=""住房公积金扣款"" DataType=""string""  DataValue=""1985.6"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM19"" LableResourceID=""SALARYITEM19""  Description=""个人社保负担"" DataType=""string""  DataValue=""4402.70"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM20"" LableResourceID=""SALARYITEM20""  Description=""税前应发合计"" DataType=""string""  DataValue=""200776"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM21"" LableResourceID=""SALARYITEM21""  Description=""假期其它扣款"" DataType=""string""  DataValue=""-6720"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM22"" LableResourceID=""SALARYITEM22""  Description=""其它加扣款"" DataType=""string""  DataValue=""0"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM23"" LableResourceID=""SALARYITEM23""  Description=""绩效奖金"" DataType=""string""  DataValue=""0"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM25"" LableResourceID=""SALARYITEM25""  Description=""计税工资"" DataType=""string""  DataValue=""194387.70"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM27"" LableResourceID=""SALARYITEM27""  Description=""差额"" DataType=""string""  DataValue=""127887.70"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM29"" LableResourceID=""SALARYITEM29""  Description=""速算扣除数"" DataType=""string""  DataValue=""8535"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM30"" LableResourceID=""SALARYITEM30""  Description=""个人所得税"" DataType=""string""  DataValue=""18240.57"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM31"" LableResourceID=""SALARYITEM31""  Description=""考勤异常扣款"" DataType=""string""  DataValue=""200"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM32"" LableResourceID=""SALARYITEM32""  Description=""其它代扣款"" DataType=""string""  DataValue=""-365.73"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM33"" LableResourceID=""SALARYITEM33""  Description=""尾数扣款"" DataType=""string""  DataValue=""11.40"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM34"" LableResourceID=""SALARYITEM34""  Description=""扣款合计"" DataType=""string""  DataValue=""18817.70"" IsEncryption=""False""  DataText="""" /> 

    <ObjectList Name=""SALARYRECORDBATCHList"" LableResourceID=""SALARYRECORDBATCHList"" Description=""月薪批量审核"" DataText="""">
      <Object Name=""SALARYRECORDBATCHDETAIL"" LableResourceID=""SALARYRECORDBATCHDETAIL"" Description=""月薪批量审核"" Key=""EMPLOYEESALARYRECORDID"" id=""149ce168-6247-46a2-b215-9a189e7b0b1d"">
<Attribute Name=""DEPARTMENT"" LableResourceID=""DEPARTMENT""  Description=""部门"" DataType=""string""  DataValue=""成本管理部"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POST"" LableResourceID=""POST""  Description=""工作岗位"" DataType=""string""  DataValue=""土建造价工程师"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""EMPLOYEENAME"" LableResourceID=""EMPLOYEENAME""  Description=""姓名"" DataType=""string""  DataValue=""张锦涛"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POSTCODE"" LableResourceID=""POSTCODE""  Description=""职级代码"" DataType=""string""  DataValue=""土建造价工程师"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""ACTUALLYPAY"" LableResourceID=""ACTUALLYPAY""  Description=""实发金额"" DataType=""string""  DataValue=""PdxPx5p7/osOJgRGraJYgQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""POSTLEVEL"" LableResourceID=""POSTLEVEL""  Description=""岗位级别"" DataType=""string""  DataValue=""13"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM6"" LableResourceID=""SALARYITEM6""  Description=""应发小计"" DataType=""string""  DataValue=""IjFQjJfUXIUKV39uSREY7g=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM7"" LableResourceID=""SALARYITEM7""  Description=""基本工资"" DataType=""string""  DataValue=""eyB+/a4lGSShbOS/0S/8rQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM8"" LableResourceID=""SALARYITEM8""  Description=""岗位工资"" DataType=""string""  DataValue=""dDZOO8Kc17F34bZvafQ/gA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM9"" LableResourceID=""SALARYITEM9""  Description=""保密津贴"" DataType=""string""  DataValue=""xDQ8Gtglz5cOu7ZO5bKpUQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM10"" LableResourceID=""SALARYITEM10""  Description=""住房补贴"" DataType=""string""  DataValue=""sdpfGGpB9N53ur53EWb6xg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM11"" LableResourceID=""SALARYITEM11""  Description=""地区差异补贴"" DataType=""string""  DataValue=""JHrsKqlqOUXZcTNwxbeUNg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM12"" LableResourceID=""SALARYITEM12""  Description=""餐费补贴"" DataType=""string""  DataValue=""lnMNsa2ofXzqoaNFenTZ4w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM13"" LableResourceID=""SALARYITEM13""  Description=""固定收入合计"" DataType=""string""  DataValue=""IjFQjJfUXIUKV39uSREY7g=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM14"" LableResourceID=""SALARYITEM14""  Description=""缺勤天数"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM15"" LableResourceID=""SALARYITEM15""  Description=""加班费"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM16"" LableResourceID=""SALARYITEM16""  Description=""值班津贴"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM17"" LableResourceID=""SALARYITEM17""  Description=""出勤工资"" DataType=""string""  DataValue=""IjFQjJfUXIUKV39uSREY7g=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM18"" LableResourceID=""SALARYITEM18""  Description=""住房公积金扣款"" DataType=""string""  DataValue=""76kJBJMyBdcuvlX2c3Efwg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM19"" LableResourceID=""SALARYITEM19""  Description=""个人社保负担"" DataType=""string""  DataValue=""SHo4k5JPx23Cgk8j1lxmoA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM20"" LableResourceID=""SALARYITEM20""  Description=""税前应发合计"" DataType=""string""  DataValue=""IjFQjJfUXIUKV39uSREY7g=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM21"" LableResourceID=""SALARYITEM21""  Description=""假期其它扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM22"" LableResourceID=""SALARYITEM22""  Description=""其它加扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM23"" LableResourceID=""SALARYITEM23""  Description=""绩效奖金"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM24"" LableResourceID=""SALARYITEM24""  Description=""纳税系数"" DataType=""string""  DataValue=""LWo3qQqyopqpUN6MfuaRqg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM25"" LableResourceID=""SALARYITEM25""  Description=""计税工资"" DataType=""string""  DataValue=""WKlR1oM01h9ndZXQKLjHuA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM26"" LableResourceID=""SALARYITEM26""  Description=""扣税基数"" DataType=""string""  DataValue=""mcnLb8ttbSi7fY6sdBo7nw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM27"" LableResourceID=""SALARYITEM27""  Description=""差额"" DataType=""string""  DataValue=""huUCXFp+EGfd7dk5La63Nw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM28"" LableResourceID=""SALARYITEM28""  Description=""税率"" DataType=""string""  DataValue=""AzPp4W+AxGPty4kBmjd45w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM29"" LableResourceID=""SALARYITEM29""  Description=""速算扣除数"" DataType=""string""  DataValue=""Njt11fXCSEDT/RKlpg5U9g=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM30"" LableResourceID=""SALARYITEM30""  Description=""个人所得税"" DataType=""string""  DataValue=""0kOPlxX65aTDhFsRpET7bA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM31"" LableResourceID=""SALARYITEM31""  Description=""考勤异常扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM32"" LableResourceID=""SALARYITEM32""  Description=""其它代扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM33"" LableResourceID=""SALARYITEM33""  Description=""尾数扣款"" DataType=""string""  DataValue=""4ETeARo6LWQZnlro0nkK2A=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM34"" LableResourceID=""SALARYITEM34""  Description=""扣款合计"" DataType=""string""  DataValue=""I2bFC4B3++hPq4TvO/Ov7A=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""REMARK"" LableResourceID=""REMARK""  Description=""备注"" DataType=""string""  DataValue=""8RYoJafePIrVJmsp3CH1cA=="" IsEncryption=""True""  DataText="""" /> 
</Object><Object Name=""SALARYRECORDBATCHDETAIL"" LableResourceID=""SALARYRECORDBATCHDETAIL"" Description=""月薪批量审核"" Key=""EMPLOYEESALARYRECORDID"" id=""178ef9ed-5cbb-47fc-b92a-83ece604021a"">
<Attribute Name=""DEPARTMENT"" LableResourceID=""DEPARTMENT""  Description=""部门"" DataType=""string""  DataValue=""营销管理部"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POST"" LableResourceID=""POST""  Description=""工作岗位"" DataType=""string""  DataValue=""营销经理"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""EMPLOYEENAME"" LableResourceID=""EMPLOYEENAME""  Description=""姓名"" DataType=""string""  DataValue=""邹锦辉"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POSTCODE"" LableResourceID=""POSTCODE""  Description=""职级代码"" DataType=""string""  DataValue=""营销经理"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""ACTUALLYPAY"" LableResourceID=""ACTUALLYPAY""  Description=""实发金额"" DataType=""string""  DataValue=""58JJBwOKe8XmzKX4IeKdOQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""POSTLEVEL"" LableResourceID=""POSTLEVEL""  Description=""岗位级别"" DataType=""string""  DataValue=""14"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM6"" LableResourceID=""SALARYITEM6""  Description=""应发小计"" DataType=""string""  DataValue=""anTrUit5SIxe1/PD5Z9CBQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM7"" LableResourceID=""SALARYITEM7""  Description=""基本工资"" DataType=""string""  DataValue=""0nqOtpB6YSdtawEEbnknMA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM8"" LableResourceID=""SALARYITEM8""  Description=""岗位工资"" DataType=""string""  DataValue=""sejHKXMQXrYka/scZ+D3xQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM9"" LableResourceID=""SALARYITEM9""  Description=""保密津贴"" DataType=""string""  DataValue=""2nmvmCXlTDI/e2JBocktqg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM10"" LableResourceID=""SALARYITEM10""  Description=""住房补贴"" DataType=""string""  DataValue=""d+GqOfRcsvifSWQsRC7Mvw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM11"" LableResourceID=""SALARYITEM11""  Description=""地区差异补贴"" DataType=""string""  DataValue=""Rl4d+Pm6WwyGycLt8Ppnjg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM12"" LableResourceID=""SALARYITEM12""  Description=""餐费补贴"" DataType=""string""  DataValue=""lnMNsa2ofXzqoaNFenTZ4w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM13"" LableResourceID=""SALARYITEM13""  Description=""固定收入合计"" DataType=""string""  DataValue=""j+TKrUGdPrqDqcDA8Lqmxw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM14"" LableResourceID=""SALARYITEM14""  Description=""缺勤天数"" DataType=""string""  DataValue=""x9emtztU/sgIavVjNkv58g=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM15"" LableResourceID=""SALARYITEM15""  Description=""加班费"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM16"" LableResourceID=""SALARYITEM16""  Description=""值班津贴"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM17"" LableResourceID=""SALARYITEM17""  Description=""出勤工资"" DataType=""string""  DataValue=""anTrUit5SIxe1/PD5Z9CBQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM18"" LableResourceID=""SALARYITEM18""  Description=""住房公积金扣款"" DataType=""string""  DataValue=""76kJBJMyBdcuvlX2c3Efwg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM19"" LableResourceID=""SALARYITEM19""  Description=""个人社保负担"" DataType=""string""  DataValue=""SHo4k5JPx23Cgk8j1lxmoA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM20"" LableResourceID=""SALARYITEM20""  Description=""税前应发合计"" DataType=""string""  DataValue=""anTrUit5SIxe1/PD5Z9CBQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM21"" LableResourceID=""SALARYITEM21""  Description=""假期其它扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM22"" LableResourceID=""SALARYITEM22""  Description=""其它加扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM23"" LableResourceID=""SALARYITEM23""  Description=""绩效奖金"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM24"" LableResourceID=""SALARYITEM24""  Description=""纳税系数"" DataType=""string""  DataValue=""LWo3qQqyopqpUN6MfuaRqg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM25"" LableResourceID=""SALARYITEM25""  Description=""计税工资"" DataType=""string""  DataValue=""Y0tXcC6t8ogZRHqSQH77iA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM26"" LableResourceID=""SALARYITEM26""  Description=""扣税基数"" DataType=""string""  DataValue=""mcnLb8ttbSi7fY6sdBo7nw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM27"" LableResourceID=""SALARYITEM27""  Description=""差额"" DataType=""string""  DataValue=""aAHaogitvtxg97E5BtSOoA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM28"" LableResourceID=""SALARYITEM28""  Description=""税率"" DataType=""string""  DataValue=""qDySDZA5PwS10IHCyj4pIQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM29"" LableResourceID=""SALARYITEM29""  Description=""速算扣除数"" DataType=""string""  DataValue=""yFkkcABi5xQn+peJ6sAUiA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM30"" LableResourceID=""SALARYITEM30""  Description=""个人所得税"" DataType=""string""  DataValue=""2HWGIwkH4/UzIaxP/5JxIA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM31"" LableResourceID=""SALARYITEM31""  Description=""考勤异常扣款"" DataType=""string""  DataValue=""y9qAIjb5Bh0TzNAiwqVEJQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM32"" LableResourceID=""SALARYITEM32""  Description=""其它代扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM33"" LableResourceID=""SALARYITEM33""  Description=""尾数扣款"" DataType=""string""  DataValue=""4bnZZFYFbu05B99BR5oY3A=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM34"" LableResourceID=""SALARYITEM34""  Description=""扣款合计"" DataType=""string""  DataValue=""myjibveBAXgOVl7xEtvlkw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""REMARK"" LableResourceID=""REMARK""  Description=""备注"" DataType=""string""  DataValue=""8RYoJafePIrVJmsp3CH1cA=="" IsEncryption=""True""  DataText="""" /> 
</Object><Object Name=""SALARYRECORDBATCHDETAIL"" LableResourceID=""SALARYRECORDBATCHDETAIL"" Description=""月薪批量审核"" Key=""EMPLOYEESALARYRECORDID"" id=""31b8fb7b-30a6-4149-a969-12e8aa035a12"">
<Attribute Name=""DEPARTMENT"" LableResourceID=""DEPARTMENT""  Description=""部门"" DataType=""string""  DataValue=""财务部"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POST"" LableResourceID=""POST""  Description=""工作岗位"" DataType=""string""  DataValue=""财务总监"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""EMPLOYEENAME"" LableResourceID=""EMPLOYEENAME""  Description=""姓名"" DataType=""string""  DataValue=""彭昌桓"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POSTCODE"" LableResourceID=""POSTCODE""  Description=""职级代码"" DataType=""string""  DataValue=""财务总监"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""ACTUALLYPAY"" LableResourceID=""ACTUALLYPAY""  Description=""实发金额"" DataType=""string""  DataValue=""OIa0paUjL+aipI4mBGy1eg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""POSTLEVEL"" LableResourceID=""POSTLEVEL""  Description=""岗位级别"" DataType=""string""  DataValue=""11"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM6"" LableResourceID=""SALARYITEM6""  Description=""应发小计"" DataType=""string""  DataValue=""ZhkuaFrefyMl+a6HzBWhSg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM7"" LableResourceID=""SALARYITEM7""  Description=""基本工资"" DataType=""string""  DataValue=""YRV5C3bH/sD9SRnTHkhVrw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM8"" LableResourceID=""SALARYITEM8""  Description=""岗位工资"" DataType=""string""  DataValue=""xf/bdDMvGSSC1eftSOM74A=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM9"" LableResourceID=""SALARYITEM9""  Description=""保密津贴"" DataType=""string""  DataValue=""cLGsk42dW5NRqP5UqS+juw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM10"" LableResourceID=""SALARYITEM10""  Description=""住房补贴"" DataType=""string""  DataValue=""1kj0hTLtHy+HbRycK/J0wg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM11"" LableResourceID=""SALARYITEM11""  Description=""地区差异补贴"" DataType=""string""  DataValue=""ZubQna7nke4YmtSktJUjyw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM12"" LableResourceID=""SALARYITEM12""  Description=""餐费补贴"" DataType=""string""  DataValue=""lnMNsa2ofXzqoaNFenTZ4w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM13"" LableResourceID=""SALARYITEM13""  Description=""固定收入合计"" DataType=""string""  DataValue=""ZhkuaFrefyMl+a6HzBWhSg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM14"" LableResourceID=""SALARYITEM14""  Description=""缺勤天数"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM15"" LableResourceID=""SALARYITEM15""  Description=""加班费"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM16"" LableResourceID=""SALARYITEM16""  Description=""值班津贴"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM17"" LableResourceID=""SALARYITEM17""  Description=""出勤工资"" DataType=""string""  DataValue=""ZhkuaFrefyMl+a6HzBWhSg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM18"" LableResourceID=""SALARYITEM18""  Description=""住房公积金扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM19"" LableResourceID=""SALARYITEM19""  Description=""个人社保负担"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM20"" LableResourceID=""SALARYITEM20""  Description=""税前应发合计"" DataType=""string""  DataValue=""ZhkuaFrefyMl+a6HzBWhSg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM21"" LableResourceID=""SALARYITEM21""  Description=""假期其它扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM22"" LableResourceID=""SALARYITEM22""  Description=""其它加扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM23"" LableResourceID=""SALARYITEM23""  Description=""绩效奖金"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM24"" LableResourceID=""SALARYITEM24""  Description=""纳税系数"" DataType=""string""  DataValue=""LWo3qQqyopqpUN6MfuaRqg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM25"" LableResourceID=""SALARYITEM25""  Description=""计税工资"" DataType=""string""  DataValue=""ZhkuaFrefyMl+a6HzBWhSg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM26"" LableResourceID=""SALARYITEM26""  Description=""扣税基数"" DataType=""string""  DataValue=""mcnLb8ttbSi7fY6sdBo7nw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM27"" LableResourceID=""SALARYITEM27""  Description=""差额"" DataType=""string""  DataValue=""iZOBrU01OP+bC8rwr087qw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM28"" LableResourceID=""SALARYITEM28""  Description=""税率"" DataType=""string""  DataValue=""a6SjjHP67Qr9+9NL4w/EsQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM29"" LableResourceID=""SALARYITEM29""  Description=""速算扣除数"" DataType=""string""  DataValue=""Yp/l0GNnT6mku+jgIYRAkA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM30"" LableResourceID=""SALARYITEM30""  Description=""个人所得税"" DataType=""string""  DataValue=""bd1k6ixOcyvHzlNYG+yY6Q=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM31"" LableResourceID=""SALARYITEM31""  Description=""考勤异常扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM32"" LableResourceID=""SALARYITEM32""  Description=""其它代扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM33"" LableResourceID=""SALARYITEM33""  Description=""尾数扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM34"" LableResourceID=""SALARYITEM34""  Description=""扣款合计"" DataType=""string""  DataValue=""bd1k6ixOcyvHzlNYG+yY6Q=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""REMARK"" LableResourceID=""REMARK""  Description=""备注"" DataType=""string""  DataValue=""8RYoJafePIrVJmsp3CH1cA=="" IsEncryption=""True""  DataText="""" /> 
</Object><Object Name=""SALARYRECORDBATCHDETAIL"" LableResourceID=""SALARYRECORDBATCHDETAIL"" Description=""月薪批量审核"" Key=""EMPLOYEESALARYRECORDID"" id=""45455f0d-79fe-473d-81d3-c091bbe2fbc4"">
<Attribute Name=""DEPARTMENT"" LableResourceID=""DEPARTMENT""  Description=""部门"" DataType=""string""  DataValue=""营销管理部"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POST"" LableResourceID=""POST""  Description=""工作岗位"" DataType=""string""  DataValue=""品牌经理"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""EMPLOYEENAME"" LableResourceID=""EMPLOYEENAME""  Description=""姓名"" DataType=""string""  DataValue=""李琛"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POSTCODE"" LableResourceID=""POSTCODE""  Description=""职级代码"" DataType=""string""  DataValue=""品牌经理"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""ACTUALLYPAY"" LableResourceID=""ACTUALLYPAY""  Description=""实发金额"" DataType=""string""  DataValue=""XPJ8mCzCaaLzGiw9JQzIMw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""POSTLEVEL"" LableResourceID=""POSTLEVEL""  Description=""岗位级别"" DataType=""string""  DataValue=""14"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM6"" LableResourceID=""SALARYITEM6""  Description=""应发小计"" DataType=""string""  DataValue=""j+TKrUGdPrqDqcDA8Lqmxw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM7"" LableResourceID=""SALARYITEM7""  Description=""基本工资"" DataType=""string""  DataValue=""0nqOtpB6YSdtawEEbnknMA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM8"" LableResourceID=""SALARYITEM8""  Description=""岗位工资"" DataType=""string""  DataValue=""sejHKXMQXrYka/scZ+D3xQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM9"" LableResourceID=""SALARYITEM9""  Description=""保密津贴"" DataType=""string""  DataValue=""2nmvmCXlTDI/e2JBocktqg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM10"" LableResourceID=""SALARYITEM10""  Description=""住房补贴"" DataType=""string""  DataValue=""d+GqOfRcsvifSWQsRC7Mvw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM11"" LableResourceID=""SALARYITEM11""  Description=""地区差异补贴"" DataType=""string""  DataValue=""Rl4d+Pm6WwyGycLt8Ppnjg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM12"" LableResourceID=""SALARYITEM12""  Description=""餐费补贴"" DataType=""string""  DataValue=""lnMNsa2ofXzqoaNFenTZ4w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM13"" LableResourceID=""SALARYITEM13""  Description=""固定收入合计"" DataType=""string""  DataValue=""j+TKrUGdPrqDqcDA8Lqmxw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM14"" LableResourceID=""SALARYITEM14""  Description=""缺勤天数"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM15"" LableResourceID=""SALARYITEM15""  Description=""加班费"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM16"" LableResourceID=""SALARYITEM16""  Description=""值班津贴"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM17"" LableResourceID=""SALARYITEM17""  Description=""出勤工资"" DataType=""string""  DataValue=""j+TKrUGdPrqDqcDA8Lqmxw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM18"" LableResourceID=""SALARYITEM18""  Description=""住房公积金扣款"" DataType=""string""  DataValue=""76kJBJMyBdcuvlX2c3Efwg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM19"" LableResourceID=""SALARYITEM19""  Description=""个人社保负担"" DataType=""string""  DataValue=""SHo4k5JPx23Cgk8j1lxmoA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM20"" LableResourceID=""SALARYITEM20""  Description=""税前应发合计"" DataType=""string""  DataValue=""j+TKrUGdPrqDqcDA8Lqmxw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM21"" LableResourceID=""SALARYITEM21""  Description=""假期其它扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM22"" LableResourceID=""SALARYITEM22""  Description=""其它加扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM23"" LableResourceID=""SALARYITEM23""  Description=""绩效奖金"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM24"" LableResourceID=""SALARYITEM24""  Description=""纳税系数"" DataType=""string""  DataValue=""LWo3qQqyopqpUN6MfuaRqg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM25"" LableResourceID=""SALARYITEM25""  Description=""计税工资"" DataType=""string""  DataValue=""WTGcikuRAGyBPYX4czYsZw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM26"" LableResourceID=""SALARYITEM26""  Description=""扣税基数"" DataType=""string""  DataValue=""mcnLb8ttbSi7fY6sdBo7nw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM27"" LableResourceID=""SALARYITEM27""  Description=""差额"" DataType=""string""  DataValue=""Zjx6TDEzFdslzMrNv7TlQg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM28"" LableResourceID=""SALARYITEM28""  Description=""税率"" DataType=""string""  DataValue=""qDySDZA5PwS10IHCyj4pIQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM29"" LableResourceID=""SALARYITEM29""  Description=""速算扣除数"" DataType=""string""  DataValue=""yFkkcABi5xQn+peJ6sAUiA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM30"" LableResourceID=""SALARYITEM30""  Description=""个人所得税"" DataType=""string""  DataValue=""bRs8rR99+a6VqZkBQ9bOGw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM31"" LableResourceID=""SALARYITEM31""  Description=""考勤异常扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM32"" LableResourceID=""SALARYITEM32""  Description=""其它代扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM33"" LableResourceID=""SALARYITEM33""  Description=""尾数扣款"" DataType=""string""  DataValue=""Kj9/oDNfgqL7FmfuA/agGA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM34"" LableResourceID=""SALARYITEM34""  Description=""扣款合计"" DataType=""string""  DataValue=""yk1emYb4nDsVCW7AniPUxw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""REMARK"" LableResourceID=""REMARK""  Description=""备注"" DataType=""string""  DataValue=""8RYoJafePIrVJmsp3CH1cA=="" IsEncryption=""True""  DataText="""" /> 
</Object><Object Name=""SALARYRECORDBATCHDETAIL"" LableResourceID=""SALARYRECORDBATCHDETAIL"" Description=""月薪批量审核"" Key=""EMPLOYEESALARYRECORDID"" id=""4652efbc-8509-421e-9efc-7fbc9a323415"">
<Attribute Name=""DEPARTMENT"" LableResourceID=""DEPARTMENT""  Description=""部门"" DataType=""string""  DataValue=""工程管理部"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POST"" LableResourceID=""POST""  Description=""工作岗位"" DataType=""string""  DataValue=""总工程师"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""EMPLOYEENAME"" LableResourceID=""EMPLOYEENAME""  Description=""姓名"" DataType=""string""  DataValue=""肖建文"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POSTCODE"" LableResourceID=""POSTCODE""  Description=""职级代码"" DataType=""string""  DataValue=""总工程师"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""ACTUALLYPAY"" LableResourceID=""ACTUALLYPAY""  Description=""实发金额"" DataType=""string""  DataValue=""ewO8KJTgoSX56FnVmXajMQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""POSTLEVEL"" LableResourceID=""POSTLEVEL""  Description=""岗位级别"" DataType=""string""  DataValue=""11"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM6"" LableResourceID=""SALARYITEM6""  Description=""应发小计"" DataType=""string""  DataValue=""3C2U1Ww3Jy/WxvDJ5CJpWA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM7"" LableResourceID=""SALARYITEM7""  Description=""基本工资"" DataType=""string""  DataValue=""RU/CpR5D03506QU3dtHgtQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM8"" LableResourceID=""SALARYITEM8""  Description=""岗位工资"" DataType=""string""  DataValue=""FiLsjaBq+UGc9YLnPls3jA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM9"" LableResourceID=""SALARYITEM9""  Description=""保密津贴"" DataType=""string""  DataValue=""EVEYj9AjHTHrebKA9N85xw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM10"" LableResourceID=""SALARYITEM10""  Description=""住房补贴"" DataType=""string""  DataValue=""m/azVkKff58eSWvFzlDT5A=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM11"" LableResourceID=""SALARYITEM11""  Description=""地区差异补贴"" DataType=""string""  DataValue=""ZubQna7nke4YmtSktJUjyw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM12"" LableResourceID=""SALARYITEM12""  Description=""餐费补贴"" DataType=""string""  DataValue=""lnMNsa2ofXzqoaNFenTZ4w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM13"" LableResourceID=""SALARYITEM13""  Description=""固定收入合计"" DataType=""string""  DataValue=""3C2U1Ww3Jy/WxvDJ5CJpWA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM14"" LableResourceID=""SALARYITEM14""  Description=""缺勤天数"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM15"" LableResourceID=""SALARYITEM15""  Description=""加班费"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM16"" LableResourceID=""SALARYITEM16""  Description=""值班津贴"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM17"" LableResourceID=""SALARYITEM17""  Description=""出勤工资"" DataType=""string""  DataValue=""3C2U1Ww3Jy/WxvDJ5CJpWA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM18"" LableResourceID=""SALARYITEM18""  Description=""住房公积金扣款"" DataType=""string""  DataValue=""76kJBJMyBdcuvlX2c3Efwg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM19"" LableResourceID=""SALARYITEM19""  Description=""个人社保负担"" DataType=""string""  DataValue=""SHo4k5JPx23Cgk8j1lxmoA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM20"" LableResourceID=""SALARYITEM20""  Description=""税前应发合计"" DataType=""string""  DataValue=""3C2U1Ww3Jy/WxvDJ5CJpWA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM21"" LableResourceID=""SALARYITEM21""  Description=""假期其它扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM22"" LableResourceID=""SALARYITEM22""  Description=""其它加扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM23"" LableResourceID=""SALARYITEM23""  Description=""绩效奖金"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM24"" LableResourceID=""SALARYITEM24""  Description=""纳税系数"" DataType=""string""  DataValue=""LWo3qQqyopqpUN6MfuaRqg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM25"" LableResourceID=""SALARYITEM25""  Description=""计税工资"" DataType=""string""  DataValue=""jcvIafjEygIzfeNpn1Rpwg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM26"" LableResourceID=""SALARYITEM26""  Description=""扣税基数"" DataType=""string""  DataValue=""mcnLb8ttbSi7fY6sdBo7nw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM27"" LableResourceID=""SALARYITEM27""  Description=""差额"" DataType=""string""  DataValue=""WeikNVzMTpGARiwlOmjb9Q=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM28"" LableResourceID=""SALARYITEM28""  Description=""税率"" DataType=""string""  DataValue=""AzPp4W+AxGPty4kBmjd45w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM29"" LableResourceID=""SALARYITEM29""  Description=""速算扣除数"" DataType=""string""  DataValue=""Njt11fXCSEDT/RKlpg5U9g=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM30"" LableResourceID=""SALARYITEM30""  Description=""个人所得税"" DataType=""string""  DataValue=""x2M2U1pbx2m72m8yOzR2PA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM31"" LableResourceID=""SALARYITEM31""  Description=""考勤异常扣款"" DataType=""string""  DataValue=""kqYtHDijbP3yxAXR7Iapeg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM32"" LableResourceID=""SALARYITEM32""  Description=""其它代扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM33"" LableResourceID=""SALARYITEM33""  Description=""尾数扣款"" DataType=""string""  DataValue=""4ETeARo6LWQZnlro0nkK2A=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM34"" LableResourceID=""SALARYITEM34""  Description=""扣款合计"" DataType=""string""  DataValue=""D2tngTtEIihypkOj1ULzbg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""REMARK"" LableResourceID=""REMARK""  Description=""备注"" DataType=""string""  DataValue=""8RYoJafePIrVJmsp3CH1cA=="" IsEncryption=""True""  DataText="""" /> 
</Object><Object Name=""SALARYRECORDBATCHDETAIL"" LableResourceID=""SALARYRECORDBATCHDETAIL"" Description=""月薪批量审核"" Key=""EMPLOYEESALARYRECORDID"" id=""4799f236-558f-45e6-bd72-9bb004c2f1fe"">
<Attribute Name=""DEPARTMENT"" LableResourceID=""DEPARTMENT""  Description=""部门"" DataType=""string""  DataValue=""财务部"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POST"" LableResourceID=""POST""  Description=""工作岗位"" DataType=""string""  DataValue=""会计"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""EMPLOYEENAME"" LableResourceID=""EMPLOYEENAME""  Description=""姓名"" DataType=""string""  DataValue=""周燕"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POSTCODE"" LableResourceID=""POSTCODE""  Description=""职级代码"" DataType=""string""  DataValue=""会计"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""ACTUALLYPAY"" LableResourceID=""ACTUALLYPAY""  Description=""实发金额"" DataType=""string""  DataValue=""fk5G/zL0ZI7vLtKU7srLXA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""POSTLEVEL"" LableResourceID=""POSTLEVEL""  Description=""岗位级别"" DataType=""string""  DataValue=""16"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM6"" LableResourceID=""SALARYITEM6""  Description=""应发小计"" DataType=""string""  DataValue=""CyvsfyjIS2tYwNtvuG03fA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM7"" LableResourceID=""SALARYITEM7""  Description=""基本工资"" DataType=""string""  DataValue=""HYSzmQ5Iin+qngh4QCIa7A=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM8"" LableResourceID=""SALARYITEM8""  Description=""岗位工资"" DataType=""string""  DataValue=""r6OYwgtZfSzMNzuTYjexgw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM9"" LableResourceID=""SALARYITEM9""  Description=""保密津贴"" DataType=""string""  DataValue=""dd9ZT2+e0aFdxNpYGpRjew=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM10"" LableResourceID=""SALARYITEM10""  Description=""住房补贴"" DataType=""string""  DataValue=""RUqqd0Y3FJPR/G6lMD+xwg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM11"" LableResourceID=""SALARYITEM11""  Description=""地区差异补贴"" DataType=""string""  DataValue=""mUHZwyIyA9nRyb8L08Spog=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM12"" LableResourceID=""SALARYITEM12""  Description=""餐费补贴"" DataType=""string""  DataValue=""lnMNsa2ofXzqoaNFenTZ4w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM13"" LableResourceID=""SALARYITEM13""  Description=""固定收入合计"" DataType=""string""  DataValue=""CyvsfyjIS2tYwNtvuG03fA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM14"" LableResourceID=""SALARYITEM14""  Description=""缺勤天数"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM15"" LableResourceID=""SALARYITEM15""  Description=""加班费"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM16"" LableResourceID=""SALARYITEM16""  Description=""值班津贴"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM17"" LableResourceID=""SALARYITEM17""  Description=""出勤工资"" DataType=""string""  DataValue=""CyvsfyjIS2tYwNtvuG03fA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM18"" LableResourceID=""SALARYITEM18""  Description=""住房公积金扣款"" DataType=""string""  DataValue=""76kJBJMyBdcuvlX2c3Efwg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM19"" LableResourceID=""SALARYITEM19""  Description=""个人社保负担"" DataType=""string""  DataValue=""SHo4k5JPx23Cgk8j1lxmoA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM20"" LableResourceID=""SALARYITEM20""  Description=""税前应发合计"" DataType=""string""  DataValue=""CyvsfyjIS2tYwNtvuG03fA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM21"" LableResourceID=""SALARYITEM21""  Description=""假期其它扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM22"" LableResourceID=""SALARYITEM22""  Description=""其它加扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM23"" LableResourceID=""SALARYITEM23""  Description=""绩效奖金"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM24"" LableResourceID=""SALARYITEM24""  Description=""纳税系数"" DataType=""string""  DataValue=""LWo3qQqyopqpUN6MfuaRqg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM25"" LableResourceID=""SALARYITEM25""  Description=""计税工资"" DataType=""string""  DataValue=""62hekdADYa4o8qLp9K7l9A=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM26"" LableResourceID=""SALARYITEM26""  Description=""扣税基数"" DataType=""string""  DataValue=""mcnLb8ttbSi7fY6sdBo7nw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM27"" LableResourceID=""SALARYITEM27""  Description=""差额"" DataType=""string""  DataValue=""BqXTKzfXwa2wRli/pFaRSQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM28"" LableResourceID=""SALARYITEM28""  Description=""税率"" DataType=""string""  DataValue=""qDySDZA5PwS10IHCyj4pIQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM29"" LableResourceID=""SALARYITEM29""  Description=""速算扣除数"" DataType=""string""  DataValue=""yFkkcABi5xQn+peJ6sAUiA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM30"" LableResourceID=""SALARYITEM30""  Description=""个人所得税"" DataType=""string""  DataValue=""sP8QSMX9nEy798hPTKEZxQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM31"" LableResourceID=""SALARYITEM31""  Description=""考勤异常扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM32"" LableResourceID=""SALARYITEM32""  Description=""其它代扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM33"" LableResourceID=""SALARYITEM33""  Description=""尾数扣款"" DataType=""string""  DataValue=""Kj9/oDNfgqL7FmfuA/agGA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM34"" LableResourceID=""SALARYITEM34""  Description=""扣款合计"" DataType=""string""  DataValue=""wzSOWdp4SWemnVGwd561ug=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""REMARK"" LableResourceID=""REMARK""  Description=""备注"" DataType=""string""  DataValue=""8RYoJafePIrVJmsp3CH1cA=="" IsEncryption=""True""  DataText="""" /> 
</Object><Object Name=""SALARYRECORDBATCHDETAIL"" LableResourceID=""SALARYRECORDBATCHDETAIL"" Description=""月薪批量审核"" Key=""EMPLOYEESALARYRECORDID"" id=""5b58df78-fc79-4138-968d-a153ccb5e579"">
<Attribute Name=""DEPARTMENT"" LableResourceID=""DEPARTMENT""  Description=""部门"" DataType=""string""  DataValue=""综合管理部"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POST"" LableResourceID=""POST""  Description=""工作岗位"" DataType=""string""  DataValue=""副总经理"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""EMPLOYEENAME"" LableResourceID=""EMPLOYEENAME""  Description=""姓名"" DataType=""string""  DataValue=""周宏翔"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POSTCODE"" LableResourceID=""POSTCODE""  Description=""职级代码"" DataType=""string""  DataValue=""副总经理"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""ACTUALLYPAY"" LableResourceID=""ACTUALLYPAY""  Description=""实发金额"" DataType=""string""  DataValue=""3qKCyYLo/rhcYoM9QCcRCQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""POSTLEVEL"" LableResourceID=""POSTLEVEL""  Description=""岗位级别"" DataType=""string""  DataValue=""8"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM6"" LableResourceID=""SALARYITEM6""  Description=""应发小计"" DataType=""string""  DataValue=""FUz6b3l/v5V4ZEK7x3BUWg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM7"" LableResourceID=""SALARYITEM7""  Description=""基本工资"" DataType=""string""  DataValue=""XHtGHX7833sq03LdSI92Cg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM8"" LableResourceID=""SALARYITEM8""  Description=""岗位工资"" DataType=""string""  DataValue=""PU3NQqoS7u4cPVLmgcZ6Dg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM9"" LableResourceID=""SALARYITEM9""  Description=""保密津贴"" DataType=""string""  DataValue=""4VUHN2ChHbkzyIB3Y+vKXA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM10"" LableResourceID=""SALARYITEM10""  Description=""住房补贴"" DataType=""string""  DataValue=""OpRtDqh7gkE9BeIlC6otQw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM11"" LableResourceID=""SALARYITEM11""  Description=""地区差异补贴"" DataType=""string""  DataValue=""ZubQna7nke4YmtSktJUjyw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM12"" LableResourceID=""SALARYITEM12""  Description=""餐费补贴"" DataType=""string""  DataValue=""lnMNsa2ofXzqoaNFenTZ4w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM13"" LableResourceID=""SALARYITEM13""  Description=""固定收入合计"" DataType=""string""  DataValue=""FUz6b3l/v5V4ZEK7x3BUWg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM14"" LableResourceID=""SALARYITEM14""  Description=""缺勤天数"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM15"" LableResourceID=""SALARYITEM15""  Description=""加班费"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM16"" LableResourceID=""SALARYITEM16""  Description=""值班津贴"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM17"" LableResourceID=""SALARYITEM17""  Description=""出勤工资"" DataType=""string""  DataValue=""FUz6b3l/v5V4ZEK7x3BUWg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM18"" LableResourceID=""SALARYITEM18""  Description=""住房公积金扣款"" DataType=""string""  DataValue=""76kJBJMyBdcuvlX2c3Efwg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM19"" LableResourceID=""SALARYITEM19""  Description=""个人社保负担"" DataType=""string""  DataValue=""SHo4k5JPx23Cgk8j1lxmoA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM20"" LableResourceID=""SALARYITEM20""  Description=""税前应发合计"" DataType=""string""  DataValue=""FUz6b3l/v5V4ZEK7x3BUWg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM21"" LableResourceID=""SALARYITEM21""  Description=""假期其它扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM22"" LableResourceID=""SALARYITEM22""  Description=""其它加扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM23"" LableResourceID=""SALARYITEM23""  Description=""绩效奖金"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM24"" LableResourceID=""SALARYITEM24""  Description=""纳税系数"" DataType=""string""  DataValue=""LWo3qQqyopqpUN6MfuaRqg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM25"" LableResourceID=""SALARYITEM25""  Description=""计税工资"" DataType=""string""  DataValue=""CUgfgF5wLBcjFfwYfMMxcA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM26"" LableResourceID=""SALARYITEM26""  Description=""扣税基数"" DataType=""string""  DataValue=""mcnLb8ttbSi7fY6sdBo7nw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM27"" LableResourceID=""SALARYITEM27""  Description=""差额"" DataType=""string""  DataValue=""w9WXpoXipNcOb1mEgz7BxQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM28"" LableResourceID=""SALARYITEM28""  Description=""税率"" DataType=""string""  DataValue=""a6SjjHP67Qr9+9NL4w/EsQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM29"" LableResourceID=""SALARYITEM29""  Description=""速算扣除数"" DataType=""string""  DataValue=""Yp/l0GNnT6mku+jgIYRAkA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM30"" LableResourceID=""SALARYITEM30""  Description=""个人所得税"" DataType=""string""  DataValue=""BOO44UghB+UqMfFHqzd9rA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM31"" LableResourceID=""SALARYITEM31""  Description=""考勤异常扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM32"" LableResourceID=""SALARYITEM32""  Description=""其它代扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM33"" LableResourceID=""SALARYITEM33""  Description=""尾数扣款"" DataType=""string""  DataValue=""y9wCJcGW/ky2FoHndXKEAw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM34"" LableResourceID=""SALARYITEM34""  Description=""扣款合计"" DataType=""string""  DataValue=""PxGyyOkqCuS2pb1REuo0Tw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""REMARK"" LableResourceID=""REMARK""  Description=""备注"" DataType=""string""  DataValue=""8RYoJafePIrVJmsp3CH1cA=="" IsEncryption=""True""  DataText="""" /> 
</Object><Object Name=""SALARYRECORDBATCHDETAIL"" LableResourceID=""SALARYRECORDBATCHDETAIL"" Description=""月薪批量审核"" Key=""EMPLOYEESALARYRECORDID"" id=""5cbf4f9e-9aea-4dac-ab9b-babb5f7235f4"">
<Attribute Name=""DEPARTMENT"" LableResourceID=""DEPARTMENT""  Description=""部门"" DataType=""string""  DataValue=""财务部"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POST"" LableResourceID=""POST""  Description=""工作岗位"" DataType=""string""  DataValue=""财务部总监"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""EMPLOYEENAME"" LableResourceID=""EMPLOYEENAME""  Description=""姓名"" DataType=""string""  DataValue=""章翔"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POSTCODE"" LableResourceID=""POSTCODE""  Description=""职级代码"" DataType=""string""  DataValue=""财务部总监"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""ACTUALLYPAY"" LableResourceID=""ACTUALLYPAY""  Description=""实发金额"" DataType=""string""  DataValue=""MF+GFOJ5s56u5GW/NjBK/Q=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""POSTLEVEL"" LableResourceID=""POSTLEVEL""  Description=""岗位级别"" DataType=""string""  DataValue=""13"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM6"" LableResourceID=""SALARYITEM6""  Description=""应发小计"" DataType=""string""  DataValue=""xR9JKpfyQshnD8nbTrkBhg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM7"" LableResourceID=""SALARYITEM7""  Description=""基本工资"" DataType=""string""  DataValue=""cfSzI1qwohuvMoBa5Ouslg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM8"" LableResourceID=""SALARYITEM8""  Description=""岗位工资"" DataType=""string""  DataValue=""yYYzqDJJNocpZPpkSsuGyA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM9"" LableResourceID=""SALARYITEM9""  Description=""保密津贴"" DataType=""string""  DataValue=""8UttihCCpS5NHXVwz+p+eA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM10"" LableResourceID=""SALARYITEM10""  Description=""住房补贴"" DataType=""string""  DataValue=""JHrsKqlqOUXZcTNwxbeUNg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM11"" LableResourceID=""SALARYITEM11""  Description=""地区差异补贴"" DataType=""string""  DataValue=""JHrsKqlqOUXZcTNwxbeUNg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM12"" LableResourceID=""SALARYITEM12""  Description=""餐费补贴"" DataType=""string""  DataValue=""lnMNsa2ofXzqoaNFenTZ4w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM13"" LableResourceID=""SALARYITEM13""  Description=""固定收入合计"" DataType=""string""  DataValue=""xR9JKpfyQshnD8nbTrkBhg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM14"" LableResourceID=""SALARYITEM14""  Description=""缺勤天数"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM15"" LableResourceID=""SALARYITEM15""  Description=""加班费"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM16"" LableResourceID=""SALARYITEM16""  Description=""值班津贴"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM17"" LableResourceID=""SALARYITEM17""  Description=""出勤工资"" DataType=""string""  DataValue=""xR9JKpfyQshnD8nbTrkBhg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM18"" LableResourceID=""SALARYITEM18""  Description=""住房公积金扣款"" DataType=""string""  DataValue=""76kJBJMyBdcuvlX2c3Efwg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM19"" LableResourceID=""SALARYITEM19""  Description=""个人社保负担"" DataType=""string""  DataValue=""SHo4k5JPx23Cgk8j1lxmoA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM20"" LableResourceID=""SALARYITEM20""  Description=""税前应发合计"" DataType=""string""  DataValue=""xR9JKpfyQshnD8nbTrkBhg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM21"" LableResourceID=""SALARYITEM21""  Description=""假期其它扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM22"" LableResourceID=""SALARYITEM22""  Description=""其它加扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM23"" LableResourceID=""SALARYITEM23""  Description=""绩效奖金"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM24"" LableResourceID=""SALARYITEM24""  Description=""纳税系数"" DataType=""string""  DataValue=""LWo3qQqyopqpUN6MfuaRqg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM25"" LableResourceID=""SALARYITEM25""  Description=""计税工资"" DataType=""string""  DataValue=""VUTQutVlAJjvqSddcNzsTg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM26"" LableResourceID=""SALARYITEM26""  Description=""扣税基数"" DataType=""string""  DataValue=""mcnLb8ttbSi7fY6sdBo7nw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM27"" LableResourceID=""SALARYITEM27""  Description=""差额"" DataType=""string""  DataValue=""5PPfA61sqxbBF/FoVpTDmw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM28"" LableResourceID=""SALARYITEM28""  Description=""税率"" DataType=""string""  DataValue=""qDySDZA5PwS10IHCyj4pIQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM29"" LableResourceID=""SALARYITEM29""  Description=""速算扣除数"" DataType=""string""  DataValue=""yFkkcABi5xQn+peJ6sAUiA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM30"" LableResourceID=""SALARYITEM30""  Description=""个人所得税"" DataType=""string""  DataValue=""1o8+xnN7/5wyH7mZhL3YiQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM31"" LableResourceID=""SALARYITEM31""  Description=""考勤异常扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM32"" LableResourceID=""SALARYITEM32""  Description=""其它代扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM33"" LableResourceID=""SALARYITEM33""  Description=""尾数扣款"" DataType=""string""  DataValue=""Kj9/oDNfgqL7FmfuA/agGA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM34"" LableResourceID=""SALARYITEM34""  Description=""扣款合计"" DataType=""string""  DataValue=""+Gye5t5paOtg5dllnoF6fA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""REMARK"" LableResourceID=""REMARK""  Description=""备注"" DataType=""string""  DataValue=""8RYoJafePIrVJmsp3CH1cA=="" IsEncryption=""True""  DataText="""" /> 
</Object><Object Name=""SALARYRECORDBATCHDETAIL"" LableResourceID=""SALARYRECORDBATCHDETAIL"" Description=""月薪批量审核"" Key=""EMPLOYEESALARYRECORDID"" id=""730e8ed8-c66f-46d0-8803-70194f516331"">
<Attribute Name=""DEPARTMENT"" LableResourceID=""DEPARTMENT""  Description=""部门"" DataType=""string""  DataValue=""财务部"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POST"" LableResourceID=""POST""  Description=""工作岗位"" DataType=""string""  DataValue=""出纳"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""EMPLOYEENAME"" LableResourceID=""EMPLOYEENAME""  Description=""姓名"" DataType=""string""  DataValue=""吴钟玲"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POSTCODE"" LableResourceID=""POSTCODE""  Description=""职级代码"" DataType=""string""  DataValue=""出纳"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""ACTUALLYPAY"" LableResourceID=""ACTUALLYPAY""  Description=""实发金额"" DataType=""string""  DataValue=""LWljBerZrqa9zeDQAalDvg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""POSTLEVEL"" LableResourceID=""POSTLEVEL""  Description=""岗位级别"" DataType=""string""  DataValue=""19"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM6"" LableResourceID=""SALARYITEM6""  Description=""应发小计"" DataType=""string""  DataValue=""/v5AzKPLCSRMbVOxF9StOw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM7"" LableResourceID=""SALARYITEM7""  Description=""基本工资"" DataType=""string""  DataValue=""u9gAtuiKC7QsAkarEdzx3Q=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM8"" LableResourceID=""SALARYITEM8""  Description=""岗位工资"" DataType=""string""  DataValue=""kJcjcZ+yQHCKaHb3zS8GXw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM9"" LableResourceID=""SALARYITEM9""  Description=""保密津贴"" DataType=""string""  DataValue=""/OVPlSfyadpig3f/typcog=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM10"" LableResourceID=""SALARYITEM10""  Description=""住房补贴"" DataType=""string""  DataValue=""6lDENcUVHiB1SkH7aensgg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM11"" LableResourceID=""SALARYITEM11""  Description=""地区差异补贴"" DataType=""string""  DataValue=""L3iJL6O6xUrsa4RCtwWKeg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM12"" LableResourceID=""SALARYITEM12""  Description=""餐费补贴"" DataType=""string""  DataValue=""lnMNsa2ofXzqoaNFenTZ4w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM13"" LableResourceID=""SALARYITEM13""  Description=""固定收入合计"" DataType=""string""  DataValue=""/v5AzKPLCSRMbVOxF9StOw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM14"" LableResourceID=""SALARYITEM14""  Description=""缺勤天数"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM15"" LableResourceID=""SALARYITEM15""  Description=""加班费"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM16"" LableResourceID=""SALARYITEM16""  Description=""值班津贴"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM17"" LableResourceID=""SALARYITEM17""  Description=""出勤工资"" DataType=""string""  DataValue=""/v5AzKPLCSRMbVOxF9StOw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM18"" LableResourceID=""SALARYITEM18""  Description=""住房公积金扣款"" DataType=""string""  DataValue=""76kJBJMyBdcuvlX2c3Efwg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM19"" LableResourceID=""SALARYITEM19""  Description=""个人社保负担"" DataType=""string""  DataValue=""CDiVm1p67cInJ2ERSSl7uw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM20"" LableResourceID=""SALARYITEM20""  Description=""税前应发合计"" DataType=""string""  DataValue=""/v5AzKPLCSRMbVOxF9StOw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM21"" LableResourceID=""SALARYITEM21""  Description=""假期其它扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM22"" LableResourceID=""SALARYITEM22""  Description=""其它加扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM23"" LableResourceID=""SALARYITEM23""  Description=""绩效奖金"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM24"" LableResourceID=""SALARYITEM24""  Description=""纳税系数"" DataType=""string""  DataValue=""LWo3qQqyopqpUN6MfuaRqg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM25"" LableResourceID=""SALARYITEM25""  Description=""计税工资"" DataType=""string""  DataValue=""VSefQPvx6fWmZh2SM3h+9A=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM26"" LableResourceID=""SALARYITEM26""  Description=""扣税基数"" DataType=""string""  DataValue=""mcnLb8ttbSi7fY6sdBo7nw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM27"" LableResourceID=""SALARYITEM27""  Description=""差额"" DataType=""string""  DataValue=""op5eIsBoHgOTg2Z+mRPW5w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM28"" LableResourceID=""SALARYITEM28""  Description=""税率"" DataType=""string""  DataValue=""gaoxG00iu/KKvpEXv7I5UA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM29"" LableResourceID=""SALARYITEM29""  Description=""速算扣除数"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM30"" LableResourceID=""SALARYITEM30""  Description=""个人所得税"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM31"" LableResourceID=""SALARYITEM31""  Description=""考勤异常扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM32"" LableResourceID=""SALARYITEM32""  Description=""其它代扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM33"" LableResourceID=""SALARYITEM33""  Description=""尾数扣款"" DataType=""string""  DataValue=""vrfIRTRA4VjwRWGxyxXmUg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM34"" LableResourceID=""SALARYITEM34""  Description=""扣款合计"" DataType=""string""  DataValue=""vrfIRTRA4VjwRWGxyxXmUg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""REMARK"" LableResourceID=""REMARK""  Description=""备注"" DataType=""string""  DataValue=""8RYoJafePIrVJmsp3CH1cA=="" IsEncryption=""True""  DataText="""" /> 
</Object><Object Name=""SALARYRECORDBATCHDETAIL"" LableResourceID=""SALARYRECORDBATCHDETAIL"" Description=""月薪批量审核"" Key=""EMPLOYEESALARYRECORDID"" id=""86319866-51f1-4189-9513-980a29357f57"">
<Attribute Name=""DEPARTMENT"" LableResourceID=""DEPARTMENT""  Description=""部门"" DataType=""string""  DataValue=""成本管理部"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POST"" LableResourceID=""POST""  Description=""工作岗位"" DataType=""string""  DataValue=""成本管理部总监"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""EMPLOYEENAME"" LableResourceID=""EMPLOYEENAME""  Description=""姓名"" DataType=""string""  DataValue=""夏翠蓉"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POSTCODE"" LableResourceID=""POSTCODE""  Description=""职级代码"" DataType=""string""  DataValue=""成本管理部总监"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""ACTUALLYPAY"" LableResourceID=""ACTUALLYPAY""  Description=""实发金额"" DataType=""string""  DataValue=""q+RYnOnV8cpkTwVXnpbFeQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""POSTLEVEL"" LableResourceID=""POSTLEVEL""  Description=""岗位级别"" DataType=""string""  DataValue=""11"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM6"" LableResourceID=""SALARYITEM6""  Description=""应发小计"" DataType=""string""  DataValue=""UIwAv0R7KtnHM1BMG5vatg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM7"" LableResourceID=""SALARYITEM7""  Description=""基本工资"" DataType=""string""  DataValue=""4NVh2cEP9NCLvFLWcs2Pdg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM8"" LableResourceID=""SALARYITEM8""  Description=""岗位工资"" DataType=""string""  DataValue=""1kax6KSg39+4sNHS4w/0/w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM9"" LableResourceID=""SALARYITEM9""  Description=""保密津贴"" DataType=""string""  DataValue=""QcJU6iFuntbwVufuATOeuQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM10"" LableResourceID=""SALARYITEM10""  Description=""住房补贴"" DataType=""string""  DataValue=""dE6RPqRf6vus/Dk1sjlnxA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM11"" LableResourceID=""SALARYITEM11""  Description=""地区差异补贴"" DataType=""string""  DataValue=""ZubQna7nke4YmtSktJUjyw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM12"" LableResourceID=""SALARYITEM12""  Description=""餐费补贴"" DataType=""string""  DataValue=""lnMNsa2ofXzqoaNFenTZ4w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM13"" LableResourceID=""SALARYITEM13""  Description=""固定收入合计"" DataType=""string""  DataValue=""/tObgsIU+QPFiGnyLLU6wA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM14"" LableResourceID=""SALARYITEM14""  Description=""缺勤天数"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM15"" LableResourceID=""SALARYITEM15""  Description=""加班费"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM16"" LableResourceID=""SALARYITEM16""  Description=""值班津贴"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM17"" LableResourceID=""SALARYITEM17""  Description=""出勤工资"" DataType=""string""  DataValue=""UIwAv0R7KtnHM1BMG5vatg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM18"" LableResourceID=""SALARYITEM18""  Description=""住房公积金扣款"" DataType=""string""  DataValue=""76kJBJMyBdcuvlX2c3Efwg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM19"" LableResourceID=""SALARYITEM19""  Description=""个人社保负担"" DataType=""string""  DataValue=""SHo4k5JPx23Cgk8j1lxmoA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM20"" LableResourceID=""SALARYITEM20""  Description=""税前应发合计"" DataType=""string""  DataValue=""UIwAv0R7KtnHM1BMG5vatg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM21"" LableResourceID=""SALARYITEM21""  Description=""假期其它扣款"" DataType=""string""  DataValue=""YJirFx831dt6A5DYyI6/mA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM22"" LableResourceID=""SALARYITEM22""  Description=""其它加扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM23"" LableResourceID=""SALARYITEM23""  Description=""绩效奖金"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM24"" LableResourceID=""SALARYITEM24""  Description=""纳税系数"" DataType=""string""  DataValue=""LWo3qQqyopqpUN6MfuaRqg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM25"" LableResourceID=""SALARYITEM25""  Description=""计税工资"" DataType=""string""  DataValue=""DHrxFKIX+QiQ3RwbpgpDAA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM26"" LableResourceID=""SALARYITEM26""  Description=""扣税基数"" DataType=""string""  DataValue=""mcnLb8ttbSi7fY6sdBo7nw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM27"" LableResourceID=""SALARYITEM27""  Description=""差额"" DataType=""string""  DataValue=""gCHzkLLFSyMDea7MGOP7TQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM28"" LableResourceID=""SALARYITEM28""  Description=""税率"" DataType=""string""  DataValue=""gaoxG00iu/KKvpEXv7I5UA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM29"" LableResourceID=""SALARYITEM29""  Description=""速算扣除数"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM30"" LableResourceID=""SALARYITEM30""  Description=""个人所得税"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM31"" LableResourceID=""SALARYITEM31""  Description=""考勤异常扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM32"" LableResourceID=""SALARYITEM32""  Description=""其它代扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM33"" LableResourceID=""SALARYITEM33""  Description=""尾数扣款"" DataType=""string""  DataValue=""2ke/kbQb6LpfvPWHKeXxzg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM34"" LableResourceID=""SALARYITEM34""  Description=""扣款合计"" DataType=""string""  DataValue=""2ke/kbQb6LpfvPWHKeXxzg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""REMARK"" LableResourceID=""REMARK""  Description=""备注"" DataType=""string""  DataValue=""8RYoJafePIrVJmsp3CH1cA=="" IsEncryption=""True""  DataText="""" /> 
</Object><Object Name=""SALARYRECORDBATCHDETAIL"" LableResourceID=""SALARYRECORDBATCHDETAIL"" Description=""月薪批量审核"" Key=""EMPLOYEESALARYRECORDID"" id=""8c06ee5e-1864-411e-917b-5f3050461ddb"">
<Attribute Name=""DEPARTMENT"" LableResourceID=""DEPARTMENT""  Description=""部门"" DataType=""string""  DataValue=""成本管理部"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POST"" LableResourceID=""POST""  Description=""工作岗位"" DataType=""string""  DataValue=""安装造价工程师"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""EMPLOYEENAME"" LableResourceID=""EMPLOYEENAME""  Description=""姓名"" DataType=""string""  DataValue=""叶芳"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POSTCODE"" LableResourceID=""POSTCODE""  Description=""职级代码"" DataType=""string""  DataValue=""安装造价工程师"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""ACTUALLYPAY"" LableResourceID=""ACTUALLYPAY""  Description=""实发金额"" DataType=""string""  DataValue=""+Ks4n1kaU3GJw1VJq+gpbA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""POSTLEVEL"" LableResourceID=""POSTLEVEL""  Description=""岗位级别"" DataType=""string""  DataValue=""15"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM6"" LableResourceID=""SALARYITEM6""  Description=""应发小计"" DataType=""string""  DataValue=""fMpjnHo0tN8EA7ay64ZFiQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM7"" LableResourceID=""SALARYITEM7""  Description=""基本工资"" DataType=""string""  DataValue=""ZGxKImOWw9eOugnUmmTdHA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM8"" LableResourceID=""SALARYITEM8""  Description=""岗位工资"" DataType=""string""  DataValue=""8G5K/fxkwbr86Go0PouG0A=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM9"" LableResourceID=""SALARYITEM9""  Description=""保密津贴"" DataType=""string""  DataValue=""6+j4VZ3vhmDWKbaZN60bJw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM10"" LableResourceID=""SALARYITEM10""  Description=""住房补贴"" DataType=""string""  DataValue=""RZJYQ7oazwTpQsrhqeC3Tw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM11"" LableResourceID=""SALARYITEM11""  Description=""地区差异补贴"" DataType=""string""  DataValue=""UNUWkqx6oE8W+9PLMQJLfg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM12"" LableResourceID=""SALARYITEM12""  Description=""餐费补贴"" DataType=""string""  DataValue=""lnMNsa2ofXzqoaNFenTZ4w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM13"" LableResourceID=""SALARYITEM13""  Description=""固定收入合计"" DataType=""string""  DataValue=""fMpjnHo0tN8EA7ay64ZFiQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM14"" LableResourceID=""SALARYITEM14""  Description=""缺勤天数"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM15"" LableResourceID=""SALARYITEM15""  Description=""加班费"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM16"" LableResourceID=""SALARYITEM16""  Description=""值班津贴"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM17"" LableResourceID=""SALARYITEM17""  Description=""出勤工资"" DataType=""string""  DataValue=""fMpjnHo0tN8EA7ay64ZFiQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM18"" LableResourceID=""SALARYITEM18""  Description=""住房公积金扣款"" DataType=""string""  DataValue=""76kJBJMyBdcuvlX2c3Efwg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM19"" LableResourceID=""SALARYITEM19""  Description=""个人社保负担"" DataType=""string""  DataValue=""CDiVm1p67cInJ2ERSSl7uw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM20"" LableResourceID=""SALARYITEM20""  Description=""税前应发合计"" DataType=""string""  DataValue=""fMpjnHo0tN8EA7ay64ZFiQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM21"" LableResourceID=""SALARYITEM21""  Description=""假期其它扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM22"" LableResourceID=""SALARYITEM22""  Description=""其它加扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM23"" LableResourceID=""SALARYITEM23""  Description=""绩效奖金"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM24"" LableResourceID=""SALARYITEM24""  Description=""纳税系数"" DataType=""string""  DataValue=""LWo3qQqyopqpUN6MfuaRqg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM25"" LableResourceID=""SALARYITEM25""  Description=""计税工资"" DataType=""string""  DataValue=""hyleGjI+yLIwVwG+qyul/Q=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM26"" LableResourceID=""SALARYITEM26""  Description=""扣税基数"" DataType=""string""  DataValue=""mcnLb8ttbSi7fY6sdBo7nw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM27"" LableResourceID=""SALARYITEM27""  Description=""差额"" DataType=""string""  DataValue=""SBbaZ5z+LMGeeNurKq0sQg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM28"" LableResourceID=""SALARYITEM28""  Description=""税率"" DataType=""string""  DataValue=""qDySDZA5PwS10IHCyj4pIQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM29"" LableResourceID=""SALARYITEM29""  Description=""速算扣除数"" DataType=""string""  DataValue=""yFkkcABi5xQn+peJ6sAUiA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM30"" LableResourceID=""SALARYITEM30""  Description=""个人所得税"" DataType=""string""  DataValue=""4N9SKHUJI1nCSiG7blNpMw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM31"" LableResourceID=""SALARYITEM31""  Description=""考勤异常扣款"" DataType=""string""  DataValue=""kqYtHDijbP3yxAXR7Iapeg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM32"" LableResourceID=""SALARYITEM32""  Description=""其它代扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM33"" LableResourceID=""SALARYITEM33""  Description=""尾数扣款"" DataType=""string""  DataValue=""GhyoAhSd2IHV3rk9w8hZfg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM34"" LableResourceID=""SALARYITEM34""  Description=""扣款合计"" DataType=""string""  DataValue=""IsfZVhEM27o7tERpjXXVdw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""REMARK"" LableResourceID=""REMARK""  Description=""备注"" DataType=""string""  DataValue=""8RYoJafePIrVJmsp3CH1cA=="" IsEncryption=""True""  DataText="""" /> 
</Object><Object Name=""SALARYRECORDBATCHDETAIL"" LableResourceID=""SALARYRECORDBATCHDETAIL"" Description=""月薪批量审核"" Key=""EMPLOYEESALARYRECORDID"" id=""9c8d00bf-51be-4a44-8bcd-d0906e4634bf"">
<Attribute Name=""DEPARTMENT"" LableResourceID=""DEPARTMENT""  Description=""部门"" DataType=""string""  DataValue=""营销管理部"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POST"" LableResourceID=""POST""  Description=""工作岗位"" DataType=""string""  DataValue=""营销管理部总监"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""EMPLOYEENAME"" LableResourceID=""EMPLOYEENAME""  Description=""姓名"" DataType=""string""  DataValue=""季文权"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POSTCODE"" LableResourceID=""POSTCODE""  Description=""职级代码"" DataType=""string""  DataValue=""营销管理部总监"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""ACTUALLYPAY"" LableResourceID=""ACTUALLYPAY""  Description=""实发金额"" DataType=""string""  DataValue=""/Ohg+mLwHSGS6MNpqt6iqA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""POSTLEVEL"" LableResourceID=""POSTLEVEL""  Description=""岗位级别"" DataType=""string""  DataValue=""11"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM6"" LableResourceID=""SALARYITEM6""  Description=""应发小计"" DataType=""string""  DataValue=""45lelM2cU7D5Ee7qDkocmA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM7"" LableResourceID=""SALARYITEM7""  Description=""基本工资"" DataType=""string""  DataValue=""HiS7PIrdW573MakvyLL4Ug=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM8"" LableResourceID=""SALARYITEM8""  Description=""岗位工资"" DataType=""string""  DataValue=""x7MbVnHcRPyrseQrsyw+wA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM9"" LableResourceID=""SALARYITEM9""  Description=""保密津贴"" DataType=""string""  DataValue=""81YJb/v4aLe/9K4+WZZIJA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM10"" LableResourceID=""SALARYITEM10""  Description=""住房补贴"" DataType=""string""  DataValue=""rglmT64wOobBVHNqNgmIGg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM11"" LableResourceID=""SALARYITEM11""  Description=""地区差异补贴"" DataType=""string""  DataValue=""ZubQna7nke4YmtSktJUjyw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM12"" LableResourceID=""SALARYITEM12""  Description=""餐费补贴"" DataType=""string""  DataValue=""lnMNsa2ofXzqoaNFenTZ4w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM13"" LableResourceID=""SALARYITEM13""  Description=""固定收入合计"" DataType=""string""  DataValue=""45lelM2cU7D5Ee7qDkocmA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM14"" LableResourceID=""SALARYITEM14""  Description=""缺勤天数"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM15"" LableResourceID=""SALARYITEM15""  Description=""加班费"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM16"" LableResourceID=""SALARYITEM16""  Description=""值班津贴"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM17"" LableResourceID=""SALARYITEM17""  Description=""出勤工资"" DataType=""string""  DataValue=""45lelM2cU7D5Ee7qDkocmA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM18"" LableResourceID=""SALARYITEM18""  Description=""住房公积金扣款"" DataType=""string""  DataValue=""76kJBJMyBdcuvlX2c3Efwg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM19"" LableResourceID=""SALARYITEM19""  Description=""个人社保负担"" DataType=""string""  DataValue=""SHo4k5JPx23Cgk8j1lxmoA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM20"" LableResourceID=""SALARYITEM20""  Description=""税前应发合计"" DataType=""string""  DataValue=""45lelM2cU7D5Ee7qDkocmA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM21"" LableResourceID=""SALARYITEM21""  Description=""假期其它扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM22"" LableResourceID=""SALARYITEM22""  Description=""其它加扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM23"" LableResourceID=""SALARYITEM23""  Description=""绩效奖金"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM24"" LableResourceID=""SALARYITEM24""  Description=""纳税系数"" DataType=""string""  DataValue=""LWo3qQqyopqpUN6MfuaRqg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM25"" LableResourceID=""SALARYITEM25""  Description=""计税工资"" DataType=""string""  DataValue=""uOIELZY1pt6UxNb1W30Efg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM26"" LableResourceID=""SALARYITEM26""  Description=""扣税基数"" DataType=""string""  DataValue=""mcnLb8ttbSi7fY6sdBo7nw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM27"" LableResourceID=""SALARYITEM27""  Description=""差额"" DataType=""string""  DataValue=""B2IWV0P+U9F3o+rlOqlnJw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM28"" LableResourceID=""SALARYITEM28""  Description=""税率"" DataType=""string""  DataValue=""AzPp4W+AxGPty4kBmjd45w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM29"" LableResourceID=""SALARYITEM29""  Description=""速算扣除数"" DataType=""string""  DataValue=""Njt11fXCSEDT/RKlpg5U9g=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM30"" LableResourceID=""SALARYITEM30""  Description=""个人所得税"" DataType=""string""  DataValue=""NtxfiCH9KWD+2s/voLbsGw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM31"" LableResourceID=""SALARYITEM31""  Description=""考勤异常扣款"" DataType=""string""  DataValue=""kqYtHDijbP3yxAXR7Iapeg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM32"" LableResourceID=""SALARYITEM32""  Description=""其它代扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM33"" LableResourceID=""SALARYITEM33""  Description=""尾数扣款"" DataType=""string""  DataValue=""4ETeARo6LWQZnlro0nkK2A=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM34"" LableResourceID=""SALARYITEM34""  Description=""扣款合计"" DataType=""string""  DataValue=""/KZFLBtPP24Nah6buTCbzQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""REMARK"" LableResourceID=""REMARK""  Description=""备注"" DataType=""string""  DataValue=""8RYoJafePIrVJmsp3CH1cA=="" IsEncryption=""True""  DataText="""" /> 
</Object><Object Name=""SALARYRECORDBATCHDETAIL"" LableResourceID=""SALARYRECORDBATCHDETAIL"" Description=""月薪批量审核"" Key=""EMPLOYEESALARYRECORDID"" id=""b019bc33-c3f9-45c8-8168-a52e990f94b8"">
<Attribute Name=""DEPARTMENT"" LableResourceID=""DEPARTMENT""  Description=""部门"" DataType=""string""  DataValue=""综合管理部"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POST"" LableResourceID=""POST""  Description=""工作岗位"" DataType=""string""  DataValue=""总经理"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""EMPLOYEENAME"" LableResourceID=""EMPLOYEENAME""  Description=""姓名"" DataType=""string""  DataValue=""李靖"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POSTCODE"" LableResourceID=""POSTCODE""  Description=""职级代码"" DataType=""string""  DataValue=""总经理"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""ACTUALLYPAY"" LableResourceID=""ACTUALLYPAY""  Description=""实发金额"" DataType=""string""  DataValue=""QvLzvTOJIBiPkd4skkAQ1A=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""POSTLEVEL"" LableResourceID=""POSTLEVEL""  Description=""岗位级别"" DataType=""string""  DataValue=""5"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM6"" LableResourceID=""SALARYITEM6""  Description=""应发小计"" DataType=""string""  DataValue=""m7zgJ9sX7FBgcsZ90COdMQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM7"" LableResourceID=""SALARYITEM7""  Description=""基本工资"" DataType=""string""  DataValue=""M+2m+rzTIdkInww8LHTGQw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM8"" LableResourceID=""SALARYITEM8""  Description=""岗位工资"" DataType=""string""  DataValue=""nNLag7W5egmuew4elb62IQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM9"" LableResourceID=""SALARYITEM9""  Description=""保密津贴"" DataType=""string""  DataValue=""qKjA3tGNP3bNh/n9GqxgBg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM10"" LableResourceID=""SALARYITEM10""  Description=""住房补贴"" DataType=""string""  DataValue=""CyvsfyjIS2tYwNtvuG03fA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM11"" LableResourceID=""SALARYITEM11""  Description=""地区差异补贴"" DataType=""string""  DataValue=""A9Hk9v4PImGbrrl88bG2AQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM12"" LableResourceID=""SALARYITEM12""  Description=""餐费补贴"" DataType=""string""  DataValue=""lnMNsa2ofXzqoaNFenTZ4w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM13"" LableResourceID=""SALARYITEM13""  Description=""固定收入合计"" DataType=""string""  DataValue=""m7zgJ9sX7FBgcsZ90COdMQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM14"" LableResourceID=""SALARYITEM14""  Description=""缺勤天数"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM15"" LableResourceID=""SALARYITEM15""  Description=""加班费"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM16"" LableResourceID=""SALARYITEM16""  Description=""值班津贴"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM17"" LableResourceID=""SALARYITEM17""  Description=""出勤工资"" DataType=""string""  DataValue=""m7zgJ9sX7FBgcsZ90COdMQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM18"" LableResourceID=""SALARYITEM18""  Description=""住房公积金扣款"" DataType=""string""  DataValue=""76kJBJMyBdcuvlX2c3Efwg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM19"" LableResourceID=""SALARYITEM19""  Description=""个人社保负担"" DataType=""string""  DataValue=""5aw1o8ZNwFA4lLH5twaAAw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM20"" LableResourceID=""SALARYITEM20""  Description=""税前应发合计"" DataType=""string""  DataValue=""m7zgJ9sX7FBgcsZ90COdMQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM21"" LableResourceID=""SALARYITEM21""  Description=""假期其它扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM22"" LableResourceID=""SALARYITEM22""  Description=""其它加扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM23"" LableResourceID=""SALARYITEM23""  Description=""绩效奖金"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM24"" LableResourceID=""SALARYITEM24""  Description=""纳税系数"" DataType=""string""  DataValue=""LWo3qQqyopqpUN6MfuaRqg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM25"" LableResourceID=""SALARYITEM25""  Description=""计税工资"" DataType=""string""  DataValue=""OwsYDqRGBRyhFkBgpxjTdQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM26"" LableResourceID=""SALARYITEM26""  Description=""扣税基数"" DataType=""string""  DataValue=""mcnLb8ttbSi7fY6sdBo7nw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM27"" LableResourceID=""SALARYITEM27""  Description=""差额"" DataType=""string""  DataValue=""b8KyNKm0BQdP20MOZ1f7+w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM28"" LableResourceID=""SALARYITEM28""  Description=""税率"" DataType=""string""  DataValue=""a6SjjHP67Qr9+9NL4w/EsQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM29"" LableResourceID=""SALARYITEM29""  Description=""速算扣除数"" DataType=""string""  DataValue=""Yp/l0GNnT6mku+jgIYRAkA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM30"" LableResourceID=""SALARYITEM30""  Description=""个人所得税"" DataType=""string""  DataValue=""/XUGnKgSdSc1EgMJdguTog=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM31"" LableResourceID=""SALARYITEM31""  Description=""考勤异常扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM32"" LableResourceID=""SALARYITEM32""  Description=""其它代扣款"" DataType=""string""  DataValue=""XRG6Giqi/IVGAJlSltQFxg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM33"" LableResourceID=""SALARYITEM33""  Description=""尾数扣款"" DataType=""string""  DataValue=""bHSSf2Ynwf0USnt300aXug=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM34"" LableResourceID=""SALARYITEM34""  Description=""扣款合计"" DataType=""string""  DataValue=""uo5nvGu8hO+DdY718CgPIA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""REMARK"" LableResourceID=""REMARK""  Description=""备注"" DataType=""string""  DataValue=""Nlf14UCBDY1c5/9MqYSksGBuiASPAVkeXcwoPb6JEPFJ9f8+b8AxeeSS50MHgFRp"" IsEncryption=""True""  DataText="""" /> 
</Object><Object Name=""SALARYRECORDBATCHDETAIL"" LableResourceID=""SALARYRECORDBATCHDETAIL"" Description=""月薪批量审核"" Key=""EMPLOYEESALARYRECORDID"" id=""bab645a5-42f0-49e5-a54c-2b212137308a"">
<Attribute Name=""DEPARTMENT"" LableResourceID=""DEPARTMENT""  Description=""部门"" DataType=""string""  DataValue=""工程管理部"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POST"" LableResourceID=""POST""  Description=""工作岗位"" DataType=""string""  DataValue=""水暖通工程师"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""EMPLOYEENAME"" LableResourceID=""EMPLOYEENAME""  Description=""姓名"" DataType=""string""  DataValue=""覃福泰"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POSTCODE"" LableResourceID=""POSTCODE""  Description=""职级代码"" DataType=""string""  DataValue=""水暖通工程师"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""ACTUALLYPAY"" LableResourceID=""ACTUALLYPAY""  Description=""实发金额"" DataType=""string""  DataValue=""b+eiKToDgvFXODijSMjqHQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""POSTLEVEL"" LableResourceID=""POSTLEVEL""  Description=""岗位级别"" DataType=""string""  DataValue=""12"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM6"" LableResourceID=""SALARYITEM6""  Description=""应发小计"" DataType=""string""  DataValue=""QmsqDccuEm3M34su5YecQw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM7"" LableResourceID=""SALARYITEM7""  Description=""基本工资"" DataType=""string""  DataValue=""WqZJOnNNqCvPrfswS89cgg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM8"" LableResourceID=""SALARYITEM8""  Description=""岗位工资"" DataType=""string""  DataValue=""SOorzGWJAOONOivlEFsdRA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM9"" LableResourceID=""SALARYITEM9""  Description=""保密津贴"" DataType=""string""  DataValue=""0ZOe+ZP8T3Zb3EbZ7cbdUg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM10"" LableResourceID=""SALARYITEM10""  Description=""住房补贴"" DataType=""string""  DataValue=""DdJ0c133svlt+NUwR+6Yaw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM11"" LableResourceID=""SALARYITEM11""  Description=""地区差异补贴"" DataType=""string""  DataValue=""GEV2t2WSpF9+XJ9OjEnQKA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM12"" LableResourceID=""SALARYITEM12""  Description=""餐费补贴"" DataType=""string""  DataValue=""lnMNsa2ofXzqoaNFenTZ4w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM13"" LableResourceID=""SALARYITEM13""  Description=""固定收入合计"" DataType=""string""  DataValue=""QmsqDccuEm3M34su5YecQw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM14"" LableResourceID=""SALARYITEM14""  Description=""缺勤天数"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM15"" LableResourceID=""SALARYITEM15""  Description=""加班费"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM16"" LableResourceID=""SALARYITEM16""  Description=""值班津贴"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM17"" LableResourceID=""SALARYITEM17""  Description=""出勤工资"" DataType=""string""  DataValue=""QmsqDccuEm3M34su5YecQw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM18"" LableResourceID=""SALARYITEM18""  Description=""住房公积金扣款"" DataType=""string""  DataValue=""76kJBJMyBdcuvlX2c3Efwg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM19"" LableResourceID=""SALARYITEM19""  Description=""个人社保负担"" DataType=""string""  DataValue=""SHo4k5JPx23Cgk8j1lxmoA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM20"" LableResourceID=""SALARYITEM20""  Description=""税前应发合计"" DataType=""string""  DataValue=""QmsqDccuEm3M34su5YecQw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM21"" LableResourceID=""SALARYITEM21""  Description=""假期其它扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM22"" LableResourceID=""SALARYITEM22""  Description=""其它加扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM23"" LableResourceID=""SALARYITEM23""  Description=""绩效奖金"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM24"" LableResourceID=""SALARYITEM24""  Description=""纳税系数"" DataType=""string""  DataValue=""LWo3qQqyopqpUN6MfuaRqg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM25"" LableResourceID=""SALARYITEM25""  Description=""计税工资"" DataType=""string""  DataValue=""dBGstt7zueuLa3abt3WX3w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM26"" LableResourceID=""SALARYITEM26""  Description=""扣税基数"" DataType=""string""  DataValue=""mcnLb8ttbSi7fY6sdBo7nw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM27"" LableResourceID=""SALARYITEM27""  Description=""差额"" DataType=""string""  DataValue=""l/CUyfNX2t0MmkDaEuFY+g=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM28"" LableResourceID=""SALARYITEM28""  Description=""税率"" DataType=""string""  DataValue=""qDySDZA5PwS10IHCyj4pIQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM29"" LableResourceID=""SALARYITEM29""  Description=""速算扣除数"" DataType=""string""  DataValue=""yFkkcABi5xQn+peJ6sAUiA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM30"" LableResourceID=""SALARYITEM30""  Description=""个人所得税"" DataType=""string""  DataValue=""sZFkWN9b7iO15zK83+JHGw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM31"" LableResourceID=""SALARYITEM31""  Description=""考勤异常扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM32"" LableResourceID=""SALARYITEM32""  Description=""其它代扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM33"" LableResourceID=""SALARYITEM33""  Description=""尾数扣款"" DataType=""string""  DataValue=""Kj9/oDNfgqL7FmfuA/agGA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM34"" LableResourceID=""SALARYITEM34""  Description=""扣款合计"" DataType=""string""  DataValue=""zqK+e92R5wsneD83eAxTPg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""REMARK"" LableResourceID=""REMARK""  Description=""备注"" DataType=""string""  DataValue=""8RYoJafePIrVJmsp3CH1cA=="" IsEncryption=""True""  DataText="""" /> 
</Object><Object Name=""SALARYRECORDBATCHDETAIL"" LableResourceID=""SALARYRECORDBATCHDETAIL"" Description=""月薪批量审核"" Key=""EMPLOYEESALARYRECORDID"" id=""bc8b6e7e-21d3-4df0-a9ea-0b1af9af9126"">
<Attribute Name=""DEPARTMENT"" LableResourceID=""DEPARTMENT""  Description=""部门"" DataType=""string""  DataValue=""综合管理部"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POST"" LableResourceID=""POST""  Description=""工作岗位"" DataType=""string""  DataValue=""人力行政经理"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""EMPLOYEENAME"" LableResourceID=""EMPLOYEENAME""  Description=""姓名"" DataType=""string""  DataValue=""徐翔"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POSTCODE"" LableResourceID=""POSTCODE""  Description=""职级代码"" DataType=""string""  DataValue=""人力行政经理"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""ACTUALLYPAY"" LableResourceID=""ACTUALLYPAY""  Description=""实发金额"" DataType=""string""  DataValue=""GKKHBykcfFxPGg5wnj3WTA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""POSTLEVEL"" LableResourceID=""POSTLEVEL""  Description=""岗位级别"" DataType=""string""  DataValue=""13"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM6"" LableResourceID=""SALARYITEM6""  Description=""应发小计"" DataType=""string""  DataValue=""4uNdMWjz/v/eacZXKJ+Oeg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM7"" LableResourceID=""SALARYITEM7""  Description=""基本工资"" DataType=""string""  DataValue=""qVJ/R8qvqtwn2DDDCyYhtw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM8"" LableResourceID=""SALARYITEM8""  Description=""岗位工资"" DataType=""string""  DataValue=""xo9+Kgz37Lf0v8dJZZDGOA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM9"" LableResourceID=""SALARYITEM9""  Description=""保密津贴"" DataType=""string""  DataValue=""8F5C4nrei8Tx34gQihVX4Q=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM10"" LableResourceID=""SALARYITEM10""  Description=""住房补贴"" DataType=""string""  DataValue=""GHbRE/sXXPSkWnfvhWZtTA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM11"" LableResourceID=""SALARYITEM11""  Description=""地区差异补贴"" DataType=""string""  DataValue=""JHrsKqlqOUXZcTNwxbeUNg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM12"" LableResourceID=""SALARYITEM12""  Description=""餐费补贴"" DataType=""string""  DataValue=""lnMNsa2ofXzqoaNFenTZ4w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM13"" LableResourceID=""SALARYITEM13""  Description=""固定收入合计"" DataType=""string""  DataValue=""1WJxvKC2lGrPRfl6se1YRQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM14"" LableResourceID=""SALARYITEM14""  Description=""缺勤天数"" DataType=""string""  DataValue=""tK9NEfQbyzDRRnIzC+DlZQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM15"" LableResourceID=""SALARYITEM15""  Description=""加班费"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM16"" LableResourceID=""SALARYITEM16""  Description=""值班津贴"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM17"" LableResourceID=""SALARYITEM17""  Description=""出勤工资"" DataType=""string""  DataValue=""4uNdMWjz/v/eacZXKJ+Oeg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM18"" LableResourceID=""SALARYITEM18""  Description=""住房公积金扣款"" DataType=""string""  DataValue=""76kJBJMyBdcuvlX2c3Efwg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM19"" LableResourceID=""SALARYITEM19""  Description=""个人社保负担"" DataType=""string""  DataValue=""SHo4k5JPx23Cgk8j1lxmoA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM20"" LableResourceID=""SALARYITEM20""  Description=""税前应发合计"" DataType=""string""  DataValue=""4uNdMWjz/v/eacZXKJ+Oeg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM21"" LableResourceID=""SALARYITEM21""  Description=""假期其它扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM22"" LableResourceID=""SALARYITEM22""  Description=""其它加扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM23"" LableResourceID=""SALARYITEM23""  Description=""绩效奖金"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM24"" LableResourceID=""SALARYITEM24""  Description=""纳税系数"" DataType=""string""  DataValue=""LWo3qQqyopqpUN6MfuaRqg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM25"" LableResourceID=""SALARYITEM25""  Description=""计税工资"" DataType=""string""  DataValue=""FlQCLLkIJptljSYzl2lymA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM26"" LableResourceID=""SALARYITEM26""  Description=""扣税基数"" DataType=""string""  DataValue=""mcnLb8ttbSi7fY6sdBo7nw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM27"" LableResourceID=""SALARYITEM27""  Description=""差额"" DataType=""string""  DataValue=""vT4mgyWXbDaImCk92YbifQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM28"" LableResourceID=""SALARYITEM28""  Description=""税率"" DataType=""string""  DataValue=""AzPp4W+AxGPty4kBmjd45w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM29"" LableResourceID=""SALARYITEM29""  Description=""速算扣除数"" DataType=""string""  DataValue=""Njt11fXCSEDT/RKlpg5U9g=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM30"" LableResourceID=""SALARYITEM30""  Description=""个人所得税"" DataType=""string""  DataValue=""VV+vfn/BjpWnz5GPbSs59g=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM31"" LableResourceID=""SALARYITEM31""  Description=""考勤异常扣款"" DataType=""string""  DataValue=""v/vrnw6ZhNZbh5MS8a/7DQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM32"" LableResourceID=""SALARYITEM32""  Description=""其它代扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM33"" LableResourceID=""SALARYITEM33""  Description=""尾数扣款"" DataType=""string""  DataValue=""w7bOFShskZ12sHWjy2C6Ow=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM34"" LableResourceID=""SALARYITEM34""  Description=""扣款合计"" DataType=""string""  DataValue=""UmEwJnTm/LrjjMmb3A02zg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""REMARK"" LableResourceID=""REMARK""  Description=""备注"" DataType=""string""  DataValue=""8RYoJafePIrVJmsp3CH1cA=="" IsEncryption=""True""  DataText="""" /> 
</Object><Object Name=""SALARYRECORDBATCHDETAIL"" LableResourceID=""SALARYRECORDBATCHDETAIL"" Description=""月薪批量审核"" Key=""EMPLOYEESALARYRECORDID"" id=""c1925edd-6899-4562-8778-84043687b30f"">
<Attribute Name=""DEPARTMENT"" LableResourceID=""DEPARTMENT""  Description=""部门"" DataType=""string""  DataValue=""工程管理部"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POST"" LableResourceID=""POST""  Description=""工作岗位"" DataType=""string""  DataValue=""工程管理部总监"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""EMPLOYEENAME"" LableResourceID=""EMPLOYEENAME""  Description=""姓名"" DataType=""string""  DataValue=""毕浔君"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POSTCODE"" LableResourceID=""POSTCODE""  Description=""职级代码"" DataType=""string""  DataValue=""工程管理部总监"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""ACTUALLYPAY"" LableResourceID=""ACTUALLYPAY""  Description=""实发金额"" DataType=""string""  DataValue=""0vxuJGrcqUJDInLfWyu1vg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""POSTLEVEL"" LableResourceID=""POSTLEVEL""  Description=""岗位级别"" DataType=""string""  DataValue=""8"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM6"" LableResourceID=""SALARYITEM6""  Description=""应发小计"" DataType=""string""  DataValue=""AjtP33D1mvVKm4gSpT4N8A=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM7"" LableResourceID=""SALARYITEM7""  Description=""基本工资"" DataType=""string""  DataValue=""mcnLb8ttbSi7fY6sdBo7nw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM8"" LableResourceID=""SALARYITEM8""  Description=""岗位工资"" DataType=""string""  DataValue=""nVXvDimCjYHtI5rctqk4Jg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM9"" LableResourceID=""SALARYITEM9""  Description=""保密津贴"" DataType=""string""  DataValue=""EQ8Hw+ujZruCaeFhLaMVsw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM10"" LableResourceID=""SALARYITEM10""  Description=""住房补贴"" DataType=""string""  DataValue=""8evaRiQrR73IKiv6TvsBSQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM11"" LableResourceID=""SALARYITEM11""  Description=""地区差异补贴"" DataType=""string""  DataValue=""8evaRiQrR73IKiv6TvsBSQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM12"" LableResourceID=""SALARYITEM12""  Description=""餐费补贴"" DataType=""string""  DataValue=""lnMNsa2ofXzqoaNFenTZ4w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM13"" LableResourceID=""SALARYITEM13""  Description=""固定收入合计"" DataType=""string""  DataValue=""AjtP33D1mvVKm4gSpT4N8A=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM14"" LableResourceID=""SALARYITEM14""  Description=""缺勤天数"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM15"" LableResourceID=""SALARYITEM15""  Description=""加班费"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM16"" LableResourceID=""SALARYITEM16""  Description=""值班津贴"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM17"" LableResourceID=""SALARYITEM17""  Description=""出勤工资"" DataType=""string""  DataValue=""AjtP33D1mvVKm4gSpT4N8A=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM18"" LableResourceID=""SALARYITEM18""  Description=""住房公积金扣款"" DataType=""string""  DataValue=""76kJBJMyBdcuvlX2c3Efwg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM19"" LableResourceID=""SALARYITEM19""  Description=""个人社保负担"" DataType=""string""  DataValue=""SHo4k5JPx23Cgk8j1lxmoA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM20"" LableResourceID=""SALARYITEM20""  Description=""税前应发合计"" DataType=""string""  DataValue=""AjtP33D1mvVKm4gSpT4N8A=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM21"" LableResourceID=""SALARYITEM21""  Description=""假期其它扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM22"" LableResourceID=""SALARYITEM22""  Description=""其它加扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM23"" LableResourceID=""SALARYITEM23""  Description=""绩效奖金"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM24"" LableResourceID=""SALARYITEM24""  Description=""纳税系数"" DataType=""string""  DataValue=""LWo3qQqyopqpUN6MfuaRqg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM25"" LableResourceID=""SALARYITEM25""  Description=""计税工资"" DataType=""string""  DataValue=""H50nnxfUOenVQBeZ+HikvQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM26"" LableResourceID=""SALARYITEM26""  Description=""扣税基数"" DataType=""string""  DataValue=""mcnLb8ttbSi7fY6sdBo7nw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM27"" LableResourceID=""SALARYITEM27""  Description=""差额"" DataType=""string""  DataValue=""EaI5whKzRXKVC5tRSzCF5w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM28"" LableResourceID=""SALARYITEM28""  Description=""税率"" DataType=""string""  DataValue=""AzPp4W+AxGPty4kBmjd45w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM29"" LableResourceID=""SALARYITEM29""  Description=""速算扣除数"" DataType=""string""  DataValue=""Njt11fXCSEDT/RKlpg5U9g=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM30"" LableResourceID=""SALARYITEM30""  Description=""个人所得税"" DataType=""string""  DataValue=""6Kd3ROlOgkqZ619DAKRaEw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM31"" LableResourceID=""SALARYITEM31""  Description=""考勤异常扣款"" DataType=""string""  DataValue=""kqYtHDijbP3yxAXR7Iapeg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM32"" LableResourceID=""SALARYITEM32""  Description=""其它代扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM33"" LableResourceID=""SALARYITEM33""  Description=""尾数扣款"" DataType=""string""  DataValue=""4ETeARo6LWQZnlro0nkK2A=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM34"" LableResourceID=""SALARYITEM34""  Description=""扣款合计"" DataType=""string""  DataValue=""8gT/ZjQYCAgbCvyfkQycPQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""REMARK"" LableResourceID=""REMARK""  Description=""备注"" DataType=""string""  DataValue=""8RYoJafePIrVJmsp3CH1cA=="" IsEncryption=""True""  DataText="""" /> 
</Object><Object Name=""SALARYRECORDBATCHDETAIL"" LableResourceID=""SALARYRECORDBATCHDETAIL"" Description=""月薪批量审核"" Key=""EMPLOYEESALARYRECORDID"" id=""db540951-a2c3-4731-bb4a-0cfd4ddc52c8"">
<Attribute Name=""DEPARTMENT"" LableResourceID=""DEPARTMENT""  Description=""部门"" DataType=""string""  DataValue=""总经办"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POST"" LableResourceID=""POST""  Description=""工作岗位"" DataType=""string""  DataValue=""副总经理"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""EMPLOYEENAME"" LableResourceID=""EMPLOYEENAME""  Description=""姓名"" DataType=""string""  DataValue=""王兵"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POSTCODE"" LableResourceID=""POSTCODE""  Description=""职级代码"" DataType=""string""  DataValue=""副总经理"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""ACTUALLYPAY"" LableResourceID=""ACTUALLYPAY""  Description=""实发金额"" DataType=""string""  DataValue=""w8xQ1TtU/xm2/ov9UxabCg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""POSTLEVEL"" LableResourceID=""POSTLEVEL""  Description=""岗位级别"" DataType=""string""  DataValue=""12"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM6"" LableResourceID=""SALARYITEM6""  Description=""应发小计"" DataType=""string""  DataValue=""0rWjiktkPbE12gbWtnyzjQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM7"" LableResourceID=""SALARYITEM7""  Description=""基本工资"" DataType=""string""  DataValue=""iCrBnplMJEVIfPYB++J9Mg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM8"" LableResourceID=""SALARYITEM8""  Description=""岗位工资"" DataType=""string""  DataValue=""77QR502tGBDtcFJzYwlbAA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM9"" LableResourceID=""SALARYITEM9""  Description=""保密津贴"" DataType=""string""  DataValue=""sEltM2JvKMr2ScDN0ALFLQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM10"" LableResourceID=""SALARYITEM10""  Description=""住房补贴"" DataType=""string""  DataValue=""k0RO8+5KKHu7r6anDHFDkw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM11"" LableResourceID=""SALARYITEM11""  Description=""地区差异补贴"" DataType=""string""  DataValue=""mUHZwyIyA9nRyb8L08Spog=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM12"" LableResourceID=""SALARYITEM12""  Description=""餐费补贴"" DataType=""string""  DataValue=""lnMNsa2ofXzqoaNFenTZ4w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM13"" LableResourceID=""SALARYITEM13""  Description=""固定收入合计"" DataType=""string""  DataValue=""0rWjiktkPbE12gbWtnyzjQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM14"" LableResourceID=""SALARYITEM14""  Description=""缺勤天数"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM15"" LableResourceID=""SALARYITEM15""  Description=""加班费"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM16"" LableResourceID=""SALARYITEM16""  Description=""值班津贴"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM17"" LableResourceID=""SALARYITEM17""  Description=""出勤工资"" DataType=""string""  DataValue=""0rWjiktkPbE12gbWtnyzjQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM18"" LableResourceID=""SALARYITEM18""  Description=""住房公积金扣款"" DataType=""string""  DataValue=""76kJBJMyBdcuvlX2c3Efwg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM19"" LableResourceID=""SALARYITEM19""  Description=""个人社保负担"" DataType=""string""  DataValue=""SHo4k5JPx23Cgk8j1lxmoA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM20"" LableResourceID=""SALARYITEM20""  Description=""税前应发合计"" DataType=""string""  DataValue=""0rWjiktkPbE12gbWtnyzjQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM21"" LableResourceID=""SALARYITEM21""  Description=""假期其它扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM22"" LableResourceID=""SALARYITEM22""  Description=""其它加扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM23"" LableResourceID=""SALARYITEM23""  Description=""绩效奖金"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM24"" LableResourceID=""SALARYITEM24""  Description=""纳税系数"" DataType=""string""  DataValue=""LWo3qQqyopqpUN6MfuaRqg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM25"" LableResourceID=""SALARYITEM25""  Description=""计税工资"" DataType=""string""  DataValue=""sAKrk5d/uwj1+x3Xib81gg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM26"" LableResourceID=""SALARYITEM26""  Description=""扣税基数"" DataType=""string""  DataValue=""mcnLb8ttbSi7fY6sdBo7nw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM27"" LableResourceID=""SALARYITEM27""  Description=""差额"" DataType=""string""  DataValue=""PPKRh4v2ZimNTg6l2frZQg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM28"" LableResourceID=""SALARYITEM28""  Description=""税率"" DataType=""string""  DataValue=""AzPp4W+AxGPty4kBmjd45w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM29"" LableResourceID=""SALARYITEM29""  Description=""速算扣除数"" DataType=""string""  DataValue=""Njt11fXCSEDT/RKlpg5U9g=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM30"" LableResourceID=""SALARYITEM30""  Description=""个人所得税"" DataType=""string""  DataValue=""ATyv9FKLlejNE5EOk+ubOg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM31"" LableResourceID=""SALARYITEM31""  Description=""考勤异常扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM32"" LableResourceID=""SALARYITEM32""  Description=""其它代扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM33"" LableResourceID=""SALARYITEM33""  Description=""尾数扣款"" DataType=""string""  DataValue=""4ETeARo6LWQZnlro0nkK2A=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM34"" LableResourceID=""SALARYITEM34""  Description=""扣款合计"" DataType=""string""  DataValue=""UmEwJnTm/LrjjMmb3A02zg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""REMARK"" LableResourceID=""REMARK""  Description=""备注"" DataType=""string""  DataValue=""8RYoJafePIrVJmsp3CH1cA=="" IsEncryption=""True""  DataText="""" /> 
</Object><Object Name=""SALARYRECORDBATCHDETAIL"" LableResourceID=""SALARYRECORDBATCHDETAIL"" Description=""月薪批量审核"" Key=""EMPLOYEESALARYRECORDID"" id=""f6e73874-5260-459e-a974-01ede59bc42a"">
<Attribute Name=""DEPARTMENT"" LableResourceID=""DEPARTMENT""  Description=""部门"" DataType=""string""  DataValue=""综合管理部"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POST"" LableResourceID=""POST""  Description=""工作岗位"" DataType=""string""  DataValue=""综合管理部总监"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""EMPLOYEENAME"" LableResourceID=""EMPLOYEENAME""  Description=""姓名"" DataType=""string""  DataValue=""杜伟"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POSTCODE"" LableResourceID=""POSTCODE""  Description=""职级代码"" DataType=""string""  DataValue=""综合管理部总监"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""ACTUALLYPAY"" LableResourceID=""ACTUALLYPAY""  Description=""实发金额"" DataType=""string""  DataValue=""bM8ydFXpgrNSfVvFxl+KhQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""POSTLEVEL"" LableResourceID=""POSTLEVEL""  Description=""岗位级别"" DataType=""string""  DataValue=""11"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM6"" LableResourceID=""SALARYITEM6""  Description=""应发小计"" DataType=""string""  DataValue=""pKakOuN69jTvuRZSkN66BQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM7"" LableResourceID=""SALARYITEM7""  Description=""基本工资"" DataType=""string""  DataValue=""nfFur+qYrvVyGGvrqOrOnQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM8"" LableResourceID=""SALARYITEM8""  Description=""岗位工资"" DataType=""string""  DataValue=""GWLgpvfJmo4TdVZeemSXNw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM9"" LableResourceID=""SALARYITEM9""  Description=""保密津贴"" DataType=""string""  DataValue=""AzbAHT/zLRwVq+cWIEuavw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM10"" LableResourceID=""SALARYITEM10""  Description=""住房补贴"" DataType=""string""  DataValue=""x7MbVnHcRPyrseQrsyw+wA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM11"" LableResourceID=""SALARYITEM11""  Description=""地区差异补贴"" DataType=""string""  DataValue=""ZubQna7nke4YmtSktJUjyw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM12"" LableResourceID=""SALARYITEM12""  Description=""餐费补贴"" DataType=""string""  DataValue=""lnMNsa2ofXzqoaNFenTZ4w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM13"" LableResourceID=""SALARYITEM13""  Description=""固定收入合计"" DataType=""string""  DataValue=""pKakOuN69jTvuRZSkN66BQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM14"" LableResourceID=""SALARYITEM14""  Description=""缺勤天数"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM15"" LableResourceID=""SALARYITEM15""  Description=""加班费"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM16"" LableResourceID=""SALARYITEM16""  Description=""值班津贴"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM17"" LableResourceID=""SALARYITEM17""  Description=""出勤工资"" DataType=""string""  DataValue=""pKakOuN69jTvuRZSkN66BQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM18"" LableResourceID=""SALARYITEM18""  Description=""住房公积金扣款"" DataType=""string""  DataValue=""76kJBJMyBdcuvlX2c3Efwg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM19"" LableResourceID=""SALARYITEM19""  Description=""个人社保负担"" DataType=""string""  DataValue=""SHo4k5JPx23Cgk8j1lxmoA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM20"" LableResourceID=""SALARYITEM20""  Description=""税前应发合计"" DataType=""string""  DataValue=""pKakOuN69jTvuRZSkN66BQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM21"" LableResourceID=""SALARYITEM21""  Description=""假期其它扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM22"" LableResourceID=""SALARYITEM22""  Description=""其它加扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM23"" LableResourceID=""SALARYITEM23""  Description=""绩效奖金"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM24"" LableResourceID=""SALARYITEM24""  Description=""纳税系数"" DataType=""string""  DataValue=""LWo3qQqyopqpUN6MfuaRqg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM25"" LableResourceID=""SALARYITEM25""  Description=""计税工资"" DataType=""string""  DataValue=""IEgRF7qUiC6AgYlQ3pI6gw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM26"" LableResourceID=""SALARYITEM26""  Description=""扣税基数"" DataType=""string""  DataValue=""mcnLb8ttbSi7fY6sdBo7nw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM27"" LableResourceID=""SALARYITEM27""  Description=""差额"" DataType=""string""  DataValue=""hlGtaCFEfbdIQMWZTlbB0A=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM28"" LableResourceID=""SALARYITEM28""  Description=""税率"" DataType=""string""  DataValue=""a6SjjHP67Qr9+9NL4w/EsQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM29"" LableResourceID=""SALARYITEM29""  Description=""速算扣除数"" DataType=""string""  DataValue=""Yp/l0GNnT6mku+jgIYRAkA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM30"" LableResourceID=""SALARYITEM30""  Description=""个人所得税"" DataType=""string""  DataValue=""TpNUnIF70uyGGLUaxNccPA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM31"" LableResourceID=""SALARYITEM31""  Description=""考勤异常扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM32"" LableResourceID=""SALARYITEM32""  Description=""其它代扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM33"" LableResourceID=""SALARYITEM33""  Description=""尾数扣款"" DataType=""string""  DataValue=""y9wCJcGW/ky2FoHndXKEAw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM34"" LableResourceID=""SALARYITEM34""  Description=""扣款合计"" DataType=""string""  DataValue=""1jUClz3TwJg4Cm5YKCd8dQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""REMARK"" LableResourceID=""REMARK""  Description=""备注"" DataType=""string""  DataValue=""8RYoJafePIrVJmsp3CH1cA=="" IsEncryption=""True""  DataText="""" /> 
</Object><Object Name=""SALARYRECORDBATCHDETAIL"" LableResourceID=""SALARYRECORDBATCHDETAIL"" Description=""月薪批量审核"" Key=""EMPLOYEESALARYRECORDID"" id=""f9035f9f-e8d7-4c3e-b38f-8c4b1c1ce00e"">
<Attribute Name=""DEPARTMENT"" LableResourceID=""DEPARTMENT""  Description=""部门"" DataType=""string""  DataValue=""财务部"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POST"" LableResourceID=""POST""  Description=""工作岗位"" DataType=""string""  DataValue=""财务经理"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""EMPLOYEENAME"" LableResourceID=""EMPLOYEENAME""  Description=""姓名"" DataType=""string""  DataValue=""蓝鸣"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""POSTCODE"" LableResourceID=""POSTCODE""  Description=""职级代码"" DataType=""string""  DataValue=""财务经理"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""ACTUALLYPAY"" LableResourceID=""ACTUALLYPAY""  Description=""实发金额"" DataType=""string""  DataValue=""sFYN9DcDyX9M/TBlEEg4qQ=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""POSTLEVEL"" LableResourceID=""POSTLEVEL""  Description=""岗位级别"" DataType=""string""  DataValue=""13"" IsEncryption=""False""  DataText="""" /> 
<Attribute Name=""SALARYITEM6"" LableResourceID=""SALARYITEM6""  Description=""应发小计"" DataType=""string""  DataValue=""xR9JKpfyQshnD8nbTrkBhg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM7"" LableResourceID=""SALARYITEM7""  Description=""基本工资"" DataType=""string""  DataValue=""cfSzI1qwohuvMoBa5Ouslg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM8"" LableResourceID=""SALARYITEM8""  Description=""岗位工资"" DataType=""string""  DataValue=""yYYzqDJJNocpZPpkSsuGyA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM9"" LableResourceID=""SALARYITEM9""  Description=""保密津贴"" DataType=""string""  DataValue=""8UttihCCpS5NHXVwz+p+eA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM10"" LableResourceID=""SALARYITEM10""  Description=""住房补贴"" DataType=""string""  DataValue=""JHrsKqlqOUXZcTNwxbeUNg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM11"" LableResourceID=""SALARYITEM11""  Description=""地区差异补贴"" DataType=""string""  DataValue=""JHrsKqlqOUXZcTNwxbeUNg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM12"" LableResourceID=""SALARYITEM12""  Description=""餐费补贴"" DataType=""string""  DataValue=""lnMNsa2ofXzqoaNFenTZ4w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM13"" LableResourceID=""SALARYITEM13""  Description=""固定收入合计"" DataType=""string""  DataValue=""xR9JKpfyQshnD8nbTrkBhg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM14"" LableResourceID=""SALARYITEM14""  Description=""缺勤天数"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM15"" LableResourceID=""SALARYITEM15""  Description=""加班费"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM16"" LableResourceID=""SALARYITEM16""  Description=""值班津贴"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM17"" LableResourceID=""SALARYITEM17""  Description=""出勤工资"" DataType=""string""  DataValue=""xR9JKpfyQshnD8nbTrkBhg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM18"" LableResourceID=""SALARYITEM18""  Description=""住房公积金扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM19"" LableResourceID=""SALARYITEM19""  Description=""个人社保负担"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM20"" LableResourceID=""SALARYITEM20""  Description=""税前应发合计"" DataType=""string""  DataValue=""xR9JKpfyQshnD8nbTrkBhg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM21"" LableResourceID=""SALARYITEM21""  Description=""假期其它扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM22"" LableResourceID=""SALARYITEM22""  Description=""其它加扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM23"" LableResourceID=""SALARYITEM23""  Description=""绩效奖金"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM24"" LableResourceID=""SALARYITEM24""  Description=""纳税系数"" DataType=""string""  DataValue=""LWo3qQqyopqpUN6MfuaRqg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM25"" LableResourceID=""SALARYITEM25""  Description=""计税工资"" DataType=""string""  DataValue=""xR9JKpfyQshnD8nbTrkBhg=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM26"" LableResourceID=""SALARYITEM26""  Description=""扣税基数"" DataType=""string""  DataValue=""mcnLb8ttbSi7fY6sdBo7nw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM27"" LableResourceID=""SALARYITEM27""  Description=""差额"" DataType=""string""  DataValue=""eClRfivhsVyUxSa+QmsAHw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM28"" LableResourceID=""SALARYITEM28""  Description=""税率"" DataType=""string""  DataValue=""AzPp4W+AxGPty4kBmjd45w=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM29"" LableResourceID=""SALARYITEM29""  Description=""速算扣除数"" DataType=""string""  DataValue=""Njt11fXCSEDT/RKlpg5U9g=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM30"" LableResourceID=""SALARYITEM30""  Description=""个人所得税"" DataType=""string""  DataValue=""N5LI0scgihPiS7AlVbeXrw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM31"" LableResourceID=""SALARYITEM31""  Description=""考勤异常扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM32"" LableResourceID=""SALARYITEM32""  Description=""其它代扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM33"" LableResourceID=""SALARYITEM33""  Description=""尾数扣款"" DataType=""string""  DataValue=""/7TVBtjdy5M5+ebIHQ1qsA=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""SALARYITEM34"" LableResourceID=""SALARYITEM34""  Description=""扣款合计"" DataType=""string""  DataValue=""N5LI0scgihPiS7AlVbeXrw=="" IsEncryption=""True""  DataText="""" /> 
<Attribute Name=""REMARK"" LableResourceID=""REMARK""  Description=""备注"" DataType=""string""  DataValue=""8RYoJafePIrVJmsp3CH1cA=="" IsEncryption=""True""  DataText="""" /> 
</Object>
    </ObjectList>
  </Object>
</System>";
            List<SMT.Workflow.Common.Model.FlowEngine.CustomUserMsg> userform = new List<SMT.Workflow.Common.Model.FlowEngine.CustomUserMsg>();
            //userform.Add(new SMT.Workflow.Common.Model.FlowEngine.CustomUserMsg() { FormID = ""3b86608a-0430-470c-a7fd-2d6af2195a9b"", UserID = ""9239b310-c7ee-4eb2-843e-08febc38a398"" });
            //cm=c34d0840-47f9-4450-9197-3a49227eef22  05cd6f06-4e25-4590-966f-31e0b1e94cbc  T_VM_VEHICLEOILCPAYCFM
            userform.Add(new Common.Model.FlowEngine.CustomUserMsg() { UserID = "c34d0840-47f9-4450-9197-3a49227eef22|05cd6f06-4e25-4590-966f-31e0b1e94cbc" , FormID=xml});
            dd.SendTaskMessage(userform, "HR", "T_HR_EMPLOYEESALARYRECORD");
            //dd.ReturnUserInfoDask("c34d0840-47f9-4450-9197-3a49227eef22","05cd6f06-4e25-4590-966f-31e0b1e94cbc","VM","T_VM_VEHICLEOILCPAYCFM","",xml,SMT.Global.IEngineContract.MsgType.Task);
            //dd.ApplicationEngineTrigger(userform, ""VM"", ""T_VM_VEHICLEOFFENCE"", ""c34d0840-47f9-4450-9197-3a49227eef22"", xml,SMT.Global.IEngineContract.MsgType.Task);
        }
    }
}