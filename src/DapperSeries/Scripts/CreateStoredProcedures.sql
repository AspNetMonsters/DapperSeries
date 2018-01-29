
CREATE PROCEDURE GetAircraftByModel @Model NVARCHAR(255) AS
BEGIN
    SELECT 
       Id
      ,Manufacturer
      ,Model
      ,RegistrationNumber
      ,FirstClassCapacity
      ,RegularClassCapacity
      ,CrewCapacity
      ,ManufactureDate
      ,NumberOfEngines
      ,EmptyWeight
      ,MaxTakeoffWeight
    FROM Aircraft a
    WHERE a.Model = @Model
END