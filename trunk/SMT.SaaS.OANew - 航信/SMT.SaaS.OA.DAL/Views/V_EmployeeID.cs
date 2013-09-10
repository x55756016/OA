using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.SaaS.OA.DAL.Views
{
    public class V_EmployeeID
    {
        private int employeeIDCode;
        public int EmployeeIDCode
        {
            get { return employeeIDCode; }
            set { employeeIDCode = value; }
        }
        private string employeeID;
        public string EmployeeID
        {
            get { return employeeID; }
            set { employeeID = value; }
        }
        private string employeeName;
        public string EmployeeName
        {
            get { return employeeName; }
            set { employeeName = value; }
        }
        private string employeeCompanyID;
        public string EmployeeCompanyID
        {
            get { return employeeCompanyID; }
            set { employeeCompanyID = value; }
        }
        private string employeeDepartmentID;
        public string EmployeeDepartmentID
        {
            get { return employeeDepartmentID; }
            set { employeeDepartmentID = value; }
        }
        private string employeePostID;
        public string EmployeePostID
        {
            get { return employeePostID; }
            set { employeePostID = value; }
        }
    }
}
