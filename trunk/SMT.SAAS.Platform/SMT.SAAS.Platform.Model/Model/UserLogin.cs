
namespace SMT.SAAS.Platform.Model
{
    public class UserLogin
    {
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public string EmployeeID { get; set; }
        public string IsManager { get; set; }
        public string LoginRecordID { get; set; }
        public string SysUserID { get; set; }
        public string[] SystemCode { get; set; }
    }
}
