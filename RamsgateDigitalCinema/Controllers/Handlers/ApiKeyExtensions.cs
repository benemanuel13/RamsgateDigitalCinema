using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RamsgateDigitalCinema.Controllers.Handlers
{
    public static class ApiKeyExtensions
    {
        public static IApplicationBuilder UseApiKey(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiKeyMiddleware>();
        }

        public static IApplicationBuilder UseRequestCheck(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestCheck>();
        }
    }
}
