## Running Unit Test Coverage
1. `dotnet tool install -g dotnet-coverage`
2. `dotnet tool install -g dotnet-reportgenerator-globaltool`
3. `dotnet-coverage collect -f cobertura -o coverage.xml -- dotnet test && reportgenerator -reports:coverage.xml -targetdir:coverage-report -reporttypes:Html`

## Areas of Investigation
1. DB access. Since I don't have transaction support cross table, is that an issue? Do I need to do read write retry loop for the background service when it polls for new work?