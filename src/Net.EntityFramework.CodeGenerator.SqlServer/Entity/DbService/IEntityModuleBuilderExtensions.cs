﻿using Net.EntityFramework.CodeGenerator.Core;
using Net.EntityFramework.CodeGenerator.SqlServer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Net.EntityFramework.CodeGenerator;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class IEntityModuleBuilderExtensions
    {
        public static IPackageToken DbService(this IEntityModuleBuilder module, params IPackageToken[] correlateWith)
        {
            var token = module.PackageTokenProvider.CreateToken();
            module.Services.TryAddSingleton<IDbServiceModuleIntentBuilder, DbServiceModuleIntentBuilder>();

            module.Services.AddSingleton(p =>
            {
                var builder = p.GetRequiredService<IDbServiceModuleIntentBuilder>();
                builder.CorrelateWith(correlateWith);
                builder.Services.AddSingleton(token);
                return (IPackageBuilder)builder;
            });
            return token;
        }
    }
}