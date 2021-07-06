using FindMyPet.Business.Interfaces;
using FindMyPet.Business.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;

namespace FindMyPet.Controllers.Base
{
    [ApiController]
    public class BaseController : ControllerBase
    {
        private readonly INotificator Notificator;
        public readonly IUser AppUser;

        protected BaseController(INotificator Notificator, IUser AppUser)
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
                    Result
                });
            }

            return BadRequest(new
            {
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
