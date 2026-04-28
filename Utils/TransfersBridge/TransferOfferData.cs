using UnityEngine;

namespace PrisonHelicopter.Utils.TransfersBridge
{
    internal struct TransferOfferData
    {
        public bool Active;
        public int Amount;
        public ushort Building;
        public uint Citizen;
        public Vector3 Position;
        public int Priority;
        public ushort Vehicle;
    }
}