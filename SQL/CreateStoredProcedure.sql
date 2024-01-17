CREATE OR REPLACE PROCEDURE CalcSumAndMedian(out sumOfInt numeric, out medianOfFloat numeric(10,8))
 LANGUAGE plpgsql AS
$$
DECLARE
   _count numeric;
begin
	select SUM("EvenNumber"), COUNT(*) into sumOfInt, _count FROM "RandomRows";
	select AVG("FloatNumber") into medianOfFloat FROM (select rr."FloatNumber" FROM "RandomRows" rr
		ORDER BY rr."FloatNumber"
		LIMIT (select ((_count+1) % 2)+1)
		OFFSET (_count/2));
end;
$$;

call CalcSumAndMedian(null, null);

