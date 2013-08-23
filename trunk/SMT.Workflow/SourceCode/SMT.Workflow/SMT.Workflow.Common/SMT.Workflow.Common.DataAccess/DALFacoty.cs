using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.Workflow.Common.DataAccess
{
    public class DALFacoty
    {
        public static IDAO Create(string connectionstring)
        {
            IDAO dao;
            string connstring = "";
            string[] connectionArray = connectionstring.Split(':');

            if (connectionArray.Length == 1)
            {
                connstring = connectionArray[0];
                dao = new OracleDAO(connstring);
            }
            else
            {
                connstring = connectionArray[1];

                switch (connectionArray[0].ToUpper())
                {
                    case "ORACLE":
                        dao = new OracleDAO(connstring);
                        break;
                    case "SQLSERVER":
                        dao = new SqlServerDAO(connstring);
                        break; 
                    default:
                        dao = new OracleDAO(connstring);
                        break;
                }
            }

            return dao;
        }
    }
}
