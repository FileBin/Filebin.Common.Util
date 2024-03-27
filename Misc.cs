using System.Reflection;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using Newtonsoft.Json;

namespace Filebin.Common.Util;

public static class Misc {
    public static readonly string NullMarker = "(null)";

    public static readonly int MaxQuery = 1024;


    public static string GetOrThrow<TConfiguration>(this TConfiguration config, string key)
    where TConfiguration : IConfiguration {
        var val = config[key];
        if (val is null) {
            throw new ArgumentException($"Config does not contain {key}");
        }
        return val;
    }

    public static string GetNpgsqlConnectionString(this IConfiguration configuration, string databaseName) {
        var config = new {
            database_host = configuration["Database:Host"] ?? "localhost",
            database_port = configuration["Database:Port"] ?? "5432",
            database_user = configuration.GetOrThrow("Database:User"),
            database_password = configuration.GetOrThrow("Database:Password"),
        };

        return $"Host={config.database_host};"
             + $"Port={config.database_port};"
             + $"Username={config.database_user};"
             + $"Password={config.database_password};"
             + $"Database={databaseName};";
    }

    public static SymmetricSecurityKey GetSecurityKey(this IConfiguration config) {
        return new SymmetricSecurityKey(Convert.FromBase64String(config.GetOrThrow("JwtSecurityKey")));
    }

    public static string DtoToUrlQuery(object o) {
        var pairs = o.GetType().GetProperties()
            .Select(p => {
                var altName = p.GetCustomAttribute<BindPropertyAttribute>()?.Name;
                var Name = altName ?? p.Name;
                return new { Name, Value = p.GetValue(o, null) };
            })
            .Where(p => p.Value != null)
            .Select(p => p.Name + "=" + HttpUtility.UrlEncode(p.Value!.ToString()))
            .ToArray();

        return string.Join("&", pairs);
    }

    public static string AnyToUrlQuery(object o) {
        if (o.GetType().GetProperties().Length > 0)
            return DtoToUrlQuery(o);

        return AnonymousToUrlQuery(o);
    }

    public static Dictionary<string, string> AnonymousToStringDictionary(object obj) {
        return HtmlHelper.AnonymousObjectToHtmlAttributes(obj)
            .Select(pair => new KeyValuePair<string, string>(
                key: pair.Key,
                value: pair.Value.ToString()!
            ))
            .Where(pair => !string.IsNullOrEmpty(pair.Value))
            .ToDictionary();
    }

    public static string AnonymousToUrlQuery(object o) {
        var json = JsonConvert.SerializeObject(o);
        var dict = HtmlHelper.AnonymousObjectToHtmlAttributes(o);
        var pairs = dict.Select(x => {
            var str = x.Value?.ToString();
            if (str is not null) {
                return $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(str)}";
            }
            return null;
        })
        .Where(x => x is not null)
        .ToArray();
        return string.Join("&", pairs);
    }
}
