using Alfred.Core.Application.Assets;
using Alfred.Core.Application.Attachments;
using Alfred.Core.Application.Brands;
using Alfred.Core.Application.Categories;
using Alfred.Core.Application.Commodities;
using Alfred.Core.Application.Common.Behaviors;
using Alfred.Core.Application.Files;
using Alfred.Core.Application.Querying.Filtering.Parsing;
using Alfred.Core.Application.Units;

using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

namespace Alfred.Core.Application;

/// <summary>
/// Application layer service configuration
/// </summary>
public static class ApplicationModule
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ApplicationModule).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // Register Validators
        services.AddValidatorsFromAssembly(typeof(ApplicationModule).Assembly);

        // Register querying services
        services.AddScoped<IFilterParser, PrattFilterParser>();

        // Register application services
        services.AddScoped<IAssetService, AssetService>();
        services.AddScoped<IAttachmentService, AttachmentService>();
        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICommodityService, CommodityService>();
        services.AddScoped<IUnitService, UnitService>();
        services.AddScoped<IFileService, FileService>();

        return services;
    }
}
