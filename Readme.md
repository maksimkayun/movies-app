# Movies App
## By Maks Kayun

![N|Solid](https://rut-miit.ru/content/logo_flagstripe_ministryeagle_ministry_eagle_rut_2.svg?id_wm=900277)

[![Build Status](https://travis-ci.org/joemccann/dillinger.svg?branch=master)](https://travis-ci.org/joemccann/dillinger)

Этот проект сделан для демонстрации работы ASP.NET Core, а так же для демонстрации применения MVC при проектировании веб-приложений.

## Настройка

- Скачайте к себе проект
- Дальше установите SSMS и SQL Server от Microsoft (https://www.microsoft.com/ru-ru/sql-server/sql-server-2019)
- Откройте терминал в среде разработки
- Используйте команды: 
```sh
dotnet tool install --global dotnet-ef
dotnet-ef migrations add init --context MoviesApp.Data.MoviesContext --output-dir Data/Migrations
dotnet-ef database update
```
- Подробнее о командах: https://github.com/maksimkayun/files-for-projects-javafx-C-sharp/blob/master/scaffold%20-%20migrations%20and%20mssql.txt
- Проверьте миграцию БД через SSMS
- Пользуйтесь!
