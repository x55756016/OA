using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_FB_EFModel;

namespace SMT_FB_EFModel
{
    class Program
    {
        static void Main(string[] args)
        {
            using (SMT_FB_EFModelContext context = new SMT_FB_EFModelContext())
            {
                Console.ReadLine();
            }
        }
    }
}
