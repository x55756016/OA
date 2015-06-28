using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace TM_SaaS_OA_EFModel
{
    public class ConsoleProgame
    {
        public static void Main(){
            using (TM_SaaS_OA_EFModelContext context = new TM_SaaS_OA_EFModelContext())
            {
                var q = from ent in context.T_SYS_FILELIST
                        select ent;
                if (q.Count() > 0)
                {

                }



                T_SYS_FILELIST file = new T_SYS_FILELIST();
                file.SMTFILELISTID = Guid.NewGuid().ToString();
                file.FILENAME = "test";
                context.AddToT_SYS_FILELIST(file);
                context.SaveChanges();
            }
        }
    }
}
