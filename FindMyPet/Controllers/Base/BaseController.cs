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
        private readonly INotificator _notificator;
        public readonly ITokenUser AppUser;

        protected BaseController(INotificator notificador, ITokenUser appUser)
        {
            _notificator = notificador;
            AppUser = appUser;
        }

        protected bool IsValidOperation()
        {
            return !_notificator.haveNotification();
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
                Errors = _notificator.getNotifications().Select(n => n.Mensagem)
            });
        }

        protected ActionResult CustomResponse(ModelStateDictionary modelState)
        {
            if (!modelState.IsValid) NotificateErrorModelInvalid(modelState);
            return CustomResponse();
        }

        protected void NotificateErrorModelInvalid(ModelStateDictionary modelState)
        {
            var erros = modelState.Values.SelectMany(e => e.Errors);
            foreach (var erro in erros)
            {
                var errorMsg = erro.Exception == null ? erro.ErrorMessage : erro.Exception.Message;
                NotificateError(errorMsg);
            }
        }

        protected void NotificateError(string mensagem)
        {
            _notificator.Handle(new Notification(mensagem));
        }
    }
}
