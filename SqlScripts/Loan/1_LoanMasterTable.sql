CREATE TABLE Loan (
    LoanId INT IDENTITY(1,1) PRIMARY KEY,
    LoanAccountNumber NVARCHAR(50) UNIQUE NOT NULL,
    LoanType NVARCHAR(50) NOT NULL,
    InitialLoanAmount DECIMAL(18, 2) NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    LoanTenureYears INT NOT NULL,
    CurrentInterestRate DECIMAL(8, 4) NOT NULL,
    InstallmentAmount DECIMAL(18, 2) NOT NULL,
    
    -- Dynamic fields
    LastInstallmentDate DATE NULL,
    FinalInstallmentDate DATE NOT NULL,
    NextInstallmentDate DATE NOT NULL,
    InstallmentsPaidTillDate INT NOT NULL DEFAULT 0,
    NoOfInstallmentsRemaining INT NOT NULL,
    TotalInstallments INT NOT NULL,
    OverDueAmount DECIMAL(18, 2) DEFAULT 0,
    OutstandingPrincipal DECIMAL(18, 2) NOT NULL,
    
    -- Status
    LoanStatus NVARCHAR(20) NOT NULL DEFAULT 'Active',
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    ModifiedDate DATETIME2 DEFAULT GETDATE()
);