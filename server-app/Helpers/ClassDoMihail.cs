using System.Collections.Generic;
using System.Linq;
namespace WebApi.Services
{
    public static class ClassDoMihail
    {
        //this- It's the object on which I am applying the method
        public static int MetodoDoMihail(this IEnumerable<string> lista)
        {
            return lista.Count();
        }
    }
}