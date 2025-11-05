-- ====== IDs & OrderCode (đổi nếu muốn) ======
DECLARE @UserId       UNIQUEIDENTIFIER = 'f0ca0757-1530-4daa-8a50-25b83749bf4b';
DECLARE @PlanId       UNIQUEIDENTIFIER = NEWID();
DECLARE @PlanPriceId  UNIQUEIDENTIFIER = NEWID();
DECLARE @SubId        UNIQUEIDENTIFIER = NEWID();
DECLARE @OrderId      UNIQUEIDENTIFIER = NEWID();
DECLARE @OrderCode    BIGINT           = 202510050001;  -- đổi số khác nếu cần tạo thêm đơn

-- ====== 1) Plan
INSERT INTO dbo.Plans (PlanId, Code, Name, IsActive)
VALUES (@PlanId, N'BASIC', N'Basic Plan', 1);

-- ====== 2) PlanPrice (Monthly = 30 ngày, 99.000đ)
-- Period: 0=OneTime, 1=Monthly, 2=Yearly
INSERT INTO dbo.PlanPrices (PlanPriceId, PlanId, Period, DurationInDays, AmountVnd, IsActive)
VALUES (@PlanPriceId, @PlanId, 1, 30, 99000, 1);

-- ====== 3) Subscription (Inactive ban đầu)
-- SubscriptionStatus: 0=Inactive,1=Active,2=PastDue,3=Cancelled,4=Expired
INSERT INTO dbo.Subscriptions (SubscriptionId, UserId, PlanId, Status, CancelAtPeriodEnd)
VALUES (@SubId, @UserId, @PlanId, 0, 0);

-- ====== 4) Order Pending
-- OrderStatus: 0=Pending,1=Paid,2=Failed,3=Cancelled,4=Expired
INSERT INTO dbo.Orders
(
    OrderId, OrderCode, UserId, SubscriptionId, PlanPriceId,
    AmountVnd, Status, ReturnUrl, CancelUrl, CreatedAt
)
VALUES
(
    @OrderId, @OrderCode, @UserId, @SubId, @PlanPriceId,
    99000, 0,
    N'https://hometrack.mlhr.org/api/Payment/paymentconfirm',
    N'https://hometrack.mlhr.org/api/Payment/payment-fail',
    SYSDATETIMEOFFSET()
);

-- ====== Xem nhanh kết quả
SELECT 'Plan' AS T, * FROM dbo.Plans WHERE PlanId = @PlanId;
SELECT 'PlanPrice' AS T, * FROM dbo.PlanPrices WHERE PlanPriceId = @PlanPriceId;
SELECT 'Subscription' AS T, * FROM dbo.Subscriptions WHERE SubscriptionId = @SubId;
SELECT 'Order' AS T, * FROM dbo.Orders WHERE OrderId = @OrderId;
