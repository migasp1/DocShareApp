using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Collections.Generic;
using System.Linq;
namespace WebApi.Services
{
    public static class ClassDoMihail
    {
        public static int MetodoDoMihail(this IEnumerable<string> lista)
        {
            return lista.Count();
        }

        public static void ExperienciaDeMetodo(AuthenticationOptions configureOptions)
        {
            configureOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            configureOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }
    }
}