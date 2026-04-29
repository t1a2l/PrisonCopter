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
                m_isLocalPark = offer.m_isLocalPark,
                m_object = offer.m_object, 
                Position = offer.Position,
                Vehicle = offer.Vehicle
            };
        }
    }
}