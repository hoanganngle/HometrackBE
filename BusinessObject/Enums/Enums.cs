using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Enums
{
    public enum BillingPeriod { OneTime = 0, Monthly = 1, Yearly = 2 }

    public enum SubscriptionStatus { Inactive = 0, Active = 1, Cancelled = 2 }
    public enum IsPremium { Inactive = 0, Active = 1, Cancelled = 2 }

    public enum OrderStatus { Pending = 0, Paid = 1, Failed = 2, Cancelled = 3, Expired = 4 }

    public enum PaymentProvider { PayOS = 1 }

    public enum PaymentStatus { Paid = 1 }
}
