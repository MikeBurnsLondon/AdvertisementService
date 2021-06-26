using ThirdParty;

namespace ImprovedAdvertisementService
{
    /// <summary>
    /// Service to provide Advertisement data
    /// </summary>
    public interface IAdvertisementService
    {
        /// <summary>
        /// Obtain Advertisement information by identifier string
        /// </summary>
        /// <param name="advertId">String identifier for an Advert</param>
        /// <returns>Advertisement object</returns>
        Advertisement GetAdvertisement(string advertId);
    }
}
