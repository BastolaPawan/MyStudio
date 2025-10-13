CREATE TABLE InterestRateHistory (
    InterestHistoryId INT IDENTITY(1,1) PRIMARY KEY,
    LoanId INT NOT NULL FOREIGN KEY REFERENCES Loan(LoanId),
    InterestRate DECIMAL(8, 4) NOT NULL,
    EffectiveFrom DATE NOT NULL,
    EffectiveTill DATE NULL,
    ChangedByUser NVARCHAR(100) NOT NULL,
    ReasonForChange NVARCHAR(500) NULL,
    CreatedDate DATETIME2 DEFAULT GETDATE()
);