
# DapperSeries
Sample code for the Dapper Series of blog posts:

1) [Loading an Object from SQL Server using Dapper](https://www.davepaquette.com/archive/2018/01/22/loading-an-object-graph-with-dapper.aspx)

The sample project for this series of blog posts is an ASP.NET Core project containing api controllers. To test the project you will issue GET and POST to the api. 

## Requirements
.NET Core SDK v2.1.4 or newer
A SQL Server database (Localdb or any modern version of SQL Server will work fine)

## Creating a new database
The sample can be used to create a new database with some sample data. The default setting is to use localdb. If you do not have localdb installed, change the connection string in applicationSettings.json to point to an instance of SQL Server on your machine.

From the `src\DapperSeries` folder, build and run the app.

```
dotnet build
dotnet run
```

Using a tool like Postman, issue a post request to `localhost:5000\api\db\init`

![Creating the database](https://user-images.githubusercontent.com/2531875/35258071-5559d10e-ffba-11e7-92ef-06e06fe9b907.png)

A database containing tables and sample data should be created. If everything worked as expected, you will receive a 200 OK result. If not, you should get some HTML that will have an error buried in it somewhere.

## Testing Queries
To execute the code for various API controllers, send get requests and see the results. For example, to query for the `Aircraft` with ID 42, send a get request to `localhost:5000\api\aircraft\42\`
![Query for an aircraft](https://user-images.githubusercontent.com/2531875/35258148-cb31ff00-ffba-11e7-89dd-7975fa080fbc.png)
