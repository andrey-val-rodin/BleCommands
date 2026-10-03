dotnet run -- --coverage --coverage-output-format xml --coverage-output coverage.xml
reportgenerator -reports:D:\Source\BleCommands\Source\IntegrationTests.Windows\bin\Debug\net9.0-windows10.0.17763.0\TestResults\coverage.xml -targetdir:D:\Dev\IntegrationTestsCoverage
start D:\Dev\IntegrationTestsCoverage\index.html
pause
