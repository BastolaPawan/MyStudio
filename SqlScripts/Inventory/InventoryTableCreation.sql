CREATE TABLE Units (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(10) NOT NULL UNIQUE,
    Name NVARCHAR(50) NOT NULL,
    Description NVARCHAR(500) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    IsActive BIT NOT NULL DEFAULT 1
);

-- Sample data for photo studio
INSERT INTO Units (Code, Name, Description) VALUES
('PCS', 'Pieces', 'Individual items'),
('BOX', 'Box', 'Items packaged in boxes'),
('ROLL', 'Roll', 'Roll-based items like paper'),
('PKT', 'Packet', 'Small packets'),
('SET', 'Set', 'Item sets');

CREATE TABLE ItemGroups (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    Code NVARCHAR(20) NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    IsActive BIT NOT NULL DEFAULT 1
);

-- Sample data for photo studio
INSERT INTO ItemGroups (Name, Code, Description) VALUES
('Camera Equipment', 'CAM', 'Cameras, lenses, and accessories'),
('Lighting', 'LIGHT', 'Studio lights, flashes, modifiers'),
('Background', 'BG', 'Backdrops and background systems'),
('Printing', 'PRINT', 'Photo paper, ink, printing supplies'),
('Accessories', 'ACC', 'Miscellaneous photography accessories');


CREATE TABLE ItemSubGroups (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    Code NVARCHAR(20) NOT NULL,
    Description NVARCHAR(500) NULL,
    ItemGroupId INT NOT NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_ItemSubGroups_ItemGroups FOREIGN KEY (ItemGroupId) REFERENCES ItemGroups(Id),
    CONSTRAINT UK_ItemSubGroups_Code_Group UNIQUE (Code, ItemGroupId)
);

-- Sample data for photo studio
INSERT INTO ItemSubGroups (Name, Code, Description, ItemGroupId) VALUES
('DSLR Cameras', 'DSLR', 'Digital SLR cameras', 1),
('Mirrorless', 'MIRROR', 'Mirrorless cameras', 1),
('Lenses', 'LENS', 'Camera lenses', 1),
('Studio Lights', 'STUDIO_LGT', 'Continuous studio lighting', 2),
('Speedlights', 'SPEED', 'Portable flash units', 2),
('Paper Backdrops', 'PAPER_BG', 'Paper roll backdrops', 3),
('Fabric Backdrops', 'FABRIC_BG', 'Muslin and fabric backgrounds', 3),
('Photo Paper', 'PHOTO_PAPER', 'Various photo papers', 4),
('Ink Cartridges', 'INK', 'Printer ink cartridges', 4);



CREATE TABLE Items (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SKU NVARCHAR(50) NOT NULL UNIQUE,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(1000) NULL,
    Brand NVARCHAR(100) NULL,
    Model NVARCHAR(100) NULL,
    UnitId INT NOT NULL,
    ItemGroupId INT NOT NULL,
    ItemSubGroupId INT NULL,
    MinimumStock DECIMAL(18,2) NOT NULL DEFAULT 0,
    MaximumStock DECIMAL(18,2) NOT NULL DEFAULT 0,
    ReorderLevel DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Items_Units FOREIGN KEY (UnitId) REFERENCES Units(Id),
    CONSTRAINT FK_Items_ItemGroups FOREIGN KEY (ItemGroupId) REFERENCES ItemGroups(Id),
    CONSTRAINT FK_Items_ItemSubGroups FOREIGN KEY (ItemSubGroupId) REFERENCES ItemSubGroups(Id)
);

-- Sample data for photo studio
INSERT INTO Items (SKU, Name, Description, Brand, Model, UnitId, ItemGroupId, ItemSubGroupId, MinimumStock, MaximumStock, ReorderLevel) VALUES
('CAM-DSLR-001', 'Professional DSLR Camera', '24MP DSLR camera body', 'Canon', 'EOS 5D Mark IV', 1, 1, 1, 2, 10, 3),
('CAM-MIR-001', 'Mirrorless Camera', 'Full frame mirrorless camera', 'Sony', 'A7 III', 1, 1, 2, 2, 8, 3),
('LENS-50MM', '50mm Prime Lens', 'Standard prime lens f/1.8', 'Canon', 'EF 50mm f/1.8', 1, 1, 3, 3, 15, 5),
('LIGHT-STUDIO-001', 'Studio Flash Light', '300W studio strobe light', 'Godox', 'MS300', 1, 2, 4, 2, 10, 4),
('BG-PAPER-107', 'Paper Backdrop 107"', 'White seamless paper backdrop 107" wide', 'Superior', '107" White', 3, 3, 6, 5, 30, 10),
('BG-MUS-BLACK', 'Black Muslin Backdrop', '10x12 ft black muslin backdrop', 'Neewer', '10x12 Black', 1, 3, 7, 3, 15, 5),
('PAPER-GLOSS-8x10', 'Glossy Photo Paper 8x10', 'High gloss photo paper 8x10 inches', 'Epson', 'Premium Glossy', 4, 4, 8, 100, 1000, 200),
('INK-CYAN-001', 'Cyan Ink Cartridge', 'Cyan ink for photo printer', 'Canon', 'PGI-280 Cyan', 1, 4, 9, 5, 50, 10);



CREATE TABLE Vendors (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL UNIQUE,
    ContactPerson NVARCHAR(100) NULL,
    Email NVARCHAR(100) NULL,
    Phone NVARCHAR(20) NULL,
    Address NVARCHAR(500) NULL,
    TaxNumber NVARCHAR(50) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    IsActive BIT NOT NULL DEFAULT 1
);

-- Sample data for photo studio
INSERT INTO Vendors (Name, ContactPerson, Email, Phone, Address, TaxNumber) VALUES
('Camera World Ltd.', 'John Smith', 'john@cameraworld.com', '+1-555-0101', '123 Camera Street, Photo City', 'TXN-001'),
('Lighting Solutions Inc.', 'Sarah Johnson', 'sarah@lightingsol.com', '+1-555-0102', '456 Light Avenue, Studio Town', 'TXN-002'),
('Paper Supply Co.', 'Mike Davis', 'mike@papersupply.co', '+1-555-0103', '789 Paper Road, Printville', 'TXN-003'),
('Photo Accessories Mart', 'Emily Wilson', 'emily@photoacc.com', '+1-555-0104', '321 Accessory Lane, Gear City', 'TXN-004');



CREATE TABLE Purchases (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PurchaseNumber NVARCHAR(20) NOT NULL UNIQUE,
    PurchaseDate DATETIME2 NOT NULL,
    DeliveryDate DATETIME2 NULL,
    VendorId INT NOT NULL,
    SubTotal DECIMAL(18,2) NOT NULL DEFAULT 0,
    TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    ShippingCost DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Notes NVARCHAR(1000) NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Draft',
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Purchases_Vendors FOREIGN KEY (VendorId) REFERENCES Vendors(Id)
);

-- Sample purchase data
INSERT INTO Purchases (PurchaseNumber, PurchaseDate, VendorId, SubTotal, TaxAmount, ShippingCost, TotalAmount, Status) VALUES
('PUR-2024-001', '2024-01-15', 1, 2500.00, 325.00, 50.00, 2875.00, 'Received'),
('PUR-2024-002', '2024-01-20', 2, 1200.00, 156.00, 30.00, 1386.00, 'Received'),
('PUR-2024-003', '2024-02-01', 3, 450.00, 58.50, 25.00, 533.50, 'Ordered');




CREATE TABLE PurchaseItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PurchaseId INT NOT NULL,
    ItemId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    DiscountPercent DECIMAL(18,2) NOT NULL DEFAULT 0,
    TaxPercent DECIMAL(18,2) NOT NULL DEFAULT 0,
    LineTotal DECIMAL(18,2) NOT NULL,
    ActualUnitPrice DECIMAL(18,2) NULL,
    QuantityReceived INT NULL,
    BatchNumber NVARCHAR(100) NULL,
    ExpiryDate DATETIME2 NULL,
    CONSTRAINT FK_PurchaseItems_Purchases FOREIGN KEY (PurchaseId) REFERENCES Purchases(Id),
    CONSTRAINT FK_PurchaseItems_Items FOREIGN KEY (ItemId) REFERENCES Items(Id)
);

-- Sample purchase items data
INSERT INTO PurchaseItems (PurchaseId, ItemId, Quantity, UnitPrice, DiscountPercent, TaxPercent, LineTotal, QuantityReceived, BatchNumber) VALUES
(1, 1, 2, 1200.00, 5.00, 13.00, 2280.00, 2, 'BATCH-CAM-001'),
(1, 3, 3, 150.00, 0.00, 13.00, 450.00, 3, 'BATCH-LENS-001'),
(2, 4, 2, 550.00, 8.00, 13.00, 1012.00, 2, 'BATCH-LIGHT-001'),
(2, 6, 2, 85.00, 0.00, 13.00, 170.00, 2, 'BATCH-BG-001'),
(3, 7, 200, 1.80, 10.00, 13.00, 324.00, 200, 'BATCH-PAPER-001'),
(3, 8, 10, 12.00, 5.00, 13.00, 120.00, 10, 'BATCH-INK-001');




CREATE TABLE StockMovements (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ItemId INT NOT NULL,
    MovementDate DATETIME2 NOT NULL,
    MovementType NVARCHAR(20) NOT NULL,
    Quantity INT NOT NULL,
    UnitCost DECIMAL(18,2) NOT NULL,
    Reference NVARCHAR(100) NOT NULL,
    Notes NVARCHAR(500) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_StockMovements_Items FOREIGN KEY (ItemId) REFERENCES Items(Id)
);

-- Sample stock movements data
INSERT INTO StockMovements (ItemId, MovementDate, MovementType, Quantity, UnitCost, Reference, Notes) VALUES
(1, '2024-01-16', 'Purchase', 2, 1200.00, 'PUR-2024-001', 'Initial stock from purchase'),
(3, '2024-01-16', 'Purchase', 3, 150.00, 'PUR-2024-001', 'Initial stock from purchase'),
(4, '2024-01-21', 'Purchase', 2, 550.00, 'PUR-2024-002', 'Lighting equipment purchase'),
(6, '2024-01-21', 'Purchase', 2, 85.00, 'PUR-2024-002', 'Backdrop purchase'),
(7, '2024-02-02', 'Purchase', 200, 1.80, 'PUR-2024-003', 'Photo paper stock'),
(8, '2024-02-02', 'Purchase', 10, 12.00, 'PUR-2024-003', 'Ink cartridge stock'),
(7, '2024-02-10', 'Sale', -50, 1.80, 'SALE-001', 'Used for client orders'),
(8, '2024-02-12', 'Sale', -2, 12.00, 'SALE-002', 'Printer maintenance');




-- View for current stock levels
CREATE VIEW CurrentStockLevels AS
SELECT 
    i.Id,
    i.SKU,
    i.Name,
    i.Brand,
    i.Model,
    u.Name AS UnitName,
    ig.Name AS ItemGroupName,
    isg.Name AS ItemSubGroupName,
    i.MinimumStock,
    i.MaximumStock,
    i.ReorderLevel,
    COALESCE(SUM(sm.Quantity), 0) AS CurrentStock,
    CASE 
        WHEN COALESCE(SUM(sm.Quantity), 0) <= i.ReorderLevel THEN 'LOW STOCK'
        WHEN COALESCE(SUM(sm.Quantity), 0) = 0 THEN 'OUT OF STOCK'
        ELSE 'IN STOCK'
    END AS StockStatus
FROM Items i
LEFT JOIN StockMovements sm ON i.Id = sm.ItemId
LEFT JOIN Units u ON i.UnitId = u.Id
LEFT JOIN ItemGroups ig ON i.ItemGroupId = ig.Id
LEFT JOIN ItemSubGroups isg ON i.ItemSubGroupId = isg.Id
WHERE i.IsActive = 1
GROUP BY i.Id, i.SKU, i.Name, i.Brand, i.Model, u.Name, ig.Name, isg.Name, i.MinimumStock, i.MaximumStock, i.ReorderLevel;

-- View for purchase history
CREATE VIEW PurchaseHistory AS
SELECT 
    p.PurchaseNumber,
    p.PurchaseDate,
    v.Name AS VendorName,
    i.SKU,
    i.Name AS ItemName,
    pi.Quantity,
    pi.UnitPrice,
    pi.DiscountPercent,
    pi.LineTotal,
    pi.QuantityReceived,
    p.Status
FROM Purchases p
JOIN Vendors v ON p.VendorId = v.Id
JOIN PurchaseItems pi ON p.Id = pi.PurchaseId
JOIN Items i ON pi.ItemId = i.Id
WHERE p.Status != 'Cancelled';





-------------------indexes


-- Create indexes for better performance
CREATE INDEX IX_StockMovements_ItemId ON StockMovements(ItemId);
CREATE INDEX IX_StockMovements_MovementDate ON StockMovements(MovementDate);
CREATE INDEX IX_PurchaseItems_PurchaseId ON PurchaseItems(PurchaseId);
CREATE INDEX IX_PurchaseItems_ItemId ON PurchaseItems(ItemId);
CREATE INDEX IX_Purchases_VendorId ON Purchases(VendorId);
CREATE INDEX IX_Purchases_PurchaseDate ON Purchases(PurchaseDate);
CREATE INDEX IX_Items_SKU ON Items(SKU);
CREATE INDEX IX_Items_ItemGroupId ON Items(ItemGroupId);



