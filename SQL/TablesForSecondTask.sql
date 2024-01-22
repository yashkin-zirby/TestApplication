DROP VIEW public."AccountingView";
DROP TABLE public."AccountStatement";
DROP TABLE public."TurnoverStatement";
DROP TABLE public."TurnoverSheet";

CREATE TABLE public."TurnoverSheet"
(
    "SheetId" serial NOT NULL,
    "ReportYear" date NOT NULL,
    "BankName" character varying(100) NOT NULL,
    "Currency" character varying(10) NOT NULL,
    "FileName" character varying(260) NOT NULL,
    CONSTRAINT "PK_TURNOVERSHEET_SHEETID" PRIMARY KEY ("SheetId")
);
CREATE TABLE public."TurnoverStatement"
(
	"StatementId" serial NOT NULL,
	"AccountCode" numeric(4) NOT NULL,
    "TurnoverSheet" integer NOT NULL,
    "Debit" numeric(20, 2) NOT NULL,
    "Credit" numeric(20, 2) NOT NULL,
	CONSTRAINT "PK_TURNOVER_STATEMENT_ID" PRIMARY KEY ("StatementId"),
	CONSTRAINT "FK_ACCOUNTING_TURNOVERSHEET" FOREIGN KEY ("TurnoverSheet")
        REFERENCES public."TurnoverSheet" ("SheetId")
        ON DELETE CASCADE,
	CONSTRAINT "UNIQUE_ACCOUNT_SHEET_COMBINATION" UNIQUE ("AccountCode","TurnoverSheet")
);
CREATE TABLE public."AccountStatement"
(
	"Statement" int NOT NULL,
    "AccountType" character varying(7) NOT NULL,
    "OpeningBalance" numeric(20, 2) NOT NULL,
	CONSTRAINT "PK_ACCOUNT_STATEMENT" PRIMARY KEY ("Statement"),
    CONSTRAINT "FK_ACCOUNTING_TURNOVER_STATEMENT" FOREIGN KEY ("Statement")
        REFERENCES public."TurnoverStatement" ("StatementId")
        ON DELETE CASCADE,
    CONSTRAINT "CHECK_ACCOUNT_TYPE" CHECK ("AccountType" in ('ACTIVE','PASSIVE'))
);
CREATE VIEW public."AccountingView" as
select ts."TurnoverSheet", ts."AccountCode", ts."Debit", ts."Credit",
		(case when ast."AccountType" = 'ACTIVE' then ast."OpeningBalance" else 0 END) "OpeningBalanceActive",
		(case when ast."AccountType" = 'PASSIVE' then ast."OpeningBalance" else 0 END) "OpeningBalancePassive"
		from public."TurnoverStatement" ts LEFT JOIN public."AccountStatement" ast
			on ts."StatementId" = ast."Statement" ORDER BY ts."AccountCode";
select * from "AccountingView";
select * from "AccountStatement";
DELETE FROM "TurnoverSheet";
