**README DLA DEWELOPERÓW**

Przed uruchomieniem aplikacji należy upewnić się, że kolejki na localhost RabbitMQ, z których korzysta Piceon, są czyste. Mogą być na nich śmieci z poprzednich uruchomień, tak to jest przy debugowaniu. Funkcja automatycznego czyszczenia kolejek przy uruchomieniu zostanie dodana w niedalekiej przyszłości.

Obecnie, aby wyczyścić kolejki należy zalogować się na stronę RabbitMQ Management, na waszego localhosta. Jak to zrobić, pod tym linkiem:
https://cmatskas.com/getting-started-with-rabbitmq-on-windows/ (punkt 3. Web Management).

Piceon korzysta z kolejek:
- front (c# na nią wysyła, controller.pyw z niej odbiera)
- back (c# z niej odbiera, controller.pyw na nią wysyła)
- launcher (c# na nią wysyła, Launcherw.pyw z niej odbiera)

Aby wyczyścić kolejkę ze śmieci, należy:
- w menedżerze rabbitmq, w zakładce **Queues**, kliknąć nazwę kolejki, 
- przescrollować w dół do sekcji **Get messages** 
- zmienić **Ack Mode** na **Ack message requeue false**
- w polu **Messages** wpisać ilość wiadomości, które chcecie odebrać, czyli usunąć z kolejki.
- kliknąć **Get message(s)**.

Sposoby uruchomienia apki:
- Kiedy nie chcemy debugować c#:
  - Upewniamy się, że wcześniej uruchomiliśmy przynajmniej raz najnowszą wersję kodu w trybie release. Jest to potrzebne, bo **Launcherw.pyw** uruchamia Piceona z cmd, czyli zainstalowaną w systemie wersję.
  - Upewniamy się, że kolejki RabbitMQ są puste.
  - Klikamy dwukronie skrypt **PythonScripts/Launcherw.pyw**.
  - Piceon i controller.pyw uruchomią się automatycznie i oba zamkną, kiedy zamknie się Piceona.
- Kiedy chcemy debugować c#:
  - W skrypcie **Launcherw.pyw** komentujemy linię 
    - <code>piceon = subprocess.Popen('Piceon.exe')</code>
  - Upewniamy się, że kolejki RabbitMQ są puste.
  - Klikamy dwukrotnie **Launcherw.pyw**. Uruchomi on **controller.pyw**.
  - Uruchamiamy Piceon w trybie debugowania.
  - Po zamknięciu Piceona, launcher powinien zamknąć controller i siebie.