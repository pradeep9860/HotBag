﻿using HotBag.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace HotBag.Core
{
    public class AppCoreModule : ApplicationModule
    {
        public override void Initialize(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            
        }

        public override void PostInitialize(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            
        }

        public override void PreInitialize(IServiceCollection serviceCollection, IConfiguration configuration)
        {
             
        }
    }
}