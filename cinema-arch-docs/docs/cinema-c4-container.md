```puml
@startuml
title Cinema System Container Diagram

top to bottom direction

!includeurl https://raw.githubusercontent.com/RicardoNiepel/C4-PlantUML/master/C4_Container.puml

Person(user, "Пользователь", "Оформление подписок, просмотр контента, получение рекомендаций, информации о скидках")

Container_Boundary(cinema, "Cinema System") {
    Container(api_gateway, "API Gateway", "Kong", "Маршрутизация, аутентификация, переадресация, на первом этапе эту функцию возьмет proxy service")
    
    Container(user_service, "User Service", ".NET", "Управление пользователями")
    Container(movies_service, "Movies Service", "GO", "Управление метаданными о фильмах: жанры, актеры, оценки")
    Container(payment_service, "Payment Service", ".NET", "Управление платежами пользователей")
    Container(subscribe_service, "Subscribe Service", ".NET", "Управление подписками пользователей")
    Container(sale_service, "Sale Service", ".NET", "Управление скидками")
    Container(content_service, "Content Service", ".NET", "Сервис-провайдер к контенту")
    Container(event_service, "Event Service", ".NET", "Сервис хранения и обработки событий")
    Container(recommended_service, "Recommended Service", ".NET", "Сервис обработки внешних рекомендаций")
    
    Container(user_db, "User DB", "PostgreSQL", "Данные о пользователях")
    Container(movies_db, "Movies DB", "PostgreSQL", "Метаданные о фильмах (жанры, актеры, оценки)")
    Container(payment_db, "Payment DB", "PostgreSQL", "Данные о платежах пользователей")
    Container(subscribe_db, "Subscribe DB", "PostgreSQL", "Данные о подписках пользователей")
    Container(sale_db, "Sale DB", "PostgreSQL", "Данные о доступных скидках")
    Container(content_db, "Content DB", "PostgreSQL", "Данные, необходимые для получения доступа к контенту")
    Container(event_db, "Event DB", "PostgreSQL", "Данные о произошедших событиях в системе")
    Container(recommended_db, "Recommended DB", "PostgreSQL", "Данные о рекомендациях")
    
    Container(message_broker, "Message Broker", "Kafka", "События, произошедшие в системе")
}

System_Ext(s3_system, "Хранилище больших объектов", "S3", "Хранение контента")
System_Ext(online_cinema_system, "Онлайн кинотеатры", "RestAPI", "Система поставщик контента")
System_Ext(recommended_system, "Рекомендательная система", "RabbitMQ", "Рекомендательная система")

Rel(user, api_gateway, "Просмотр, оплата, подписки, получение рекомендаций, скидок", "HTTP")

Rel(api_gateway, user_service, "Маршрутизация", "HTTP")
Rel(api_gateway, movies_service, "Маршрутизация", "HTTP")
Rel(api_gateway, payment_service, "Маршрутизация", "HTTP")
Rel(api_gateway, subscribe_service, "Маршрутизация", "HTTP")
Rel(api_gateway, sale_service, "Маршрутизация", "HTTP")
Rel(api_gateway, content_service, "Маршрутизация", "HTTP")
Rel(api_gateway, event_service, "Маршрутизация", "HTTP")
Rel(api_gateway, recommended_service, "Маршрутизация", "HTTP")

Rel(user_service, user_db, "CRUD", "SQL")
Rel(movies_service, movies_db, "CRUD", "SQL")
Rel(payment_service, payment_db, "CRUD", "SQL")
Rel(subscribe_service, subscribe_db, "CRUD", "SQL")
Rel(sale_service, sale_db, "CRUD", "SQL")
Rel(content_service, content_db, "CRUD", "SQL")
Rel(event_service, event_db, "CRUD", "SQL")
Rel(recommended_service, recommended_db, "CRUD", "SQL")

Rel(user_service, message_broker, "Публикация событий", "Kafka Protocol")
Rel(movies_service, message_broker, "Публикация событий", "Kafka Protocol")
Rel(payment_service, message_broker, "Публикация событий", "Kafka Protocol")
Rel(subscribe_service, message_broker, "Публикация событий", "Kafka Protocol")
Rel(sale_service, message_broker, "Публикация событий", "Kafka Protocol")
Rel(content_service, message_broker, "Публикация событий", "Kafka Protocol")
Rel(event_service, message_broker, "Публикация событий, подписка на события", "Kafka Protocol")
Rel(recommended_service, message_broker, "Подписка на события", "Kafka Protocol")

Rel(recommended_service, recommended_system, "Отправка запросов на рекомендации, получение ответных сообщений с рекомендациями", "MQTT")
Rel(content_service, s3_system, "Получение контента", "HTTP")
Rel(content_service, online_cinema_system, "Переадресация/получение контента", "HTTP")

@enduml
```