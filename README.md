Dear Xeros,
I did a first inspection of the original _"ReformatThis"_ API project.  

Several have been the reasons for which I decided to replace it instead of refactoring it. Mainly, because it fails uncountable Non-Functional Requirement criteria.
Technologies, poor design, lack of testability, and several tight dependencies, just to mention some...

The most evident are:

 1.	**Old dev framework**: .NET Frameworks 4.5.2 -> Accordingly to: (https://devblogs.microsoft.com/dotnet/net-framework-4-5-2-4-6-4-6-1-will-reach-end-of-support-on-april-26-2022/) Microsoft will end the support on April 26, 2022.  
	I pointed to the new ".NET 6 minimal API" because of the great characteristics: https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-6#:~:text=Better%20performance%3A%20.,tools%2C%20and%20better%20team%20collaboration.
	Natively supports SSL.

 2.	**Synchronous code - lack of scalability** all the API and DB access are synchronous.
	I redesigned all the APIs and DB acces to Async.

 3. **Models into the solution project** 
	Now, are in a separated DLL, to be shared over multiple projects.

 4.	**Models classes tight to DB structure** This makes impossible to update the code separately from the DB structure, and no protection from SQL injection.
	Now Models are based on Interface and have no dependencies on persistence technology. **For this exercise, I opted in favour of EntityFramework using the InMemoryDB capability, to ease your tests.**  
    In case of a production scenario, to support performances, scalability and better separation of concerns, I would have used at least: a DB Migrations technology and using Stored Procedures, great to decouple the code from the DB structure, and also providing top performances.

 5.	**Do not support Versioning**  
	NOTICE: also *.NET 6 Minimal API* doesn't support versioning (planned for .NET 7).  However, my proposed solution results easy to clone and refactor in favour of URI, models and DB structure versioning, best practice for resilience having micro-services scenario in mind.
 
 6. **Platform compatibility** the current solution targets only Windows.
	The new solution supports: Win/Linux/iOs, can be deployed to Docker container and also supports Arm64 processors.

 7. **Logs** No logs in the original project.
	I included Serilog that integrates smootly on Minimal API and has a huge variety of sinks.  It supports the an external file configuration. I decided the code setting, to ease your tests.
	I included an example of "Correlation Id", best practice in hypermedia 

 8. I also included **Swagger** for your convenience and tests. 

 9. **Security** The original **"ReformatThis"** project, has no Authentication nor Authorisation concepts. 
	I have included the middleware.  However, no decorations included.

10. **Tests** The original **"ReformatThis"** project, has no test projects.
	Given the lack of business logic, I haven't added any unit-test nor integration-test projects.
