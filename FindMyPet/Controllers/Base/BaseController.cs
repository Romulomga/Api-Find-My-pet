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

        protected BaseController(INotificator notificador, ITokenUser appUser)
        {
            Notificator = notificador;
            AppUser = appUser;
        }

        protected bool IsValidOperation()
        {
            return !Notificator.haveNotification();
        }

        protected ActionResult CustomResponse(object result = null)
        {
            if (IsValidOperation())
            {
                return Ok(new
                {
                    Success = true,
                    Result = result
                });
            }

            return BadRequest(new
            {
                Success = false,
                Errors = Notificator.getNotifications().Select(n => n.Message)
            });
        }

        protected ActionResult CustomResponse(ModelStateDictionary modelState)
        {
            if (!modelState.IsValid) NotificateErrorModelInvalid(modelState);
            return CustomResponse();
        }

        protected void NotificateErrorModelInvalid(ModelStateDictionary modelState)
        {
            var Erros = modelState.Values.SelectMany(e => e.Errors);
            foreach (var Erro in Erros)
            {
                var ErrorMsg = Erro.Exception == null ? Erro.ErrorMessage : Erro.Exception.Message;
                NotificateError(ErrorMsg);
            }
        }

        protected void NotificateError(string message)
        {
            Notificator.Handle(new Notification(message));
        }
    }
}
