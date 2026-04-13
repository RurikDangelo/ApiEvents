using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apieventsr.Domain.Entities
{
    public class DomainEntity : BaseEntity
    {
        public int MyProperty { get; private set; }

        protected DomainEntity() { } // necessário para o EF Core

        public DomainEntity(int property)
        {
            MyProperty = property;
        }
    }
}
