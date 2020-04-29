**README DLA DEWELOPERÓW**


Sposoby uruchomienia apki:
- Kiedy nie chcemy debugować c#:
  - Upewniamy się, że wcześniej uruchomiliśmy przynajmniej raz najnowszą wersję kodu w trybie release. Jest to potrzebne, bo **Launcherw.pyw** uruchamia Piceona z cmd, czyli zainstalowaną w systemie wersję.
  - Klikamy dwukronie skrypt **PythonScripts/Launcherw.pyw**.
  - Piceon i controller.pyw uruchomią się automatycznie i oba zamkną, kiedy zamknie się Piceona.
- Kiedy chcemy debugować c#:
  - W skrypcie **Launcherw.pyw** komentujemy linię 
    - <code>piceon = subprocess.Popen('Piceon.exe')</code>
  - Klikamy dwukrotnie **Launcherw.pyw**. Uruchomi on **controller.pyw**.
  - Uruchamiamy Piceon w trybie debugowania.
  - Po zamknięciu Piceona, launcher powinien zamknąć controller i siebie.

Żeby śledzic, co się dzieje na kolejkach rabbita, można zalogować się na stronę RabbitMQ Management, na waszego localhosta. Jak to zrobić, pod tym linkiem:
https://cmatskas.com/getting-started-with-rabbitmq-on-windows/ (punkt 3. Web Management).

Piceon korzysta z kolejek:
- front (c# na nią wysyła, controller.pyw z niej odbiera)
- back (c# z niej odbiera, controller.pyw na nią wysyła)
- launcher (c# na nią wysyła, Launcherw.pyw z niej odbiera)
