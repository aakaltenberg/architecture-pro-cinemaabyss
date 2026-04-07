## Изучите [README.md](README.md) файл и структуру проекта.

## Задание 1

1. Спроектируйте to be архитектуру КиноБездны, разделив всю систему на отдельные домены и организовав интеграционное взаимодействие и единую точку вызова сервисов.
Результат представьте в виде контейнерной диаграммы в нотации С4.
Добавьте ссылку на файл в этот шаблон

---------------------
1. cd cinema-arch-docs
2. mkdocs serve
3. в брауезере перейти по [cinema-c4-container](http://127.0.0.1:8000/cinema-c4-container/) 
--------------------

## Задание 2

### 1. Proxy
Команда КиноБездны уже выделила сервис метаданных о фильмах movies и вам необходимо реализовать бесшовный переход с применением паттерна Strangler Fig в части реализации прокси-сервиса (API Gateway), с помощью которого можно будет постепенно переключать траффик, используя фиче-флаг.


Реализуйте сервис на любом языке программирования в ./src/microservices/proxy.
Конфигурация для запуска сервиса через docker-compose уже добавлена
```yaml
  proxy-service:
    build:
      context: ./src/microservices/proxy
      dockerfile: Dockerfile
    container_name: cinemaabyss-proxy-service
    depends_on:
      - monolith
      - movies-service
      - events-service
    ports:
      - "8000:8000"
    environment:
      PORT: 8000
      MONOLITH_URL: http://monolith:8080
      #монолит
      MOVIES_SERVICE_URL: http://movies-service:8081 #сервис movies
      EVENTS_SERVICE_URL: http://events-service:8082 
      GRADUAL_MIGRATION: "true" # вкл/выкл простого фиче-флага
      MOVIES_MIGRATION_PERCENT: "50" # процент миграции
    networks:
      - cinemaabyss-network
```

- После реализации запустите postman тесты - они все должны быть зеленые.
- Отправьте запросы к API Gateway:
   ```bash
   curl http://localhost:8000/api/movies
   ```
- Протестируйте постепенный переход, изменив переменную окружения MOVIES_MIGRATION_PERCENT в файле docker-compose.yml.

-----------------------------------------------
Создан новый сервис proxy на базе платформы .NET
К запросам добавлено логирование пути. 
При смене  MOVIES_MIGRATION_PERCENT значения на 0, все запросы идут по пути 
2026-04-06 18:51:07       Routing /api/movies -> http://monolith:8080
При смене MOVIES_MIGRATION_PERCENT значения на 100, все запросы идут по пути:
2026-04-06 18:50:03       Routing /api/movies -> http://movies-service:8081
При смене MOVIES_MIGRATION_PERCENT значения на 50, запросы проходят как в монолит, так и в сервис movies-service:
2026-04-06 18:54:23       Routing /api/movies -> http://monolith:8080
2026-04-06 18:54:23       Routing /api/movies -> http://movies-service:8081

P.S. для того, чтобы изменения параметра MOVIES_MIGRATION_PERCENT применились, приложение требуется перезапустить 
(
  docker-compose down 
  docker-compose up -d
)

### 2. Kafka
 Вам как архитектуру нужно также проверить гипотезу насколько просто реализовать применение Kafka в данной архитектуре.

Для этого нужно сделать MVP сервис events, который будет при вызове API создавать и сам же читать сообщения в топике Kafka.

    - Разработайте сервис на любом языке программирования с consumer'ами и producer'ами.
    - Реализуйте простой API, при вызове которого будут создаваться события User/Payment/Movie и обрабатываться внутри сервиса с записью в лог
    - Добавьте в docker-compose новый сервис, kafka там уже есть

Необходимые тесты для проверки этого API вызываются при запуске npm run test:local из папки tests/postman 
Приложите скриншот тестов и скриншот состояния топиков Kafka http://localhost:8090 

-------------------------------------------------------
Создан MVP сервис events на базе платформы .NET.
Сервис включает в себя consumer и producer, работающие с топиками: movie-events, user-events, payment-events
А также RestAPI для взаимодействия, за основу был взят API, описанный api-specification.yaml
Результат выполнения тестов, скриншоты приложены по адресу:
1. /tests/results/KafkaAfterTests.png
2. /tests/results/TestsResult.png
PS C:\architecture-pro-cinemaabyss\tests\postman> npm run test:local

CinemaAbyss API Tests

□ Monolith Service
└ Health Check
  GET http://127.0.0.1:8080/health [200 OK, 124B, 103ms]
  √  Status code is 200

└ Get All Users
  GET http://127.0.0.1:8080/api/users [200 OK, 551B, 76ms]
  √  Status code is 200
  √  Response is an array

└ Create User
  POST http://127.0.0.1:8080/api/users [201 Created, 181B, 28ms]
  √  Status code is 201
  √  Response has id

└ Get User by ID
  GET http://127.0.0.1:8080/api/users?id=8 [200 OK, 176B, 11ms]
  √  Status code is 200
  √  User ID matches

└ Get All Movies
  GET http://127.0.0.1:8080/api/movies [200 OK, 2.58kB, 74ms]
  √  Status code is 200
  √  Response is an array

└ Create Movie
  POST http://127.0.0.1:8080/api/movies [201 Created, 246B, 17ms]
  √  Status code is 201
  √  Response has id

└ Get Movie by ID
  GET http://127.0.0.1:8080/api/movies?id=14 [200 OK, 241B, 9ms]
  √  Status code is 200
  √  Movie ID matches

└ Create Payment
  POST http://127.0.0.1:8080/api/payments [201 Created, 192B, 17ms]
  √  Status code is 201
  √  Response has id

└ Get Payment by ID
  GET http://127.0.0.1:8080/api/payments?id=8 [200 OK, 184B, 22ms]
  √  Status code is 200
  √  Payment ID matches

└ Create Subscription
  POST http://127.0.0.1:8080/api/subscriptions [201 Created, 235B, 29ms]
  √  Status code is 201
  √  Response has id

└ Get Subscription by ID
  GET http://127.0.0.1:8080/api/subscriptions?id=8 [200 OK, 230B, 6ms]
  √  Status code is 200
  √  Subscription ID matches

□ Movies Microservice
└ Health Check
  GET http://127.0.0.1:8081/api/movies/health [200 OK, 124B, 68ms]
  √  Status code is 200
  √  Status is true

└ Get All Movies
  GET http://127.0.0.1:8081/api/movies [200 OK, 2.72kB, 42ms]
  √  Status code is 200
  √  Response is an array

└ Create Movie
  POST http://127.0.0.1:8081/api/movies [201 Created, 282B, 23ms]
  √  Status code is 201
  √  Response has id

└ Get Movie by ID
  GET http://127.0.0.1:8081/api/movies?id=15 [200 OK, 277B, 10ms]
  √  Status code is 200
  √  Movie ID matches

□ Events Microservice
└ Health Check
  GET http://127.0.0.1:8082/api/events/health [200 OK, 163B, 610ms]
  √  Status code is 200
  √  Status is true

└ Create Movie Event
  POST http://127.0.0.1:8082/api/events/movie [201 Created, 206B, 846ms]
  √  Status code is 201
  √  Response has status success

└ Create User Event
  POST http://127.0.0.1:8082/api/events/user [201 Created, 205B, 659ms]
  √  Status code is 201
  √  Response has status success

└ Create Payment Event
  POST http://127.0.0.1:8082/api/events/payment [201 Created, 208B, 788ms]
  √  Status code is 201
  √  Response has status success

□ Proxy Service
└ Health Check
  GET http://127.0.0.1:8000/health [200 OK, 163B, 127ms]
  √  Status code is 200

└ Get All Movies via Proxy
  GET http://127.0.0.1:8000/api/movies [200 OK, 2.9kB, 98ms]
  √  Status code is 200
  √  Response is an array

└ Get All Users via Proxy
  GET http://127.0.0.1:8000/api/users [200 OK, 636B, 42ms]
  √  Status code is 200
  √  Response is an array

┌─────────────────────────┬────────────────────┬───────────────────┐
│                         │           executed │            failed │
├─────────────────────────┼────────────────────┼───────────────────┤
│              iterations │                  1 │                 0 │
├─────────────────────────┼────────────────────┼───────────────────┤
│                requests │                 22 │                 0 │
├─────────────────────────┼────────────────────┼───────────────────┤
│            test-scripts │                 22 │                 0 │
├─────────────────────────┼────────────────────┼───────────────────┤
│      prerequest-scripts │                  0 │                 0 │
├─────────────────────────┼────────────────────┼───────────────────┤
│              assertions │                 42 │                 0 │
├─────────────────────────┴────────────────────┴───────────────────┤
│ total run duration: 8.3s                                         │
├──────────────────────────────────────────────────────────────────┤
│ total data received: 10.14kB (approx)                            │
├──────────────────────────────────────────────────────────────────┤
│ average response time: 168ms [min: 6ms, max: 846ms, s.d.: 267ms] │
└──────────────────────────────────────────────────────────────────┘
Newman run completed!
Total requests: 22
Failed requests: 0
Total assertions: 42
Failed assertions: 0


## Задание 3

Команда начала переезд в Kubernetes для лучшего масштабирования и повышения надежности. 
Вам, как архитектору осталось самое сложное:
 - реализовать CI/CD для сборки прокси сервиса
 - реализовать необходимые конфигурационные файлы для переключения трафика.


### CI/CD

 В папке .github/worflows доработайте деплой новых сервисов proxy и events в docker-build-push.yml , чтобы api-tests при сборке отрабатывали корректно при отправке коммита в вашу новую ветку.

Нужно доработать 
```yaml
on:
  push:
    branches: [ main ]
    paths:
      - 'src/**'
      - '.github/workflows/docker-build-push.yml'
  release:
    types: [published]
```
и добавить необходимые шаги в блок
```yaml
jobs:
  build-and-push:
    runs-on: ubuntu-latest
    permissions:
      contents: read
      packages: write

    steps:
      - name: Checkout repository
        uses: actions/checkout@v3

      - name: Set up Docker Buildx
        uses: docker/setup-buildx-action@v2

      - name: Log in to the Container registry
        uses: docker/login-action@v2
        with:
          registry: ${{ env.REGISTRY }}
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}

```
Как только сборка отработает и в github registry появятся ваши образы, можно переходить к блоку настройки Kubernetes
Успешным результатом данного шага является "зеленая" сборка и "зеленые" тесты


### Proxy в Kubernetes

#### Шаг 1
Для деплоя в kubernetes необходимо залогиниться в docker registry Github'а.
1. Создайте Personal Access Token (PAT) https://github.com/settings/tokens . Создавайте class с правом read:packages
2. В src/kubernetes/*.yaml (event-service, monolith, movies-service и proxy-service)  отредактируйте путь до ваших образов 
```bash
 spec:
      containers:
      - name: events-service
        image: ghcr.io/ваш логин/имя репозитория/events-service:latest
```
3. Добавьте в секрет src/kubernetes/dockerconfigsecret.yaml в поле
```bash
 .dockerconfigjson: значение в base64 файла ~/.docker/config.json
```

4. Если в ~/.docker/config.json нет значения для аутентификации
```json
{
        "auths": {
                "ghcr.io": {
                       тут пусто
                }
        }
}
```
то выполните 

и добавьте

```json 
 "auth": "имя пользователя:токен в base64"
```

Чтобы получить значение в base64 можно выполнить команду
```bash
 echo -n ваш_логин:ваш_токен | base64
```

После заполнения config.json, также прогоните содержимое через base64

```bash
cat .docker/config.json | base64
```

и полученное значение добавляем в

```bash
 .dockerconfigjson: значение в base64 файла ~/.docker/config.json
```

#### Шаг 2

  Доработайте src/kubernetes/event-service.yaml и src/kubernetes/proxy-service.yaml

  - Необходимо создать Deployment и Service 
  - Доработайте ingress.yaml, чтобы можно было с помощью тестов проверить создание событий
  - Выполните дальшейшие шаги для поднятия кластера:

  1. Создайте namespace:
  ```bash
  kubectl apply -f src/kubernetes/namespace.yaml
  ```
  2. Создайте секреты и переменные
  ```bash
  kubectl apply -f src/kubernetes/configmap.yaml
  kubectl apply -f src/kubernetes/secret.yaml
  kubectl apply -f src/kubernetes/dockerconfigsecret.yaml
  kubectl apply -f src/kubernetes/postgres-init-configmap.yaml
  ```

  3. Разверните базу данных:
  ```bash
  kubectl apply -f src/kubernetes/postgres.yaml
  ```

  На этом этапе если вызвать команду
  ```bash
  kubectl -n cinemaabyss get pod
  ```
  Вы увидите

  NAME         READY   STATUS    
  postgres-0   1/1     Running   

  4. Разверните Kafka:
  ```bash
  kubectl apply -f src/kubernetes/kafka/kafka.yaml
  ```

  Проверьте, теперь должно быть запущено 3 пода, если что-то не так, то посмотрите логи
  ```bash
  kubectl -n cinemaabyss logs имя_пода (например - kafka-0)
  ```

  5. Разверните монолит:
  ```bash
  kubectl apply -f src/kubernetes/monolith.yaml
  ```
  6. Разверните микросервисы:
  ```bash
  kubectl apply -f src/kubernetes/movies-service.yaml
  kubectl apply -f src/kubernetes/events-service.yaml
  ```
  7. Разверните прокси-сервис:
  ```bash
  kubectl apply -f src/kubernetes/proxy-service.yaml
  ```

  После запуска и поднятия подов вывод команды 
  ```bash
  kubectl -n cinemaabyss get pod
  ```

  Будет наподобие такого

  NAME                              READY   STATUS    

  events-service-7587c6dfd5-6whzx   1/1     Running  

  kafka-0                           1/1     Running   

  monolith-8476598495-wmtmw         1/1     Running  

  movies-service-6d5697c584-4qfqs   1/1     Running  

  postgres-0                        1/1     Running  

  proxy-service-577d6c549b-6qfcv    1/1     Running  

  zookeeper-0                       1/1     Running 

  8. Добавим ingress

  - добавьте аддон
  ```bash
  minikube addons enable ingress
  --------
  в docker-desctop использовал kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.8.1/deploy/static/provider/cloud/deploy.yaml
  --------
  ```
  ```bash
  kubectl apply -f src/kubernetes/ingress.yaml
  ```
  9. Добавьте в /etc/hosts
  127.0.0.1 cinemaabyss.example.com

  10. Вызовите
  ```bash
  minikube tunnel

  в docker desctop использовал kubectl port-forward -n ingress-nginx service/ingress-nginx-controller 80:80
  ```
  11. Вызовите https://cinemaabyss.example.com/api/movies
  Вы должны увидеть вывод списка фильмов
  Можно поэкспериментировать со значением   MOVIES_MIGRATION_PERCENT в src/kubernetes/configmap.yaml и убедится, что вызовы movies уходят полностью в новый сервис

  12. Запустите тесты из папки tests/postman
  ```bash
   npm run test:kubernetes
  ```
  Часть тестов с health-чек упадет, но создание событий отработает.
  Откройте логи event-service и сделайте скриншот обработки событий

#### Шаг 3
Добавьте сюда скриншота вывода при вызове https://cinemaabyss.example.com/api/movies и  скриншот вывода event-service после вызова тестов.
----------------------------
Cкриншоты приложены по адресу:
1. Логи event-service после тестов: 
/tests/results/EventServiceLogs_Kuber.png

2. Вывод результата при вызове https://cinemaabyss.example.com/api/movies:
 /tests/results/cinemaabyss_example_com_api_movies.png
---------------------------

## Задание 4
Для простоты дальнейшего обновления и развертывания вам как архитектуру необходимо так же реализовать helm-чарты для прокси-сервиса и проверить работу 

Для этого:
1. Перейдите в директорию helm и отредактируйте файл values.yaml

```yaml
# Proxy service configuration
proxyService:
  enabled: true
  image:
    repository: ghcr.io/db-exp/cinemaabysstest/proxy-service
    tag: latest
    pullPolicy: Always
  replicas: 1
  resources:
    limits:
      cpu: 300m
      memory: 256Mi
    requests:
      cpu: 100m
      memory: 128Mi
  service:
    port: 80
    targetPort: 8000
    type: ClusterIP
```

- Вместо ghcr.io/db-exp/cinemaabysstest/proxy-service напишите свой путь до образа для всех сервисов
- для imagePullSecret проставьте свое значение (скопируйте из конфигурации kubernetes)
  ```yaml
  imagePullSecrets:
      dockerconfigjson: ewoJImF1dGhzIjogewoJCSJnaGNyLmlvIjogewoJCQkiYXV0aCI6ICJaR0l0Wlhod09tZG9jRjl2UTJocVZIa3dhMWhKVDIxWmFVZHJOV2hRUW10aFVXbFZSbTVaTjJRMFNYUjRZMWM9IgoJCX0KCX0sCgkiY3JlZHNTdG9yZSI6ICJkZXNrdG9wIiwKCSJjdXJyZW50Q29udGV4dCI6ICJkZXNrdG9wLWxpbnV4IiwKCSJwbHVnaW5zIjogewoJCSIteC1jbGktaGludHMiOiB7CgkJCSJlbmFibGVkIjogInRydWUiCgkJfQoJfSwKCSJmZWF0dXJlcyI6IHsKCQkiaG9va3MiOiAidHJ1ZSIKCX0KfQ==
  ```

2. В папке ./templates/services заполните шаблоны для proxy-service.yaml и events-service.yaml (опирайтесь на свою kubernetes конфигурацию - смысл helm'а сделать шаблоны для быстрого обновления и установки)

```yaml
template:
    metadata:
      labels:
        app: proxy-service
    spec:
      containers:
       Тут ваша конфигурация
```

3. Проверьте установку
Сначала удалим установку руками

```bash
kubectl delete all --all -n cinemaabyss
kubectl delete  namespace cinemaabyss
```
Запустите 
```bash
helm install cinemaabyss .\src\kubernetes\helm --namespace cinemaabyss --create-namespace
```
Если в процессе будет ошибка
```code
[2025-04-08 21:43:38,780] ERROR Fatal error during KafkaServer startup. Prepare to shutdown (kafka.server.KafkaServer)
kafka.common.InconsistentClusterIdException: The Cluster ID OkOjGPrdRimp8nkFohYkCw doesn't match stored clusterId Some(sbkcoiSiQV2h_mQpwy05zQ) in meta.properties. The broker is trying to join the wrong cluster. Configured zookeeper.connect may be wrong.
```

Проверьте развертывание:
```bash
kubectl get pods -n cinemaabyss
minikube tunnel
```

----------------------------
вместо minikube tunnel использовал для docker desctop kubectl port-forward -n ingress-nginx service/ingress-nginx-controller 80:80
----------------------------

Потом вызовите 
https://cinemaabyss.example.com/api/movies
и приложите скриншот развертывания helm и вывода https://cinemaabyss.example.com/api/movies

----------------------------
Cкриншоты приложены по адресу:
1. Выполнение команды kubectl get pods -n cinemaabyss: 
/tests/results/Task4_GetPodsCinemaabyss.png

2. Вывод результата при вызове https://cinemaabyss.example.com/api/movies:
 /tests/results/Task4_cinaabyss_example_com_api_movies.png
---------------------------

# Задание 5
Компания планирует активно развиваться и для повышения надежности, безопасности, реализации сетевых паттернов типа Circuit Breaker и канареечного деплоя вам как архитектору необходимо развернуть istio и настроить circuit breaker для monolith и movies сервисов.

```bash

helm repo add istio https://istio-release.storage.googleapis.com/charts
helm repo update

helm install istio-base istio/base -n istio-system --set defaultRevision=default --create-namespace
helm install istio-ingressgateway istio/gateway -n istio-system
helm install istiod istio/istiod -n istio-system --wait

helm install cinemaabyss .\src\kubernetes\helm --namespace cinemaabyss --create-namespace

kubectl label namespace cinemaabyss istio-injection=enabled --overwrite

kubectl get namespace -L istio-injection

kubectl apply -f .\src\kubernetes\circuit-breaker-config.yaml -n cinemaabyss

```

Тестирование

# fortio
```bash
kubectl apply -f https://raw.githubusercontent.com/istio/istio/release-1.25/samples/httpbin/sample-client/fortio-deploy.yaml -n cinemaabyss
```

# Get the fortio pod name
```bash
FORTIO_POD=$(kubectl get pod -n cinemaabyss | grep fortio | awk '{print $1}')

----------------------
Для windows:
$FORTIO_POD = kubectl get pod -n cinemaabyss | Select-String "fortio" | ForEach-Object { ($_ -split '\s+')[0] }
----------------------

kubectl exec -n cinemaabyss $FORTIO_POD -c fortio -- fortio load -c 50 -qps 0 -n 500 -loglevel Warning http://movies-service:8081/api/movies
```
Например,

```bash
kubectl exec -n cinemaabyss fortio-deploy-b6757cbbb-7c9qg  -c fortio -- fortio load -c 50 -qps 0 -n 500 -loglevel Warning http://movies-service:8081/api/movies
```

Вывод будет типа такого

```bash
IP addresses distribution:
10.106.113.46:8081: 421
Code 200 : 79 (15.8 %)
Code 500 : 22 (4.4 %)
Code 503 : 399 (79.8 %)
```
Можно еще проверить статистику

```bash
kubectl exec -n cinemaabyss fortio-deploy-b6757cbbb-7c9qg -c istio-proxy -- pilot-agent request GET stats | grep movies-service | grep pending
```
Для windows: 
kubectl exec -n cinemaabyss $FORTIO_POD -c istio-proxy -- pilot-agent request GET stats | Select-String "movies-service" | Select-String "pending"

И там смотрим 

```bash
cluster.outbound|8081||movies-service.cinemaabyss.svc.cluster.local;.upstream_rq_pending_total: 311 - столько раз срабатывал circuit breaker
You can see 21 for the upstream_rq_pending_overflow value which means 21 calls so far have been flagged for circuit breaking.
```

Приложите скриншот работы circuit breaker'а

Cкриншоты приложены по адресу:
1. Статистика, результат: 
/tests/results/Task5_statistic_result.png

Удаляем все
```bash
istioctl uninstall --purge
kubectl delete namespace istio-system
kubectl delete all --all -n cinemaabyss
kubectl delete namespace cinemaabyss
```
