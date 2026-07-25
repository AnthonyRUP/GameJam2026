namespace Countdown.World
{
    // Optional companion to Interactable/StationHighlight: anything that animates
    // into place (e.g. a vial hopping out of the Injector) can implement this so
    // StationHighlight knows to wait until it's actually settled before showing the
    // "you can interact with this" affordance. Objects that don't implement it are
    // assumed to be always settled (a static station has nothing to wait for).
    public interface ISettleGate
    {
        bool HasSettled { get; }
    }
}
