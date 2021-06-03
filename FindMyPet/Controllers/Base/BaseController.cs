using FindMyPet.Business.Interfaces;
using FindMyPet.Business.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FindMyPet.Controllers.Base
{
    [ApiController]
    public class BaseController : ControllerBase
    {
        private readonly INotificator Notificator;
        public readonly ITokenUser AppUser;

        protected BaseController(INotificator Notificator, ITokenUser AppUser)
        {
            this.Notificator = Notificator;
            this.AppUser = AppUser;
        }

        protected bool IsValidOperation()
        {
            return !Notificator.HaveNotification();
        }

        protected ActionResult CustomResponse(object Result = null)
        {
            if (IsValidOperation())
            {
                return Ok(new
                {
                    Success = true,
                    Result
                });
            }

            return BadRequest(new
            {
                Success = false,
                Errors = Notificator.GetNotifications().Select(n => n.Message)
            });
        }

        protected ActionResult CustomResponse(ModelStateDictionary ModelState)
        {
            if (!ModelState.IsValid) NotificateErrorModelInvalid(ModelState);
            return CustomResponse();
        }

        protected void NotificateErrorModelInvalid(ModelStateDictionary ModelState)
        {
            var Errors = ModelState.Values.SelectMany(e => e.Errors);
            foreach (var Error in Errors)
            {
                var ErrorMsg = Error.Exception == null ? Error.ErrorMessage : Error.Exception.Message;
                NotificateError(ErrorMsg);
            }
        }

        protected void NotificateError(string Message)
        {
            Notificator.Handle(new Notification(Message));
        }
    }
}
