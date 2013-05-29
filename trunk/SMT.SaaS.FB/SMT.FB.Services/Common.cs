using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SMT.FB.BLL;
using System.Data.Objects.DataClasses;
using SMT_FB_EFModel;
using System.Reflection;
using System.Xml.Linq;
using SMT.Foundation.Log;
using System.Collections;
namespace SMT.FB.Services
{
    internal static class Helper
    {
        public static IEnumerable<Type> GetKnownTypes(ICustomAttributeProvider provider)
        {
            System.Collections.Generic.List<System.Type> knownTypes =
                new System.Collections.Generic.List<System.Type>();
            //// Add any types to include here.

            //return knownTypes;
            Type[] types = Assembly.Load("SMT_FB_EFModel").GetTypes();

            for (int i = 0; i < types.Length; i++)
            {
                if ((types[i].BaseType == typeof(EntityObject)) || typeof(VisitUserBase).IsAssignableFrom(types[i]))
                {
                    knownTypes.Add(types[i]);
                }
            }
            //List<Type> typesO = knownTypes.ToList();
            //typesO.ForEach(item =>
            //{
            //    knownTypes.Add(typeof(List<>).MakeGenericType(new Type[] { item }));
            //});

            knownTypes.Add(typeof(AuditResult));
            knownTypes.Add(typeof(SaveResult));



            return knownTypes;
        }
    }
    
    


}
