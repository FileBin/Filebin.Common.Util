namespace Filebin.Common.Util.Abstraction;

public interface ILinkGenerator {
    public string GenerateLink(object? values = null);
}