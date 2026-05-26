using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

namespace Store.API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Store Orders API",
                Version = "v1",
                Description = "RESTful API for managing e-commerce orders.",
                Contact = new OpenApiContact
                {
                    Name = "Store Team",
                    Email = "dev@Store.com"
                }
            });

            string xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);
        });

        return services;
    }

    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "openapi/{documentName}.json";
        });

        // Scalar UI at /scalar
        app.MapScalarApiReference(options =>
        {
            options.Title = "Store Orders API";
            options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });

        // Classic Swagger UI at /swagger
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "Store API v1");
            options.RoutePrefix = "swagger";
        });

        return app;
    }
}
