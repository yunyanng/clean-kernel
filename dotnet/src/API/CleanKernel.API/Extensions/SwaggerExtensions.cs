﻿using NSwag.Generation.AspNetCore;

namespace Microsoft.Extensions.DependencyInjection;

public static class SwaggerExtensions
{
    public static void AddSwagger(
        this IServiceCollection services,
        string title,
        string description,
        Action<AspNetCoreOpenApiDocumentGeneratorSettings>? documentSettings)
    {
        var apiVersionDescriptionProvider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

        foreach (var apiVersionDescription in apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            services.SwaggerDocument(o =>
            {
                o.DocumentSettings = s =>
                {
                    s.DocumentName = apiVersionDescription.GroupName;
                    s.Title = title;
                    s.Version = apiVersionDescription.ApiVersion.ToString();

                    var text = new StringBuilder(description);

                    if (apiVersionDescription.IsDeprecated)
                    {
                        text.Append(" This API version has been deprecated.");
                    }

                    if (apiVersionDescription.SunsetPolicy is SunsetPolicy policy)
                    {
                        if (policy.Date is DateTimeOffset when)
                        {
                            text.Append(" The API will be sunset on ")
                                .Append(when.Date.ToShortDateString())
                                .Append('.');
                        }

                        if (policy.HasLinks)
                        {
                            text.AppendLine();

                            for (var i = 0; i < policy.Links.Count; i++)
                            {
                                var link = policy.Links[i];

                                if (link.Type == "text/html")
                                {
                                    text.AppendLine();

                                    if (link.Title.HasValue)
                                    {
                                        text.Append(link.Title.Value).Append(": ");
                                    }

                                    text.Append(link.LinkTarget.OriginalString);
                                }
                            }
                        }
                    }

                    s.Description = text.ToString();

                    if (documentSettings is not null)
                    {
                        documentSettings(s);
                    }
                };

                o.EndpointFilter = ep => ep.EndpointTags?.Contains("IntegrationEventHandler") is false or null;
            });
        }
    }

    public static void UseSwagger([NotNull] this WebApplication app, string? oAuthClientId = null, string? oAuthAppName = null)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        app.UseSwaggerGen(uiConfig: options =>
        {
            var descriptions = app.DescribeApiVersions();

            // build a swagger endpoint for each discovered API version
            foreach (var description in descriptions)
            {
                var url = $"/swagger/{description.GroupName}/swagger.json";
                var name = description.GroupName.ToUpperInvariant();

                options.SwaggerRoutes.Add(new(name, url));
            }

            if (oAuthClientId is not null)
            {
                options.OAuth2Client ??= new();
                options.OAuth2Client.ClientId = oAuthClientId;
                options.OAuth2Client.AppName = oAuthAppName ?? oAuthClientId;
            }
        });
    }
}
