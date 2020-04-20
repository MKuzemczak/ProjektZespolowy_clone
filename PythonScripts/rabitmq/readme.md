<h1>Api controllera</h1>

Kolejki:
-
- front, kolejka na która wrzuca C# i odbiera Python
- back, kolejka na którą wrzuca Python i odbiera C#


Dostpnę metody:
-
- PATH - ustawienie sciezki do bazy danych
- COMPARE - porownanie zdjec i zapisanie w bazie
- LOCALISATION 


<h2>Korzystanie z metod:</h2>
-
<h3>Na początku jednorazowo należy ustawić sciezke, potem mozna 
korzystać z pozostałych metod</h3>

- PATH(spacja)sciezka\\do\\pliku.db
- COMPARE(spacja){glowne id}(spacja){kolejne id} np:
    COMPARE  3 4 5 6 1 2
    takie zapytanie wywoła poruwnanie zdjec 4 5 6 1 2 ze zdjeciem 3


<h2>Zwrot</h2>
<h3>zapytanie->odpowiedz</h3>
Jeżeli operacja się uda:
zapytanie->DONE
np COMPARE 4 1 2 3->DONE
<h2>Błędy</h2>
<h3>OGÓLNE:</h3>
- ->BAD PARAMS AND DATA oznacza puste zapytanie
- ->NO DATA oznacza brak danych
- ->BAD REQUEST oznacza probe trollowania

<h3>Dla PATH:</h3>
- ->BRAK PLIKU oznacza nie poprawna sciezke do bazy
- ->ZLE ROZSZERZENIE oznacza ze to jest plik ale nie .db

<h3>Dla COMPARE:</h3>
- ->BRAK SCIEZKI oznacza ze controller nie dostal poprawnej sciezki
 wiec nie moze wywołac funkcji
- ->NIE POPRAWNE ID oznacza ze wsrod id jest nie Integer

 

