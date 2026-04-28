namespace PrisonHelicopter.Utils.TransfersBridge
{
    internal static class TransferOfferConverter
    {
        public static TransferOfferData FromVanilla(TransferManager.TransferOffer offer)
        {
            return new TransferOfferData
            {
                Active = offer.Active,
                Amount = offer.Amount,
                Building = offer.Building,
                Citizen = offer.Citizen,
                Position = offer.Position,
                Priority = offer.Priority,
                Vehicle = offer.Vehicle,
            };
        }
    }
}