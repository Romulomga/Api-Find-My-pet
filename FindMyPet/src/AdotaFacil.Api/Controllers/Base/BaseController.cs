using AdotaFacil.Business.Interfaces;
using AdotaFacil.Business.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;

namespace AdotaFacil.Api.Controllers.Base
{
    [ApiController]
    public class BaseController : ControllerBase
    {
        private readonly INotificator Notificator;
        public readonly IJwtUser AppUser;

        protected BaseController(INotificator notificator, IJwtUser appUser)
        {
            this.Notificator = notificator;
            this.AppUser = appUser;
        }

        protected bool IsValidOperation()
        {
            return !Notificator.HaveNotification();
        }

        protected ActionResult CustomResponse(object result = null)
        {
            if (IsValidOperation())
            {
                return Ok(new
                {
                    result
                });
            }

            return BadRequest(new
            {
                Errors = Notificator.GetNotifications().Select(n => n.Message)
            });
        }

        protected ActionResult CustomResponse(ModelStateDictionary modelState)
        {
            if (!modelState.IsValid) NotificateErrorModelInvalid(modelState);
            return CustomResponse();
        }

        protected void NotificateErrorModelInvalid(ModelStateDictionary modelState)
        {
            var errors = modelState.Values.SelectMany(e => e.Errors);
            foreach (var error in errors)
            {
                var errorMsg = error.Exception == null ? error.ErrorMessage : error.Exception.Message;
                NotificateError(errorMsg);
            }
        }

        protected void NotificateError(string message)
        {
            Notificator.Handle(new Notification(message));
        }
    }
}
