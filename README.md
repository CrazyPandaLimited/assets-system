# Assets system

## Описание
Система для менеджмента ассетов в билде. Основана на паттерне DataFlow и позволяет построить логику работы с ассетами в проекте.

Основной список базовых фич, поставляемых из коробки:

- загрузка ассета из любого источника(локальная папка Resources, ассет бандли, веб сервер, локальная папка на диске)
- кеширование ассетов/бандлей, кеширование с подсчетом ссылок на ассеты
- организация логики очистки памяти
- дебаг состояния системы через отдельное окно редактора
- возможность расширения логики через добавление новых нод


## Зависимости
- [https://github.com/CrazyPandaLimited/OrderedNotifySet]()
- [https://github.com/CrazyPandaLimited/SimplePromises]()
- [https://github.com/CrazyPandaLimited/MessagesFlow]()


- Newtonsoft Json .Net(not included, used for manifest file preparing and examples)

## Описание системы и API

Система состоит из набора нод, которые соеденяются между собой. Нода это часть логики, выполняющая какую-то конкретную задачу. Система конструируется и настраивается специальным объектом, пример такого объекта это DefaultBuilder. Результатом работы билдера является новый объект AssetsStorage с подключенными к нему нодами. 

Класс AssetsStorage реализует публичный интерфейс взаимодействия с системой(IAssetsStorage) и содержит следующие методы:

#### AssetType LoadAssetSync< AssetType >( string url, MetaData metaData )
Этот метод и его перегрузки отвечают за синхронную загрузку ассета и бросают эксепшны в случае если в процессе прохождения графа в синхронном режиме начнется что-то асинхронное или указаны некорректные входные данные.

**url** - имя ассета, который нужно загрузить

**metaData** - дополнительная информация, которая может быть использована логикой внутри системы

**AssetType** - тип ассета, который надо загрузить

#### IPandaTask< AssetType > LoadAssetAsync< AssetType >( string url, MetaData metaData, CancellationToken tocken, IProgressTracker< float > tracker )
Этот метот и его перегрузки отвечают за асинхронную загрузку ассета. Бросают эксепшны в момент вызова в случаях, если указаны некорректные входные данные или произошла непредвиденная ошибка пока исполнялась синхронная часть кода.

**url** - имя ассета, который нужно загрузить

**metaData** - дополнительная информация, которая может быть использована логикой внутри системы

**tocken** - токен для возможности отмены асинхронного процесса

**tracker** - объект для отслеживания прогресса запроса

**AssetType** - тип ассета, который надо загрузить

**IPandaTask< AssetType >** - промис, который нотифицирует об успешности операции и предоставляет результат(либо загруженный ассет либо информацию об случившийся ошибке)



### Алгоритм работы системы 
После вызова одного из методов публичного апи всегда создается новый запрос(в соответствии с паттерном DataFlow и системой MessagesFlow). Далее этот запрос передается от ноды к ноде, пока не достигнет последней ноды либо не будет вызван токен отмены операции.

В случае вызова токена отмены операции промис сразу же переходит в состояние Rejected с ошибкой OperationCanceledException(но асинхронные процессы внутри нод системы могут еще какое-то время прекращать свою работу), нода, которая в момент вызова отмены обрабатывала запрос, прекратит его обрабатывать и не передаст дальше по цепочке.



	Важно!!!
	
	При построении графа нод нужно учитывать что начинаться граф должен в ноде AssetsStorage и все ветки графа должны заканчиваться одной из нод EndPointProcessor. Если в конце графа не будет такой ноды то промис не будет Resolved и соответственно будет всегда в состоянии ожидания результата(Pending) без каких-то ошибок.
	
	Ошибка будет только в том случае, если нода, которая находится последней в ветви, попытается передать запрос дальше и не окажется подключенной следующей ноды

### Кеширование ассетов системой
В подключенном к AssetsStorage графе могут быть ноды, которые добавляют/проверяют или достают что-то из кеша. После добавления загруженного ассета в кеш всегда встанет вопрос об удалении ассета из кеша и очистке памяти. Так вот AssetsStorage не имеет апи для очистки кешей!!! Такое ограничение связано с невозможностью привести все реализации кешей к одному интерфейсу и граф не обязан вообще иметь ноды для кеширования ассетов. Кеши передаются нодам как ссылки в момент конструирования системы и каждая реализация кеша имеет свои методы по очистке кеша и памяти. Предполагается что в случае необходимости удаления чего-то из кеша и очистке памяти будет напрямую вызван один из методов кеша. 

**НО! Система устроена таким образом, что можно организовать очистку определенного кеша используя существующее публичное апи!**

Для этого необходимо сделать следующее:

Вызывать один из методов загрузки с специальным параметром, который нужно добавить в metaData(Например проставить туда флаг ReleaseAssetFromCache)
Написать новую ноду, которая будет вызывать определенный метод в объекте кеша с определенными параметрами
Подключить новую ноду в дерево и указать при каких условиях запрос должен попасть в эту новую ветку(например наличие флага ReleaseAssetFromCache)

### Обработка ошибок
В случае вызова любого публичного апи системы, могут быть брошены ошибки синхронно(либо в случае некорректных входных данных, либо в случае непредвиденной ситуации) и такие ошибки ловятся прям в месте вызова.

В случае если вызвано синхронное публичное апи и в процессе загрузки произошла ошибка, то эта ошибка будет брошена в месте вызова апи(не в каллбеки и т.д.)

В случае если вызвано асинхронное апи, то в месте вызова будут брошены только ошибки некорректных входных данных либо в случае непредвиденной ситуации. Если ошибка возникнет в процессе обработки асинхронного запроса то ошибка будет находится в промисе, который возвращает асинхронное апи.

В случае если в процессе обработки запроса сломается какая-то нода, то нода перейдет в состояние Failed, будет вызвано событие OnStatusChanged c случившейся ошибкой. А сама ошибка будет в поле Exception сломавшейся ноды. Если ломается сама нода то запрос не будет передан дальше по цепочке и останется в состоянии Pending



