CREATE PROCEDURE udpDelEmployee (@id int, @err varchar(1000) out)
AS
BEGIN
	BEGIN TRY
		DELETE FROM Employee WHERE ReportsTo = @id;
		DELETE FROM Employee WHERE EmployeeId = @id;
		SET @err = ''
		RETURN (@@rowcount)
	END Try
	BEGIN CATCH
		SET @err = ERROR_MESSAGE()
		return (-1)
	END CATCH
END