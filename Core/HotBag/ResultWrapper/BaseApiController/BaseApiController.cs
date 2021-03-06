﻿using HotBag.Identity.AppSession;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace HotBag.BaseController
{
 
    [Route("api/app/v1/[controller]")]
    [ApiController]
    //[Authorize]
    public class BaseApiController : ControllerBase
    {
        protected readonly IAppSession AppSession; 
        public BaseApiController()
        {
            AppSession = NullAppSession.Instance;
        }
    }
}
