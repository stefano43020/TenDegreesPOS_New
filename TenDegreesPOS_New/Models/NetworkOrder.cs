using System.Collections.Generic;

namespace TenDegreesPOS_New.Models
{
    public class NetworkOrder
    {
        public string TransactionNumber { get; set; } = string.Empty;
        public string MessageType { get; set; } = "NEW_ORDER"; // or "ORDER_COMPLETED"
        public List<NetworkOrderItem> Items { get; set; } = new List<NetworkOrderItem>();
    }

    public class NetworkOrderItem
    {
        public string ProductName { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}