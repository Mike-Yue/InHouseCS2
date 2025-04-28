## Running Unit Test Coverage
1. `dotnet tool install -g dotnet-coverage`
2. `dotnet tool install -g dotnet-reportgenerator-globaltool`
3. `dotnet-coverage collect -f cobertura -o coverage.xml -- dotnet test && reportgenerator -reports:coverage.xml -targetdir:coverage-report -reporttypes:Html`