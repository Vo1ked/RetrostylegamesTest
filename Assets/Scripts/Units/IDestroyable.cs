using System.Threading;

namespace RetroStyleGamesTest.Units
{
    public interface IDestroyable
    {
        CancellationTokenSource DestroyCancellationToken { get; set; }
    }
}