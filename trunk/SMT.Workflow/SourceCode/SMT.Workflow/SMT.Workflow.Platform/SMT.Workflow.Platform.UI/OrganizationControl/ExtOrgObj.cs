
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;
using PersonnelWS = SMT.Saas.Tools.PersonnelWS;

namespace SMT.Workflow.Platform.UI.OrganizationControl
{
    public class ExtOrgObj
    {
        private string objectID = "";
        private string objectName = "";
        private OrgTreeItemTypes objType ;

        public OrgTreeItemTypes ObjectType
        {
            get
            {
                if (ObjectInstance is OrganizationWS.T_HR_COMPANY)
                    return OrgTreeItemTypes.Company;
                else if (ObjectInstance is OrganizationWS.T_HR_DEPARTMENT)
                    return OrgTreeItemTypes.Department;
                else if (ObjectInstance is OrganizationWS.T_HR_POST)
                    return OrgTreeItemTypes.Post;
                else if (ObjectInstance is PersonnelWS.T_HR_EMPLOYEE)
                    return OrgTreeItemTypes.Personnel;
                else
                    return objType;
            }
            set 
            {
                objType = value; 
            }
        }

        public string ObjectID
        {
            get
            {
                if (ObjectInstance is OrganizationWS.T_HR_COMPANY)
                    return ((OrganizationWS.T_HR_COMPANY)ObjectInstance).COMPANYID;
                else if (ObjectInstance is OrganizationWS.T_HR_DEPARTMENT)
                    return ((OrganizationWS.T_HR_DEPARTMENT)ObjectInstance).DEPARTMENTID;
                else if (ObjectInstance is OrganizationWS.T_HR_POST)
                    return ((OrganizationWS.T_HR_POST)ObjectInstance).POSTID;
                else if (ObjectInstance is PersonnelWS.T_HR_EMPLOYEE)
                    return ((PersonnelWS.T_HR_EMPLOYEE)ObjectInstance).EMPLOYEEID;
                else
                    return objectID;
            }
            set 
            {
                objectID = value;
            }

        }

        public string ObjectName
        {
            get
            {
                if (ObjectInstance is OrganizationWS.T_HR_COMPANY)
                    return ((OrganizationWS.T_HR_COMPANY)ObjectInstance).CNAME;
                else if (ObjectInstance is OrganizationWS.T_HR_DEPARTMENT)
                    return ((OrganizationWS.T_HR_DEPARTMENT)ObjectInstance).T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                else if (ObjectInstance is OrganizationWS.T_HR_POST)
                    return ((OrganizationWS.T_HR_POST)ObjectInstance).T_HR_POSTDICTIONARY.POSTNAME;
                else if (ObjectInstance is PersonnelWS.T_HR_EMPLOYEE)
                    return ((PersonnelWS.T_HR_EMPLOYEE)ObjectInstance).EMPLOYEECNAME;
                else
                    return objectName;
            }
            set
            {
                objectName = value;
            }
        }



        public object ObjectInstance { get; set; }

        public object ParentObject { get; set; }
    }
}
