[![.NET Core Desktop](https://github.com/zekchopper/geronimoNofity/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/zekchopper/geronimoNofity/actions/workflows/dotnet-desktop.yml)
 
 ## What is it?
Dohvaća popis aktualnih izleta sa geronimo stranice, uspoređuje sa prethodno dohvaćenim popisom i šalje post request na upisane adrese

## Postavljanje: 
u appsettings.json upisati geronimo username, app password (potrebno generirati u postavkama profila na stranici) i adrese gdje se šalje post request

Ako dođe do greške pokušat će grešku poslati na istu adresu, ako ne uspije onda dump u error.txt
