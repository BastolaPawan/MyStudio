CREATE TABLE LoanTransaction (
    TransactionId INT IDENTITY(1,1) PRIMARY KEY,
    LoanId INT NOT NULL FOREIGN KEY REFERENCES Loan(LoanId),
    InstallmentId INT NULL FOREIGN KEY REFERENCES LoanInstallment(InstallmentId),
    TransactionDate DATE NOT NULL,
    TransactionType NVARCHAR(20) NOT NULL,
    Amount DECIMAL(18, 2) NOT NULL,
    PrincipalAmount DECIMAL(18, 2) NOT NULL,
    InterestAmount DECIMAL(18, 2) NOT NULL,
    LateFeeAmount DECIMAL(18, 2) DEFAULT 0,
    PaymentMethod NVARCHAR(50),
    ReferenceNumber NVARCHAR(100),
    Remarks NVARCHAR(500),
    CreatedDate DATETIME2 DEFAULT GETDATE()
);