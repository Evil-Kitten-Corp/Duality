using Unity.Netcode.Components;

namespace Network
{
    public class ClientNetworkAnimator : NetworkAnimator
    {
        protected override bool OnIsServerAuthoritative() => false;
    }
}