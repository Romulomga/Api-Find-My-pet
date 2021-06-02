using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FindMyPet.Extensions
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate Next;

        public ExceptionMiddleware(RequestDelegate Next)
        {
            this.Next = Next;
        }

        public async Task InvokeAsync(HttpContext HttpContext)
        {
            try
            {
                await Next(HttpContext);
            }
            catch (Exception ex)
            {
                HandleExceptionAsync(HttpContext, ex);
            }
        }

        private static void HandleExceptionAsync(HttpContext Context, Exception Exception)
        {
            //exception.Ship(context);
            Context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
    }
}
