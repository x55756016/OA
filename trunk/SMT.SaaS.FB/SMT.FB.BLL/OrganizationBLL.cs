using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT_FB_EFModel;
using OrganizationWS = SMT.SaaS.BLLCommonServices.OrganizationWS;
using SMT.SaaS.BLLCommonServices;
using EngineWS = SMT.SaaS.BLLCommonServices.EngineConfigWS;
using PersonnelWS = SMT.SaaS.BLLCommonServices.PersonnelWS;
using System.Reflection;

namespace SMT.FB.BLL
{
    public class OrganizationBLL 
    {
        //public static List<VirtualCompany> listC=new List<VirtualCompany>();
        //public static List<VirtualDepartment> listDepartment=new List<VirtualDepartment>();
        //public static List<VirtualPost> listPost=new List<VirtualPost>();
        private OrganizationWS.OrganizationServiceClient organizationService;
        public OrganizationBLL()
        {
            organizationService = new OrganizationWS.OrganizationServiceClient();
        }

        public List<FBEntity> GetCompany(QueryExpression qe)
        {
            //string strType=qe.QueryType;
            //switch (strType)
            //{
            //    case "T_FB_SUBJECTDEPTMENT":
            //        listC = InnerGetCompany(qe);
            //        break;
            //    case "T_FB_SUBJECTCOMPANY":
            //        listC = InnerGetCompany(qe);
            //        break;
            //    case "T_FB_SUBJECTPOST":
            //        listC = InnerGetCompany(qe);
            //        break;
            //    case "T_FB_SUBJECTCOMPANYSET":
            //        listC = InnerGetCompany(qe);
            //        break;
            //    default:
            //        break;
            //}
            List<VirtualCompany> listC = InnerGetCompany(qe);
            List<FBEntity> listResult = listC.ToFBEntityList();
            listResult.ForEach(company =>
            {
                VirtualCompany vcCompany = company.Entity as VirtualCompany;
                List<FBEntity> listDepartMent = vcCompany.DepartmentCollection.ToFBEntityList();
                RelationManyEntity rme = new RelationManyEntity();
                rme.EntityType = typeof(VirtualDepartment).Name;
                rme.FBEntities = listDepartMent;
                company.CollectionEntity.Add(rme);

                listDepartMent.ForEach(department =>
                {
                    VirtualDepartment virtualDepartment = department.Entity as VirtualDepartment;
                    List<FBEntity> listPost = virtualDepartment.PostCollection.ToFBEntityList();
                    RelationManyEntity rmePost = new RelationManyEntity();
                    rmePost.EntityType = typeof(VirtualPost).Name;
                    rmePost.FBEntities = listPost;
                    department.CollectionEntity.Add(rmePost);
                });
            });
            return listResult;
        }
        public List<VirtualCompany> InnerGetCompany(QueryExpression qe)
        {
            try
            {

                string action = ((int)Utility.Permissions.Browse).ToString();
                
                List<OrganizationWS.T_HR_COMPANY> comList = organizationService.GetCompanyByEntityPerm(qe.VisitUserID, action, qe.VisitModuleCode).ToList();
                List<OrganizationWS.V_DEPARTMENT> deptList = organizationService.GetDepartmentView(qe.VisitUserID, action, qe.VisitModuleCode).ToList();
                List<OrganizationWS.V_POST> vpostList = organizationService.GetPostView(qe.VisitUserID, action, qe.VisitModuleCode).ToList();
                //有效的部门和岗位
                deptList = deptList.Where(s => s.EDITSTATE == "1").ToList();
                vpostList = vpostList.Where(s => s.EDITSTATE == "1").ToList();
                List<OrganizationWS.T_HR_POST> postList = new List<OrganizationWS.T_HR_POST>();
                try
                {
                    foreach (var ent in vpostList)
                    {
                        OrganizationWS.T_HR_POST pt = new OrganizationWS.T_HR_POST();
                        pt.POSTID = ent.POSTID;
                        pt.FATHERPOSTID = ent.FATHERPOSTID;
                        pt.CHECKSTATE = ent.CHECKSTATE;
                        pt.EDITSTATE = ent.EDITSTATE;

                        pt.T_HR_POSTDICTIONARY = new OrganizationWS.T_HR_POSTDICTIONARY();
                        pt.T_HR_POSTDICTIONARY.POSTDICTIONARYID = Guid.NewGuid().ToString();
                        pt.T_HR_POSTDICTIONARY.POSTNAME = ent.POSTNAME;

                        pt.T_HR_DEPARTMENT = new OrganizationWS.T_HR_DEPARTMENT();
                        try
                        {
                            string strDepartmentid = deptList.Where(s => s.DEPARTMENTID == ent.DEPARTMENTID).FirstOrDefault().DEPARTMENTID;
                            pt.T_HR_DEPARTMENT.DEPARTMENTID = strDepartmentid;
                            postList.Add(pt);
                        }
                        catch (Exception ex)
                        {
                            SystemBLL.Debug("当前员工："+qe.VisitUserID+"查询岗位所属部门错误，岗位名称 " + ent.POSTNAME + "岗位id" + ent.POSTID + "没找到所属部门，可能是权限不足导致");
                        }
                       

                       
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }


                List<VirtualCompany> listCompany = new List<VirtualCompany>();
                try
                {
                    comList.ForEach(comHR =>
                    {
                        List<VirtualDepartment> listDepartment = new List<VirtualDepartment>();

                       
                            VirtualCompany vc = new VirtualCompany();
                            vc.ID = comHR.COMPANYID;
                            vc.Name = comHR.CNAME;
                            vc.DepartmentCollection = listDepartment;
                            listCompany.Add(vc);

                            List<OrganizationWS.V_DEPARTMENT> deptListPart = deptList.FindAll(item =>
                            {
                                if (item.COMPANYID == null)
                                {
                                    return false;
                                }
                                return item.COMPANYID == comHR.COMPANYID;

                            });
                        deptListPart.ForEach(deptHR =>
                        {
                            try
                            {
                                List<VirtualPost> listPost = new List<VirtualPost>();

                                VirtualDepartment vd = new VirtualDepartment();
                                vd.ID = deptHR.DEPARTMENTID;
                                vd.Name = deptHR.DEPARTMENTNAME;
                                vd.VirtualCompany = vc;
                                vd.PostCollection = listPost;
                                listDepartment.Add(vd);

                                List<OrganizationWS.T_HR_POST> postListPart = postList.FindAll(item =>
                                {
                                    if (item.T_HR_DEPARTMENT.DEPARTMENTID == null)
                                    {
                                        return false;
                                    }
                                    return item.T_HR_DEPARTMENT.DEPARTMENTID == deptHR.DEPARTMENTID;
                                });
                                postListPart.ForEach(postHR =>
                                {
                                    try
                                    {
                                        VirtualPost vp = new VirtualPost();
                                        vp.ID = postHR.POSTID;
                                        vp.Name = postHR.T_HR_POSTDICTIONARY.POSTNAME;
                                        vp.VirtualCompany = vc;
                                        vp.VirtualDepartment = vd;
                                        listPost.Add(vp);
                                    }
                                    catch (Exception ex)
                                    {
                                        throw ex;
                                    }
                                });
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }

                        });


                    });
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                return listCompany;
            }
            catch (Exception ex)
            {
                SystemBLL.Debug(ex.ToString());
                throw new Exception ("调用HR服务异常", ex);
            }
        }

        internal List<VirtualCompany> GetVirtualCompany(QueryExpression qe)
        {
            List<VirtualCompany> listCompany = new List<VirtualCompany>();
            string action = ((int)Utility.Permissions.Browse).ToString();
            string ModuleCode = qe.VisitModuleCode;
            if (qe.VisitModuleCode == "T_FB_SUBJECTCOMPANY_COMPANY")
            {
                ModuleCode = "T_FB_SUBJECTCOMPANY";
            }
            List<string> CompanyIds = new List<string>();
            //在公司科目维护中去掉以下2个公司
            CompanyIds.Add("427eb67d-35b4-47a9-9609-baf5355d2ed5");//深圳市神州通投资集团有限公司
            CompanyIds.Add("7cd6c0a4-9735-476a-9184-103b962d3383");//初始化公司
            List<OrganizationWS.T_HR_COMPANY> comList = organizationService.GetCompanyByEntityPerm(qe.VisitUserID, action, ModuleCode).ToList();

            comList.ForEach(comHR =>
            {
                VirtualCompany vc = new VirtualCompany();
                vc.ID = comHR.COMPANYID;
                vc.Name = comHR.CNAME;
                if (ModuleCode == "T_FB_SUBJECTCOMPANY" || ModuleCode == "T_FB_SUBJECTCOMPANYSET")
                {
                    if (!(CompanyIds.IndexOf(vc.ID) > -1))
                    {
                        listCompany.Add(vc);
                    }
                }
                else
                {
                    listCompany.Add(vc);
                }
                
            });
            return listCompany;
        }
        internal List<VirtualDepartment> GetVirtualDepartment(QueryExpression qe)
        {
            List<VirtualDepartment> listDepartment = new List<VirtualDepartment>();
            string action = ((int)Utility.Permissions.Browse).ToString();
            //if (listDepartment.Count == 0)
            //{
                List<OrganizationWS.V_DEPARTMENT> deptList = organizationService.GetDepartmentView(qe.VisitUserID, action, qe.VisitModuleCode).ToList();

                deptList.ForEach(deptHR =>
                {
                    VirtualDepartment vd = new VirtualDepartment();
                    vd.ID = deptHR.DEPARTMENTID;
                    vd.Name = deptHR.DEPARTMENTNAME;
                    listDepartment.Add(vd);
                });              
            //}
            return listDepartment;
        }
        internal List<VirtualPost> GetVirtualPost(QueryExpression qe)
        {
            List<VirtualPost> listPost = new List<VirtualPost>();
            string action = ((int)Utility.Permissions.Browse).ToString();
            //if (listPost.Count == 0)
            //{
                List<OrganizationWS.T_HR_POST> postList = organizationService.GetPostByEntityPerm(qe.VisitUserID, action, qe.VisitModuleCode).ToList();

                postList.ForEach(postHR =>
                {
                    VirtualPost vp = new VirtualPost();
                    vp.ID = postHR.POSTID;
                    vp.Name = postHR.T_HR_POSTDICTIONARY.POSTNAME;
                    listPost.Add(vp);
                });
            //}
            return listPost;
        }
        internal List<VirtualUser> GetVirtualUser(List<string> userIDs)
        {
            List<VirtualUser> listUser = new List<VirtualUser>();
            if (userIDs.Count == 0)
            {
                return listUser;
            }
            try
            {
                PersonnelWS.PersonnelServiceClient personService = new PersonnelWS.PersonnelServiceClient();

                List<PersonnelWS.T_HR_EMPLOYEE> employeeList = personService.GetEmployeeByIDs(userIDs.ToArray()).ToList();

                employeeList.ForEach(employeeHR =>
                {
                    VirtualUser vu = new VirtualUser();
                    vu.ID = employeeHR.EMPLOYEEID;
                    vu.Name = employeeHR.EMPLOYEECNAME + "(" + employeeHR.EMPLOYEECODE + ")";
                    listUser.Add(vu);
                });
            }
            catch ( Exception ex)
            {
                SystemBLL.Debug(ex.ToString());
            }
            return listUser;

        }

        public List<VirtualCompany> InitData()
        {           
            
            string userID = "";
            
            List<OrganizationWS.T_HR_COMPANY> comList = organizationService.GetCompanyActived(userID).ToList();
            List<OrganizationWS.T_HR_DEPARTMENT> deptList = organizationService.GetDepartmentActived(userID).ToList();
            List<OrganizationWS.T_HR_POST> postList = organizationService.GetPostActived(userID).ToList();

            List<VirtualCompany> listCompany = new List<VirtualCompany>();
            comList.ForEach(comHR =>
                {
                    List<VirtualDepartment> listDepartment = new List<VirtualDepartment>();

                    VirtualCompany vc = new VirtualCompany();
                    vc.ID = comHR.COMPANYID;
                    vc.Name = comHR.CNAME;
                    vc.DepartmentCollection = listDepartment;
                    listCompany.Add(vc);

                    List<OrganizationWS.T_HR_DEPARTMENT> deptListPart = deptList.FindAll(item =>
                        {
                            return item.T_HR_COMPANY.COMPANYID == comHR.COMPANYID;
                        });
                    
                    deptListPart.ForEach(deptHR =>
                        {
                            List<VirtualPost> listPost = new List<VirtualPost>();

                            VirtualDepartment vd = new VirtualDepartment();
                            vd.ID = deptHR.DEPARTMENTID;
                            vd.Name = deptHR.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                            vd.VirtualCompany = vc;
                            vd.PostCollection = listPost;
                            listDepartment.Add(vd);

                            List<OrganizationWS.T_HR_POST> postListPart = postList.FindAll(item =>
                                {
                                    return item.T_HR_DEPARTMENT.DEPARTMENTID == deptHR.DEPARTMENTID;
                                });
                            postListPart.ForEach(postHR =>
                                {
                                    VirtualPost vp = new VirtualPost();
                                    vp.ID = postHR.POSTID;
                                    vp.Name = postHR.T_HR_POSTDICTIONARY.POSTNAME;
                                    vp.VirtualCompany = vc;
                                    vp.VirtualDepartment = vd;
                                    listPost.Add(vp);
                                });
                        });

                   
                });
            return listCompany;
        }

        //处理岗位信息一栏
        public List<T_FB_PERSONMONEYASSIGNDETAIL> UpdatePostInfo(List<T_FB_PERSONMONEYASSIGNDETAIL> listperson)
        {
            PersonnelWS.PersonnelServiceClient pe=new PersonnelWS.PersonnelServiceClient ();
            PersonnelWS.V_EMPLOYEEPOSTFORFB[] vlistpostinfo = new PersonnelWS.V_EMPLOYEEPOSTFORFB[listperson.Count];
            int i = 0;
            listperson.ForEach(item =>
                {
                    PersonnelWS.V_EMPLOYEEPOSTFORFB vpostinfo = new PersonnelWS.V_EMPLOYEEPOSTFORFB();
                    vpostinfo.PERSONBUDGETAPPLYDETAILID = item.PERSONBUDGETAPPLYDETAILID;
                    vpostinfo.OWNERID = item.OWNERID;
                    vpostinfo.OWNERPOSTID = item.OWNERPOSTID;
                    vlistpostinfo[i]=vpostinfo;
                    i++;
                });
            try
            {
                vlistpostinfo = pe.GetEmployeeListForFB(vlistpostinfo) as PersonnelWS.V_EMPLOYEEPOSTFORFB[];
                               
            }
            catch (Exception ex)
            {
                SystemBLL.Debug("调用HR服务异常 GetEmployeeListForFB " + ex.ToString());  
            }

            listperson.ForEach(item =>
            {
                var person = vlistpostinfo.Where(p => p.PERSONBUDGETAPPLYDETAILID == item.PERSONBUDGETAPPLYDETAILID ).FirstOrDefault();
                if (person != null)
                {
                    item.SUGGESTBUDGETMONEY = person.SUM;
                    switch (person.EMPLOYEESTATE)
                    {
                        case "0"://试用期
                            item.POSTINFO = "试用期，请注意";
                            break;
                        case "1"://在职
                            item.POSTINFO = string.Empty;
                            break;
                        case "2"://已离职
                            item.POSTINFO = "已离职，请删除";
                            break;
                        case "3"://离职中
                            item.POSTINFO = "离职中，请注意";
                            break;
                        case "4"://未入职
                            item.POSTINFO = "未入职，请删除";
                            break;
                        case "10"://异动中
                            item.POSTINFO = "异动中，请异动后再处理";
                            break;
                        case "11"://异动过
                            item.POSTINFO = "异动过，已转换成新岗位";

                            item.OWNERPOSTID = person.NEWPOSTID;
                            item.OWNERPOSTNAME  = person.NEWPOSTNAME;
                            item.OWNERDEPARTMENTID = person.NEWDEPARTMENTID;
                            item.OWNERDEPARTMENTNAME  = person.NEWDEPARTMENTNAME;
                            item.OWNERCOMPANYID = person.NEWCOMPANYID;
                            item.OWNERCOMPANYNAME = person.NEWCOMPANYNAME;
                            break;
                        case "12"://岗位异常
                            item.POSTINFO = "岗位异常，请删除后再选择";
                            break;
                        default:
                            item.POSTINFO = string.Empty;
                            break;
                    }

                    switch (person.ISAGENCY)
                    {
                        case "0"://主岗位
                          // item.POSTINFO += string.Empty;
                            break;
                        case "1"://赚职
                            if (item.POSTINFO != string.Empty && person.EMPLOYEESTATE!="1")
                                item.POSTINFO = item.POSTINFO.Insert(0, "兼职，");
                            else
                                item.POSTINFO = "兼职";
                            break;
                        default:
                           // item.POSTINFO += string.Empty;
                            break;
                    }
                }
            });
           return listperson;
        }

     

       
    }
}
