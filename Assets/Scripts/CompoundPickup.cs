using Countdown.Player;
using Countdown.Runtime;

namespace Countdown.World
{
    // A finished compound that pops out of the Mixer - see EjectablePickup for the
    // shared hop/settle/collect behavior. The compound to hand over is set right
    // after Instantiate (before Start runs the eject animation), via the Compound
    // property below.
    public class CompoundPickup : EjectablePickup
    {
        public Compound Compound { get; set; }

        protected override bool TryCollect(PlayerInventory inventory) => inventory.SetCompound(Compound);
    }
}
