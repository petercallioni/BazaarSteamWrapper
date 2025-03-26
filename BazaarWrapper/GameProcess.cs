using System.Diagnostics;

namespace BazaarWrapper
{
    public record GameProcess(Process Process, GameLaunchParameters GameLaunchParameters);
}
