**README DLA DEWELOPERÓW**

Przed uruchomieniem aplikacji należy upewnić się, że kolejki na localhost RabbitMQ, z których korzysta Piceon, są czyste. Mogą być na nich śmieci z poprzednich uruchomień, tak to jest przy debugowaniu. Funkcja czyszczenia kolejek przy uruchomieniu zostanie dodana w niedalekiej przyszłości.

Aby wyczyścić kolejki należy zalogować się na stronę RabbitMQ Management, na waszego localhosta. Jak to zrobić, pod tym linkiem:
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
- kliknąć **Get message(s)** 

Sposoby uruchomienia:
- Kiedy nie chcemy debugować c#:
  - Upewniamy się, że wcześniej uruchomiliśmy raz najnowszą wersję kody w trybie release. Jest to potrzebno, bo **Launcherw.pyw** uruchamia Piceona z cmd, czyli zainstalowaną wersję.
  - Upewniamy się, że kolejki RabbitMQ są puste.
  - Klikamy dwukronie skrypt **PythonScripts/Launcherw.pyw**.
  - Piceon i controller.pyw uruchomią się automatycznie i oba zamkną, kiedy zamknie się Piceona.
- Kiedy chcemy debugować c#:
  - W skrypcie **Launcherw.pyw** komentujemY linię 
    - <code>rabbit = subprocess.Popen('Piceon.exe')</code>
  - Upewniamy się, że kolejki RabbitMQ są puste.
  - Klikamy dwukrotnie **Launcherw.pyw**. uruchomi on **controller.pyw**
  - Uruchamiamy Piceon w trybie debugowania.
  - Po zamknięciu Piceona, launcher powinien zamknąć controller i siebie.