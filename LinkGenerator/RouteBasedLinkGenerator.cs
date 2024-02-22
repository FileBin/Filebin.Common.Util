using Filebin.Common.Util.Abstraction;

namespace Filebin.Common.Util.LinkGenerator;

public class RouteBasedLinkGenerator : ILinkGenerator {
    public required string Route { get; init; }

    public string GenerateLink(object? values = null) {
        var link = Route.Trim();
        if (link.EndsWith('/')) {
            link = link[..^1];
        }
        if (values is not null) {
            link = $"{link}?{Misc.AnyToUrlQuery(values)}";
        }
        return link;
    }
}
