<h1>Api controllera</h1>

Kolejki:
-
- front, kolejka na która wrzuca C# i odbiera Python
- back, kolejka na którą wrzuca Python i odbiera C#


Dostpnę metody:
-
- "type":0 - wiadmosc testowa
- "type":1 - porgrupowanie podobnych zdjec



<h2>Korzystanie z metod:</h2>
-
- JSON w formie:<br>
<h4>{"taskid":int,"type":int,"images":[]}</h4><br>
np: {"taskid": 5678, "type": 1, "images": ["C:\\Users\\mgole\\OneDrive\\Obrazy\\Z aparatu\\WIN_20200417_23_43_44_Pro.jpg","nestpath"]}<br>
<h2>Zwrot</h2>
<h3>JSON w formie</h3>
<h4>
{"taskid": int, "result": string, "error_massage": string/null, "images": [[]]}
</h4><br>
<h2>Poprawne zapytanie</h2>
"taskid":int<br>
"result":"DONE"<br>
"error_massage": null<br>
"images":[[path1,path2],[pat3,path4]....]<br>
<h2>Błędy</h2>
<h3>
pole "result" ma wartosc = "ERR"
<h3>
<h3>OGÓLNE, error_massage = :</h3>
- -BAD PARAMS AND DATA oznacza puste zapytanie<br>
- -NO DATA oznacza brak danych<br>
- -BAD REQUEST oznacza probe trollowania<br>
- -LACK OF METHOD oznacza brak metody o podanej nazwie<br>

 

