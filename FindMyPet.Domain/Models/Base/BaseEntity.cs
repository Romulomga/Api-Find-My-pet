using System;
using System.Collections.Generic;
using System.Text;

namespace FindMyPet.Business.Models.Base
{
    public abstract class BaseEntity
    {
        protected BaseEntity()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
    }
}
