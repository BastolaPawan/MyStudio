CREATE TABLE LoanInstallment (
    InstallmentId INT IDENTITY(1,1) PRIMARY KEY,
    LoanId INT NOT NULL FOREIGN KEY REFERENCES Loan(LoanId),
    InstallmentNumber INT NOT NULL,
    DueDate DATE NOT NULL,
    InstallmentAmount DECIMAL(18, 2) NOT NULL,
    PrincipalComponent DECIMAL(18, 2) NOT NULL,
    InterestComponent DECIMAL(18, 2) NOT NULL,
    OpeningBalance DECIMAL(18, 2) NOT NULL,
    ClosingBalance DECIMAL(18, 2) NOT NULL,
    
    -- Payment tracking
    PaidDate DATE NULL,
    PaidAmount DECIMAL(18, 2) DEFAULT 0,
    PrincipalPaid DECIMAL(18, 2) DEFAULT 0,
    InterestPaid DECIMAL(18, 2) DEFAULT 0,
    LateFee DECIMAL(18, 2) DEFAULT 0,
    
    -- Status
    InstallmentStatus NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    
    CONSTRAINT UK_Loan_Installment UNIQUE (LoanId, InstallmentNumber)
);