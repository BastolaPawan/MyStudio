CREATE TABLE Customer (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    CustomerType NVARCHAR(20) NOT NULL DEFAULT 'Retail',
    Phone NVARCHAR(20) NULL,
    Email NVARCHAR(100) NULL,
    Address NVARCHAR(500) NULL,
    BusinessName NVARCHAR(200) NULL,
    TaxNumber NVARCHAR(50) NULL,
    DiscountPercentage DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    IsActive BIT NOT NULL DEFAULT 1
);

-- Sample customers
INSERT INTO Customer (Name, CustomerType, Phone, Email, BusinessName, DiscountPercentage) VALUES
('John Smith', 'Retail', '+1-555-0101', 'john.smith@email.com', NULL, 0),
('Sarah Johnson', 'Retail', '+1-555-0102', 'sarah.j@email.com', NULL, 0),
('Photo Studio Pro', 'Wholesale', '+1-555-0201', 'orders@photostudiopro.com', 'Photo Studio Pro LLC', 15.00),
('Creative Images Inc', 'Wholesale', '+1-555-0202', 'billing@creativeimages.com', 'Creative Images Incorporated', 20.00),
('Mike Davis', 'Retail', '+1-555-0103', 'mike.davis@email.com', NULL, 0);

CREATE TABLE ProductService (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Type NVARCHAR(20) NOT NULL DEFAULT 'Service',
    Category NVARCHAR(50) NULL,
    Description NVARCHAR(1000) NULL,
    Price DECIMAL(18,2) NOT NULL,
    Cost DECIMAL(18,2) NOT NULL DEFAULT 0,
    Unit NVARCHAR(20) NULL,
    StockQuantity INT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- Sample products and services
INSERT INTO ProductService (Name, Type, Category, Description, Price, Cost, Unit) VALUES
('Studio Portrait Session', 'Service', 'Photography', '1-hour professional studio portrait session', 150.00, 50.00, 'Session'),
('Outdoor Photo Shoot', 'Service', 'Photography', '2-hour outdoor location photo shoot', 300.00, 100.00, 'Session'),
('Wedding Photography', 'Service', 'Photography', 'Full day wedding photography coverage', 1200.00, 400.00, 'Day'),
('8x10 Photo Print', 'Product', 'Printing', 'High quality 8x10 inch photo print', 15.00, 3.00, 'Piece'),
('12x18 Photo Print', 'Product', 'Printing', 'Premium 12x18 inch photo print', 25.00, 5.00, 'Piece'),
('Photo Album', 'Product', 'Album', '20-page custom photo album', 200.00, 80.00, 'Album'),
('Digital Files Package', 'Service', 'Digital', 'All edited digital files', 250.00, 20.00, 'Package'),
('Passport Photos', 'Service', 'Document', 'Official passport photos (4 pieces)', 20.00, 2.00, 'Set');



CREATE TABLE [Order] (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderNumber NVARCHAR(20) NOT NULL UNIQUE,
    OrderDate DATETIME2 NOT NULL,
    DeliveryDate DATETIME2 NULL,
    CustomerId INT NOT NULL,
    OrderType NVARCHAR(50) NOT NULL DEFAULT 'Studio',
    SessionType NVARCHAR(100) NULL,
    SubTotal DECIMAL(18,2) NOT NULL DEFAULT 0,
    TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    ShippingCharge DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    AdvancePaid DECIMAL(18,2) NOT NULL DEFAULT 0,
    DeliveryMethod NVARCHAR(20) NOT NULL DEFAULT 'Pickup',
    CourierName NVARCHAR(100) NULL,
    TrackingNumber NVARCHAR(100) NULL,
    DeliveryAddress NVARCHAR(500) NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Draft',
    PaymentStatus NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    PhotoShootDate DATETIME2 NULL,
    ReadyDate DATETIME2 NULL,
    ActualDeliveryDate DATETIME2 NULL,
    IsCancelled BIT NOT NULL DEFAULT 0,
    CancelledDate DATETIME2 NULL,
    CancellationReason NVARCHAR(500) NULL,
    CancellationCharge DECIMAL(18,2) NOT NULL DEFAULT 0,
    IsBadDebt BIT NOT NULL DEFAULT 0,
    Notes NVARCHAR(1000) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Order_Customer FOREIGN KEY (CustomerId) REFERENCES Customer(Id)
);

-- Sample orders
INSERT INTO [Order] (OrderNumber, OrderDate, CustomerId, OrderType, SessionType, SubTotal, TaxAmount, DiscountAmount, ShippingCharge, TotalAmount, AdvancePaid, Status, PaymentStatus) VALUES
('ORD-2024-001', '2024-01-15', 1, 'Studio', 'Portrait', 150.00, 19.50, 0.00, 0.00, 169.50, 85.00, 'Ready', 'Partial'),
('ORD-2024-002', '2024-01-16', 3, 'Outdoor', 'Commercial', 600.00, 78.00, 90.00, 100.00, 688.00, 344.00, 'Confirmed', 'Partial'),
('ORD-2024-003', '2024-01-18', 2, 'Studio', 'Family', 300.00, 39.00, 0.00, 0.00, 339.00, 339.00, 'Delivered', 'Paid'),
('ORD-2024-004', '2024-01-20', 4, 'Package', 'Wedding', 2000.00, 260.00, 400.00, 0.00, 1860.00, 930.00, 'InProgress', 'Partial'),
('ORD-2024-005', '2024-01-22', 5, 'Service', 'Passport', 20.00, 2.60, 0.00, 0.00, 22.60, 0.00, 'Ready', 'Pending');


CREATE TABLE OrderItem (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    ItemType NVARCHAR(20) NOT NULL DEFAULT 'Service',
    Description NVARCHAR(500) NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    UnitPrice DECIMAL(18,2) NOT NULL,
    DiscountPercent DECIMAL(18,2) NOT NULL DEFAULT 0,
    LineTotal DECIMAL(18,2) NOT NULL,
    ProductId INT NULL,
    CONSTRAINT FK_OrderItem_Order FOREIGN KEY (OrderId) REFERENCES [Order](Id),
    CONSTRAINT FK_OrderItem_ProductService FOREIGN KEY (ProductId) REFERENCES ProductService(Id)
);

-- Sample order items
INSERT INTO OrderItem (OrderId, ItemType, Description, Quantity, UnitPrice, DiscountPercent, LineTotal) VALUES
(1, 'Service', 'Studio Portrait Session - 1 hour', 1, 150.00, 0, 150.00),
(2, 'Service', 'Outdoor Photo Shoot - 2 hours', 2, 300.00, 15, 510.00),
(2, 'Product', '12x18 Photo Prints', 5, 25.00, 15, 106.25),
(3, 'Service', 'Family Portrait Session', 1, 200.00, 0, 200.00),
(3, 'Product', '8x10 Photo Prints', 5, 15.00, 0, 75.00),
(3, 'Product', 'Digital Files Package', 1, 250.00, 0, 250.00),
(4, 'Service', 'Wedding Photography - Full Day', 1, 1200.00, 20, 960.00),
(4, 'Product', 'Premium Photo Album', 1, 200.00, 20, 160.00),
(4, 'Product', '12x18 Canvas Print', 2, 80.00, 20, 128.00),
(5, 'Service', 'Passport Photos', 1, 20.00, 0, 20.00);


CREATE TABLE Payment (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    PaymentNumber NVARCHAR(20) NOT NULL,
    PaymentDate DATETIME2 NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    PaymentMethod NVARCHAR(20) NOT NULL DEFAULT 'Cash',
    PaymentType NVARCHAR(20) NOT NULL DEFAULT 'Advance',
    ReferenceNumber NVARCHAR(100) NULL,
    Notes NVARCHAR(500) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Payment_Order FOREIGN KEY (OrderId) REFERENCES [Order](Id)
);

-- Sample payments
INSERT INTO Payment (OrderId, PaymentNumber, PaymentDate, Amount, PaymentMethod, PaymentType, ReferenceNumber) VALUES
(1, 'PAY-20240115-001', '2024-01-15', 85.00, 'Card', 'Advance', 'TXN001'),
(2, 'PAY-20240116-001', '2024-01-16', 344.00, 'BankTransfer', 'Advance', 'BT001'),
(3, 'PAY-20240118-001', '2024-01-18', 339.00, 'Cash', 'Full', NULL),
(4, 'PAY-20240120-001', '2024-01-20', 930.00, 'Online', 'Advance', 'OL001'),
(1, 'PAY-20240125-001', '2024-01-25', 84.50, 'Cash', 'Partial', NULL);


-- Create indexes for better performance
CREATE INDEX IX_Order_CustomerId ON [Order](CustomerId);
CREATE INDEX IX_Order_OrderDate ON [Order](OrderDate);
CREATE INDEX IX_Order_Status ON [Order](Status);
CREATE INDEX IX_Order_PaymentStatus ON [Order](PaymentStatus);
CREATE INDEX IX_Order_IsCancelled ON [Order](IsCancelled);
CREATE INDEX IX_Order_IsBadDebt ON [Order](IsBadDebt);

CREATE INDEX IX_OrderItem_OrderId ON OrderItem(OrderId);
CREATE INDEX IX_OrderItem_ProductId ON OrderItem(ProductId);

CREATE INDEX IX_Payment_OrderId ON Payment(OrderId);
CREATE INDEX IX_Payment_PaymentDate ON Payment(PaymentDate);

CREATE INDEX IX_Customer_CustomerType ON Customer(CustomerType);
CREATE INDEX IX_Customer_IsActive ON Customer(IsActive);

CREATE INDEX IX_ProductService_Type ON ProductService(Type);
CREATE INDEX IX_ProductService_IsActive ON ProductService(IsActive);


-- View for unpaid bills
CREATE VIEW UnpaidBills AS
SELECT 
    o.Id,
    o.OrderNumber,
    o.OrderDate,
    c.Name AS CustomerName,
    c.CustomerType,
    o.TotalAmount,
    o.AdvancePaid,
    (o.TotalAmount - o.AdvancePaid - ISNULL(SUM(p.Amount), 0)) AS BalanceAmount,
    o.Status,
    o.PaymentStatus,
    o.DeliveryDate
FROM [Order] o
JOIN Customer c ON o.CustomerId = c.Id
LEFT JOIN Payment p ON o.Id = p.OrderId
WHERE o.IsCancelled = 0
GROUP BY o.Id, o.OrderNumber, o.OrderDate, c.Name, c.CustomerType, o.TotalAmount, o.AdvancePaid, o.Status, o.PaymentStatus, o.DeliveryDate
HAVING (o.TotalAmount - o.AdvancePaid - ISNULL(SUM(p.Amount), 0)) > 0;

-- View for order details with customer info
CREATE VIEW OrderDetails AS
SELECT 
    o.*,
    c.Name AS CustomerName,
    c.Phone AS CustomerPhone,
    c.Email AS CustomerEmail,
    c.CustomerType,
    (SELECT SUM(Amount) FROM Payment WHERE OrderId = o.Id) AS TotalPayments,
    (o.TotalAmount - o.AdvancePaid - (SELECT SUM(Amount) FROM Payment WHERE OrderId = o.Id)) AS CurrentBalance
FROM [Order] o
JOIN Customer c ON o.CustomerId = c.Id;


-- View for sales report
CREATE VIEW SalesReport AS
SELECT 
    o.OrderNumber,
    o.OrderDate,
    c.Name AS CustomerName,
    c.CustomerType,
    o.SubTotal,
    o.DiscountAmount,
    o.TaxAmount,
    o.ShippingCharge,
    o.TotalAmount,
    o.AdvancePaid,
    (SELECT SUM(Amount) FROM Payment WHERE OrderId = o.Id) AS TotalPayments,
    o.Status,
    o.PaymentStatus
FROM [Order] o
JOIN [Customer] c ON o.CustomerId = c.Id
WHERE o.IsCancelled = 0;


-- Add some sample cancelled orders and bad debts
INSERT INTO [Order] (OrderNumber, OrderDate, CustomerId, OrderType, SubTotal, TaxAmount, TotalAmount, AdvancePaid, Status, PaymentStatus, IsCancelled, CancelledDate, CancellationReason, CancellationCharge, IsBadDebt) VALUES
('ORD-2024-006', '2024-01-10', 1, 'Studio', 200.00, 26.00, 226.00, 113.00, 'Cancelled', 'Partial', 1, '2024-01-12', 'Customer changed mind', 50.00, 1),
('ORD-2024-007', '2024-01-08', 2, 'Outdoor', 450.00, 58.50, 508.50, 254.25, 'Cancelled', 'Partial', 1, '2024-01-20', 'Weather conditions', 100.00, 0);

-- Add payments for cancelled orders
INSERT INTO Payment (OrderId, PaymentNumber, PaymentDate, Amount, PaymentMethod, PaymentType) VALUES
(6, 'PAY-20240110-001', '2024-01-10', 113.00, 'Card', 'Advance'),
(7, 'PAY-20240108-001', '2024-01-08', 254.25, 'Cash', 'Advance');