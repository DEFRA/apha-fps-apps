namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public enum BudgetResourceLevelModalType
    {
        BudgetBid,
        Purchase
    }

    public class BudgetResourceLevelItem
    {
        public BudgetResourceLevelModalType ModalType { get; init; }
        public BudgetResourceCentreLevelItem? BudgetBid { get; init; }
        public PurchaseItem? Purchase { get; init; }

        public static BudgetResourceLevelItem ForBudgetBid(BudgetResourceCentreLevelItem model) =>
            new() { ModalType = BudgetResourceLevelModalType.BudgetBid, BudgetBid = model };

        public static BudgetResourceLevelItem ForPurchase(PurchaseItem model) =>
            new() { ModalType = BudgetResourceLevelModalType.Purchase, Purchase = model };
    }
}
