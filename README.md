**README DLA DEWELOPERÓW**


Sposoby uruchomienia apki:
- Kiedy nie chcemy debugować c#:
  - Upewniamy się, że wcześniej uruchomiliśmy przynajmniej raz najnowszą wersję kodu w trybie release. W ten sposób Visual Studio instaluje program w systemie. Jest to potrzebne, bo **Launcher** uruchamia Piceona z cmd, czyli właśnie zainstalowaną w systemie wersję.
  - W projekcie **Launcher**, w pliku **Program.cs** odkomentowujemy poniższą linię, jeśli jest zakomentowana
    - <code>piceonProcess.Start();</code>
  - W VisualStudio, w górnej częsci okna zmieniamy aktywny projekt na Launcher.
  - Klikamy Ctrl+F5, uruchamiając tym samym **Launcher** bez debugowania.
  - Piceon i controller.pyw uruchomią się automatycznie i oba zamkną, kiedy zamknie się Piceona.
- Kiedy chcemy debugować c#:
  - W projekcie **Launcher**, w pliku **Program.cs** komentujemy linię 
    - <code>piceonProcess.Start();</code>
  - W VisualStudio, w górnej częsci okna zmieniamy aktywny projekt na Launcher.
  - Klikamy Ctrl+F5, uruchamiając tym samym **Launcher** bez debugowania.
  - Uruchamiamy Piceon w trybie debugowania.
  - Po zamknięciu Piceona, **Launcher** powinien zamknąć controller i siebie.

Żeby śledzic, co się dzieje na kolejkach rabbita, można zalogować się na stronę RabbitMQ Management, na waszego localhosta. Jak to zrobić, pod tym linkiem:
https://cmatskas.com/getting-started-with-rabbitmq-on-windows/ (punkt 3. Web Management).

Piceon korzysta z kolejek:
- front (c# na nią wysyła, controller.pyw z niej odbiera)
- back (c# z niej odbiera, controller.pyw na nią wysyła)
- launcher (c# na nią wysyła, Launcherw.pyw z niej odbiera)
