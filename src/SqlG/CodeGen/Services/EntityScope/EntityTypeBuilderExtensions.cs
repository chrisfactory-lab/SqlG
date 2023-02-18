﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace SqlG
{
    public static partial class ISqlGenEntityBuilderExtensions
    {
        internal const string EntityGenerateAnnotationKey = nameof(EntityGenerateAnnotationKey);
        public static EntityTypeBuilder Generate(this EntityTypeBuilder entityBuilder, Action<ISqlGenEntityBuilder> builder)
        {
            var b = new SqlGenEntityBuilder();
            builder?.Invoke(b);
            entityBuilder.HasAnnotation(EntityGenerateAnnotationKey, b.Build());
            return entityBuilder;
        }

        public static ISqlGenEntityBuilder AddGenActionBuilder<TServiceActionBuilder, TActionBuilder>(this ISqlGenEntityBuilder builder, Action<TServiceActionBuilder>? builderService = null)
             where TServiceActionBuilder : ISqlGenActionBuilder
             where TActionBuilder : ISqlGenActionBuilder, TServiceActionBuilder, new()
        {

            TServiceActionBuilder b = new TActionBuilder();
            builderService?.Invoke(b);
            builder.Services.AddSingleton<ISqlGenActionBuilder>(b);
            return builder;
        }

        public static ISqlGenModelBuilder AddGenActionBuilder<TServiceActionBuilder, TActionBuilder>(this ISqlGenModelBuilder builder, Action<TServiceActionBuilder>? builderService = null)
             where TServiceActionBuilder : ISqlGenActionBuilder
             where TActionBuilder : ISqlGenActionBuilder, TServiceActionBuilder, new()
        {
            TServiceActionBuilder b = new TActionBuilder();
            builderService?.Invoke(b);
            builder.Services.AddSingleton<ISqlGenActionBuilder>(b);
            return builder;
        }
    }



}
