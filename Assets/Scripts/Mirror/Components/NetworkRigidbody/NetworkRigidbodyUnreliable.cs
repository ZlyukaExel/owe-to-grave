using UnityEngine;

namespace Mirror
{
    // [RequireComponent(typeof(Rigidbody))] <- OnValidate ensures this is on .target
    [AddComponentMenu("Network/Network Rigidbody (Unreliable)")]
    public class NetworkRigidbodyUnreliable : NetworkTransformUnreliable
    {
        bool clientAuthority => syncDirection == SyncDirection.ClientToServer;
        public bool isKinematic = false;
        Rigidbody rb;

        protected override void OnValidate()
        {
            // Skip if Editor is in Play mode
            if (Application.isPlaying) return;

            base.OnValidate();

            // we can't overwrite .target to be a Rigidbody.
            // but we can ensure that .target has a Rigidbody, and use it.
            if (target.GetComponent<Rigidbody>() == null)
            {
                Debug.LogWarning($"{name}'s NetworkRigidbody.target {target.name} is missing a Rigidbody", this);
            }
        }

        // cach Rigidbody and original isKinematic setting
        protected override void Awake()
        {
            // we can't overwrite .target to be a Rigidbody.
            // but we can use its Rigidbody component.
            rb = target.GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError($"{name}'s NetworkRigidbody.target {target.name} is missing a Rigidbody", this);
                return;
            }
            base.Awake();
        }

        // reset forced isKinematic flag to original.
        // otherwise the overwritten value would remain between sessions forever.
        // for example, a game may run as client, set rigidbody.iskinematic=true,
        // then run as server, where .iskinematic isn't touched and remains at
        // the overwritten=true, even though the user set it to false originally.
        public override void OnStopServer() => rb.isKinematic = isKinematic;
        public override void OnStopClient() => rb.isKinematic = isKinematic;

        // overwriting Construct() and Apply() to set Rigidbody.MovePosition
        // would give more jittery movement.

        // FixedUpdate for physics
        void FixedUpdate()
        {
            // who ever has authority moves the Rigidbody with physics.
            // everyone else simply sets it to kinematic.
            // so that only the Transform component is synced.

            /*bool owned = false;

            // Host (Server + Client) calculates physics if item has no owner or it has rights
            if (isServer && isClient)
            {
                owned = netIdentity.connectionToClient == null || IsClientWithAuthority;
            }
            // Client with rights calculates physics
            else if (isClient)
            {
                owned = IsClientWithAuthority;
            }
            // Server calculates physics if item has no owner
            else if (isServer)
            {
                owned = netIdentity.connectionToClient == null;
            }*/

            // If item has no owner server calculates physics
            // Otherwise only client with ownership calculates physics
            bool owned = netIdentity.connectionToClient == null && isServer || IsClientWithAuthority;

            // Calculate physics
            if (owned)
            {
                rb.isKinematic = isKinematic;
            }
            // Don't calculate
            else
            {
                rb.isKinematic = true;
            }
        }

        protected override void OnTeleport(Vector3 destination)
        {
            base.OnTeleport(destination);

            rb.position = transform.position;
        }

        protected override void OnTeleport(Vector3 destination, Quaternion rotation)
        {
            base.OnTeleport(destination, rotation);

            rb.position = transform.position;
            rb.rotation = transform.rotation;
        }
    }
}
