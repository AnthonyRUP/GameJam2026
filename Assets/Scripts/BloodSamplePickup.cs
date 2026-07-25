using Countdown.Player;

namespace Countdown.World
{
    // A vial that pops out of the Injector - see EjectablePickup for the shared
    // hop/settle/collect behavior. This just says what "collected" means here.
    public class BloodSamplePickup : EjectablePickup
    {
        protected override bool TryCollect(PlayerInventory inventory) => inventory.SetBloodSample();
    }
}