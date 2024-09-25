using System;

namespace Ntl.AuthSS.OrderManagement.Business.Services
{
    public interface IPackageService
    {
        (ReelDto reel, string errorMessage) GetReelDetailsFromCodeForFullfillment(string reelCode, Guid orderItemId);
        (CartonDto carton, string errorMessage) GetCartonDetailsFromCodeForFulfillment(string cartonCode, Guid orderItemId);
        (ReelDto reel, string errorMessage) GetReelDetailsFromCodeForReturnOrder(string reelCode, int orgId);
        (CartonDto carton, string errorMessage) GetCartonDetailsFromCodeForReturnOrder(string cartonCode, int orgId);
        (ReelDto reel, string errorMessage) TraceReel(string reelCode);
        (CartonDto carton, string errorMessage) TraceCarton(string cartonCode);
        (PalletDto pallet, string errorMessage) TracePallet(string palletCode);
    }
}