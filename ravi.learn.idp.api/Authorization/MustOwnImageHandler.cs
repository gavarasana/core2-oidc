using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ravi.learn.idp.api.Authorization
{
    public class MustOwnImageHandler : AuthorizationHandler<MustOwnImageRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MustOwnImageRequirement requirement)
        {
            throw new NotImplementedException();
        }
    }
}
