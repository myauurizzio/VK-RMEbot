# VK-RMEbot
Бот для сообщества VK с поддержкой сценариев

Программа на dotNet Core 3.1 для работы с longPoll API VK.com

Подключается к личным сообщениям группы (сообщества) и общается с пользователями в интерактивном режиме.
Пользователь управляет программой при помощи кнопок в чате.

В алгоритмы включена базовая отказоустойчивость по отношению к плохим каналам связи.

Программа поддерживает (практически) неограниченное число сценариев с (практически) любым числом шагов.

Описание формата файлов сценариев - [VK-RMEbot script algo 20200412.pptx](VK-RMEbot%20script%20algo%2020200412.pptx)

Файловое окружение необходимое для корректной работы размещено в каталоге [bin/Release/netcoreapp3.1/publish](bin/Release/netcoreapp3.1/publish), включая демонстрационные сценарии и cmd-файл для запуска с предварительной установкой в консоли кодовой страницы 65001 (для поддержки русского языка на сервере с другим языком по умолчанию). 

Для работы программы требуется поместить в файлы _apikey, _appid, _groupid значения, полученные из раздела для Разработчиков [VK](https://vk.com/dev)

Так же, требуется активная учётная запись пользователя VK, логин и пароль от которой помещаются в файлы _login и _password, соответственно. 
Двухфакторная аутентификация этой учётной записи должна быть отключена.

Пошаговое описание настройки API на сайте VK и странице сообщества - [VK-RMEbot server settings 20200413.pptx](VK-RMEbot%20server%20settings%2020200413.pptx)


