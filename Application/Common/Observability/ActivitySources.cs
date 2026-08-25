using System.Diagnostics;

namespace Application.Common.Observability;

public static class ActivitySources
{
    public static readonly ActivitySource ShopApi =
        new("ShopApi");
}
