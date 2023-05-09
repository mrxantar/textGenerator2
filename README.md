# Генератор докуметов .docx со случайным набором текста.
Данная программа генерирует файлы с расширением .docx со случайными словами и знаками. Количество слов и файлов зависит от введённых данных.
<hr>
<h1>Инструкция по использованию:</h1>
Склонируйте репорзиторий

    git clone https://github.com/mrxantar/textGenerator2.git

1. Соберите проект, запустив файл .sln
2. Запустите получившийся .exe файл
3. В открывшемся окне заполните все поля, выберите директорию для сохранения файлов, а также файл с исходными словами из корневой папки. Вы также можете выбрать свой собственный файл со словами, но убедитесь что он сохранён в кодировке UTF-8, а также что каждое слово занимает отдельную строчку.
4. Нажмите кнопку Сгенерировать и ожидайте сообщения об окончании работы программы.
<hr>
<h1>Особенности:</h1>
Многопоточная программа позволяет создавать документы с огромным количеством слов за короткое время.<br>
Предусмотрена защита от неправильного ввода данных. Программа не позволит запустить себя, пока не будут удовлетворены все условия.<br>
Графический интерфейс написан с помощью фреймворка WPF.<br>
Программа запоминает выбранные пользователем директории, автоматически загружая их в следующем сеансе работы.

    
  
