CREATE OR REPLACE PROCEDURE CalcSumAndMedian is
	out sumOfInt numeric(20),
	out medianOfFloat numeric(10,8),
begin
	select SUM(EvenNumber) into sumOfInt FROM RandomRows;
	select AVG(FloatNumber) into medianOfFloat FROM RandomRows
		Order by FloatNumber
		LIMIT (select COUNT(*)/2 FROM RandomRows) [ OFFSET number ]
		;
end;