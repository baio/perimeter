# E2E

+ Run microsoft local storage emulator
+ psql
+ configs  `appsettings.e2e.json` `akka.e2e.hocon`
+ build configuration `E2E`
+ run configuration `PRR.API.E2E` configuration

## E2E compile flag
+ Adds `e2e/reset` endpoint to reset database
+ Set `HASH` static token for user signup confirmation
+ Disable authorization token expiration check

