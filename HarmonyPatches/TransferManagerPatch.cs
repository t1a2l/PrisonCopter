using HarmonyLib;
using PrisonHelicopter.Utils.TransfersBridge;

namespace PrisonHelicopter.HarmonyPatches
{
    [HarmonyPatch]
    internal class TransferManagerPatch
    {
        [HarmonyPatch(typeof(TransferManager), "StartTransfer")]
        [HarmonyPrefix]
        internal static bool StartTransfer(TransferManager.TransferReason material, TransferManager.TransferOffer offerOut, TransferManager.TransferOffer offerIn, int delta)
        {
            if (TransfersAPI.Backend is not MoreTransferReasonsBackend)
                return true;

            if (!TryMapVanillaMaterial(material, out var prisonReason))
                return true;

            var outgoing = TransferOfferConverter.FromVanilla(offerOut);
            var incoming = TransferOfferConverter.FromVanilla(offerIn);

            if (MoreTransferReasonsBridge.TryExtendedStartTransfer(prisonReason, outgoing, incoming, delta))
                return false;

            return true;
        }

        private static bool TryMapVanillaMaterial(TransferManager.TransferReason material, out PrisonHelicopterTransferReason prisonReason)
        {
            byte raw = (byte)material;

            switch (raw)
            {
                case 58:
                    prisonReason = PrisonHelicopterTransferReason.PoliceVanCrimeMove;
                    return true;
                case 59:
                    prisonReason = PrisonHelicopterTransferReason.CrimePickup2;
                    return true;
                case 60:
                    prisonReason = PrisonHelicopterTransferReason.CrimeMove2;
                    return true;
                default:
                    prisonReason = default;
                    return false;
            }
        }
    }
}
