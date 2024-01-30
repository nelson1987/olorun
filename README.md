# olorun
Netcore + Mongodb + Postgresql + Redis + Kafka

Criaremos uma API de Cadastro
Um método Get que retornará todas as contas cadastradas
Um método Post que criará uma conta nova

Feature - Buscar contas cadastradas
Será requisitado à base cadastro todas as contas cadastradas.

Feature - Criar conta
Teremos uma Api Gateway de Cadastro, ela fará uma requisição HTTP para a Api de AntiFraude
ao retornar sucesso, ela enviará um evento para o tópico conta-cadastrada.
Este tópico será consumido por 2 APIs Conta e Cliente, cada 1 delas guardará o dado em seus respectivos bancos de dados, e enviará um evento para o tópico conta aberta.
Teremos um worker de Contas abertas que alterará esses dados em na base de cadastro.


# references
https://github.com/aspnetrun/run-aspnetcore-microservices 
https://medium.com/c-sharp-progarmming/the-basics-of-net-integration-testing-2ad1a0de54ed 
https://www.codeproject.com/Articles/5267948/Integration-Testing-More-Fixtures-than-AutoFixture 
https://wrapt.dev/blog/integration-tests-in-dotnet-without-webappfactory 
https://timdeschryver.dev/blog/how-to-test-your-csharp-web-api#conclusion 
https://www.freecodecamp.org/news/learn-tdd-with-integration-tests-in-net-5-0/ 

'''
{
	"Id": "8fbd0093-685b-4b99-9654-f30f54cd3daa",
	"Date": "2024-01-16",
	"TemperatureC": 15,
	"Summary": "Cool",
	"TemperatureF": 32
}
'''