CREATE TABLE IF NOT EXISTS public."RandomRows"
(
	"RowId" serial NOT NULL primary key,
    "RandomDate" date NOT NULL,
    "LatinString" character(10) COLLATE pg_catalog."default" NOT NULL,
    "RussianString" character(20) COLLATE pg_catalog."default" NOT NULL,
    "EvenNumber" numeric(8,0) NOT NULL,
    "FloatNumber" numeric(10,8) NOT NULL
) TABLESPACE pg_default;
DROP TABLE public."RandomRows";