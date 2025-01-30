# dotNetDDDScaffold

CLI desenvolvido com o objetivo de otimizar a criação de um projeto usando DDD (Domain-Driven Design) e deixando de lado o processo custoso de configuração.

## Índice

- [Pré-requisitos](#pré-requisitos)
- [Instalação](#instalação)
- [Uso](#uso)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Executando o Projeto](#executando-o-projeto)
- [Contribuição](#contribuição)
- [Licença](#licença)

## Pré-requisitos

Antes de começar, certifique-se de ter o seguinte instalado em sua máquina:

- [.NET SDK 6.0 ou superior](https://dotnet.microsoft.com/download/dotnet/6.0)
- [Git](https://git-scm.com/)

## Instalação

Clone este repositório em sua máquina local:

``` bash
git clone https://github.com/devdahmer99/dotNetDDDScaffold.git
cd dotNetDDDScaffold
```
## Uso
Para criar um novo projeto usando o CLI do dotNetDDDScaffold, siga os passos abaixo:
``` bash
dotnet build
```
Execute o CLI para criar uma nova solução DDD:
``` bash
dotnet run --project ./src/dotNetDDDScaffold/Program.cs
```
Siga as instruções fornecidas pelo CLI para configurar seu novo projeto.<br/>
Você será solicitado a fornecer:<br/>
 => Nome do projeto<br/>
 => Nome de usuário do banco de dados<br/>
 => Senha do banco de dados<br/>
 => Caminho para o diretório do projeto

 ## Estrutura do Projeto
 O projeto gerado terá a seguinte estrutura:
```
|-- src
    |-- <ProjectName>.API
    |-- <ProjectName>.Aplicacao
        |-- AutoMapper
        |-- Enums
        |-- Reports
        |-- UseCase
    |-- <ProjectName>.Dominio
        |-- Entidades
        |-- Enums
        |-- Extensoes
        |-- Reports
        |-- Repositories
        |-- Seguranca
        |-- Services
    |-- <ProjectName>.Infra
        |-- DataAccess
        |-- Extensoes
        |-- Migrations
        |-- Seguranca
        |-- Services
        |-- Repositories
    |-- <ProjectName>.Comunicacao
        |-- Enums
        |-- Requests
        |-- Responses
    |-- <ProjectName>.Exception
        |-- ExceptionBase
```
## O Que o Projeto Gerado Possui
O projeto gerado pelo dotNetDDDScaffold já virá configurado com os seguintes recursos:<br/>

=> Arquitetura DDD (Domain-Driven Design): Estrutura de pastas organizada seguindo os princípios do DDD.<br/>
=> Entity Framework Core: Configurado para uso com MySQL, incluindo migrações automáticas.<br/>
=> Injeção de Dependência: Configuração de DI (Dependency Injection) para repositórios e serviços.<br/>
=> AutoMapper: Configurado para mapeamento de objetos.<br/>
=> Controllers de API: Exemplo de controlador de API para a entidade Usuario.<br/>
=> Repositórios: Interface e implementação de um repositório para a entidade Usuario.<br/>
=> Casos de Uso: Exemplo de caso de uso para adicionar um usuário.<br/>
=> Serviço de JWT: Serviço de geração de tokens JWT para autenticação.<br/>
=> Arquivos de Configuração: Arquivo appsettings.json configurado para conexão com o banco de dados e chave secreta JWT.<br/>
=> Estrutura de Pastas: Estrutura de pastas organizada para Aplicação, Domínio, Infraestrutura, Comunicação e Exceções.

## Executando o Projeto
Após a criação do projeto, siga os passos abaixo para executar o projeto:
Navegue até o diretório do projeto gerado:
``` bash
cd <caminho/do/projeto>/<ProjectName>
```
Restaure as dependências:
``` bash
dotnet restore
```
Crie as migrações e atualize o banco de dados:
``` bash
dotnet ef migrations add InitialCreate --project ./src/<ProjectName>.Infra --startup-project ./src/<ProjectName>.API
dotnet ef database update --project ./src/<ProjectName>.Infra --startup-project ./src/<ProjectName>.API
```
Execute o projeto:
``` bash
dotnet run --project ./src/<ProjectName>.API
```

## Contribuição
Contribuições são bem-vindas! Por favor, abra um problema ou envie um pull request para melhorias e correções.

## Licença
Este projeto está licenciado sob a Licença MIT. Veja o arquivo LICENSE para mais detalhes.
``` bash
Este `README.md` fornece uma visão geral abrangente do projeto `dotNetDDDScaffold`, incluindo instruções detalhadas sobre como instalar, usar e executar o projeto gerado pelo CLI.
```
