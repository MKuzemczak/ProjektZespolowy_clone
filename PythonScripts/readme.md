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

- KOD(spacja)PATH(spacja)sciezka\\do\\pliku.db<br>
- KOD(spacja)COMPARE(spacja){glowne id}(spacja){kolejne id} np:
    233 COMPARE  3 4 5 6 1 2
    takie zapytanie wywoła poruwnanie zdjec 4 5 6 1 2 ze zdjeciem 3<br>
*gdzie KOD to kod generowany przez apke po stronie UWP

<h2>Zwrot</h2>
<h3>zapytanie->odpowiedz</h3>
Jeżeli operacja się uda:
KOD-DONE<br>
np 233->DONE<br>
<h2>Błędy</h2>
<h3>OGÓLNE:</h3>
- -BAD PARAMS AND DATA oznacza puste zapytanie<br>
- -NO DATA oznacza brak danych<br>
- -BAD REQUEST oznacza probe trollowania<br>
- -LACK OF METHOD oznacza brak metody o podanej nazwie<br>
np 233-NO DATA
<h3>Dla PATH:</h3>
- -LACK OF FILE oznacza nie poprawna sciezke do bazy<br>
- -WRONG EXTENSION oznacza ze to jest plik ale nie .db<br>
np 233-LACK OF FILE
<h3>Dla COMPARE:</h3>
- -LACK OF PATH oznacza ze controller nie dostal poprawnej sciezki<br>
 wiec nie moze wywołac funkcji<br>
- -WRONG ID oznacza ze wsrod id jest nie Integer<br>
np 233-WRONG ID
 

