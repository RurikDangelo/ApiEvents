# Guia do Desenvolvedor — API de Eventos Poliedro

> Este documento foi escrito para ser lido por qualquer pessoa da equipe,  
> mesmo quem está começando. Evitamos jargão técnico sempre que possível.

---

## Sumário

1. [O Que É Este Projeto?](#1-o-que-é-este-projeto)
2. [Por Que Uma API?](#2-por-que-uma-api)
3. [Os Cards — O Que Precisamos Entregar](#3-os-cards--o-que-precisamos-entregar)
4. [Como Pensamos o Sistema](#4-como-pensamos-o-sistema)
5. [A Estrutura de Pastas](#5-a-estrutura-de-pastas)
6. [O Banco de Dados com Docker](#6-o-banco-de-dados-com-docker)
7. [O Swagger — A Interface de Testes](#7-o-swagger--a-interface-de-testes)
8. [O Que Foi Feito Até Agora](#8-o-que-foi-feito-até-agora)
9. [Problemas Que Enfrentamos](#9-problemas-que-enfrentamos)
10. [O Que Falta Fazer](#10-o-que-falta-fazer)
11. [Como Estudar o Projeto](#11-como-estudar-o-projeto)
12. [Como Ver e Testar o Banco de Dados](#12-como-ver-e-testar-o-banco-de-dados)
13. [Checklist do Ambiente Local](#13-checklist-do-ambiente-local)

---

## 1. O Que É Este Projeto?

Este é o **backend** (o "servidor") do módulo de **Eventos da Poliedro**.

Imagine o seguinte cenário:
- A Poliedro organiza eventos científicos e de inovação (como feiras e olimpíadas)
- As escolas parceiras precisam **se inscrever** nesses eventos com projetos de alunos
- Os professores e coordenadores precisam **enviar arquivos**, **acompanhar o status** da inscrição e **gerenciar projetos**

Essa API é o "cérebro" por trás de tudo isso. O aplicativo (frontend) faz perguntas para ela — como "quais eventos estão abertos?" — e ela responde com os dados corretos.

---

## 2. Por Que Uma API?

Uma **API** (interface de programação) é como um garçom em um restaurante:
- Você (o app/frontend) faz o pedido
- O garçom (a API) busca a informação no banco de dados (a cozinha)
- Você recebe o resultado

Sem a API, o aplicativo precisaria falar diretamente com o banco de dados, o que seria inseguro e difícil de manter. A API centraliza as regras de negócio — quem pode ver o quê, quais validações existem, etc.

---

## 3. Os Cards — O Que Precisamos Entregar

Aqui está cada card do projeto, o que ele significa na prática e o estado atual:

---

### ✅ Card 85067 — Lista de Eventos

**O que o usuário vê:** A tela com todos os eventos disponíveis — passados e futuros.

**O que a API faz:**
- Retorna todos os eventos
- Calcula automaticamente o STATUS de cada evento baseado nas datas (sem precisar que alguém atualize manualmente)
- Indica se a escola do usuário logado **já tem uma inscrição** naquele evento

**Status atual:** ✅ Implementado e funcionando

**Endpoint:** `GET /api/v1/event/all`

---

### ✅ Card 85069 — Status do Evento

**O que o usuário vê:** Um "badge" ou etiqueta indicando se o evento está "Em breve", "Inscrições abertas", "Em andamento" ou "Encerrado".

**Como calculamos o status:**

| Situação | Status exibido |
|---|---|
| Data de início das inscrições ainda não chegou | **Em breve** |
| Estamos dentro do período de inscrições | **Inscrição aberta** |
| Inscrições encerradas, resultado não divulgado | **Em andamento** |
| Data de divulgação já passou | **Encerrado** |

> Importante: o status **não fica salvo no banco**. Ele é calculado na hora em que alguém faz a consulta. Assim, nunca fica desatualizado.

**Status atual:** ✅ Implementado

---

### ✅ Card 85071 — Informações Gerais do Evento

**O que o usuário vê:** A página de detalhe de um evento — título, descrição, banner, datas.

**O que a API faz:** Retorna todas as informações de um evento específico.

**Status atual:** ✅ Implementado

**Endpoint:** `GET /api/v1/event/{id}` (onde `{id}` é o código único do evento)

---

### ✅ Card 85072 — Cronograma do Evento

**O que o usuário vê:** Uma linha do tempo dentro da tela do evento — "Início das inscrições em tal data", "Resultado em tal data", etc.

**O que a API faz:** Monta o cronograma automaticamente a partir das datas já cadastradas no evento. Nenhum campo extra é necessário.

**Status atual:** ✅ Implementado (junto com o card 85071)

---

### ✅ Card 85074 — Regulamento e Documentos

**O que o usuário vê:** Links para baixar o regulamento, edital e outros documentos do evento.

**O que a API faz:** Retorna a lista de documentos anexados ao evento.

**Status atual:** ✅ Implementado (junto com o card 85071)

---

### ✅ Card 85079 — Detalhes da Premiação

**O que o usuário vê:** Uma seção descrevendo os prêmios do evento.

**O que a API faz:** Retorna o campo `AwardDetails` junto com o detalhe do evento.

**Status atual:** ✅ Implementado

---

### ⏳ Card 85075 — Botão de Inscrição (Habilitado ou Não)

**O que o usuário vê:** O botão "Inscrever-se" habilitado ou desabilitado.

**Como a API já ajuda:** O endpoint de detalhe do evento já retorna os campos:
- `canEnroll: true` → botão habilitado (inscrições abertas)
- `canEnroll: false` → botão desabilitado
- `canViewProjects: true/false` → controla se a escola vê os projetos inscritos

**Status atual:** ⏳ A lógica de resposta está pronta, falta o endpoint de inscrição propriamente dito (card 85077)

---

### ⏳ Card 85077 — Formulário de Inscrição

**O que o usuário vê:** O formulário para cadastrar um novo projeto no evento.

**O que a API precisará fazer:**
- Receber o nome do projeto, responsável, segmento, categoria
- Validar que a escola ainda não tem projeto naquele segmento+categoria
- Salvar a inscrição

**Status atual:** ⏳ Pendente (próximo passo)

**Endpoint futuro:** `POST /api/v1/event-enrollment`

---

### ⏳ Card 85078 — Envio de Arquivos

**O que o usuário vê:** O upload de fotos, PDF e vídeo do projeto.

**Regras que a API precisará validar:**
- Máximo de **6 arquivos** por projeto
- Máximo de **4 imagens**
- Máximo de **1 PDF**
- Máximo de **1 vídeo** (duração máxima: 1 minuto)
- O nome do arquivo deve ser único por projeto

**Status atual:** ⏳ Pendente

**Endpoint futuro:** `POST /api/v1/file`

---

### ⏳ Card 85082 — Lista de Projetos Cadastrados

**O que o usuário vê:** A lista de todos os projetos inscritos no evento por aquela escola.

**O que a API precisará fazer:**
- Retornar os projetos da escola
- Indicar se o usuário logado pode **editar** ou **excluir** cada projeto (baseado no perfil)

| Perfil | O que pode fazer |
|---|---|
| **Escola** | Edita e exclui qualquer projeto da escola |
| **Coordenador** | Edita e exclui apenas projetos dos seus segmentos |
| **Professor** | Edita e exclui apenas o projeto do qual é autor |

**Status atual:** ⏳ Pendente

**Endpoint futuro:** `GET /api/v1/event-enrollment/all`

---

### ⏳ Card 85293 — Permissões de Edição e Exclusão

**O que o usuário vê:** O botão de editar/excluir aparece apenas para quem tem permissão.

**Status atual:** ⏳ Será implementado junto com o card 85082

---

### ⏳ Card 85083 — Editar Projeto

**Status atual:** ⏳ Pendente

**Endpoint futuro:** `PUT /api/v1/event-enrollment/{id}`

---

### ⏳ Card 85084 — Excluir Projeto

**O que acontece nos bastidores:** O projeto **não é deletado do banco**. Ele recebe uma "data de exclusão", e o sistema passa a ignorá-lo nas consultas. Isso se chama **exclusão lógica** — o dado fica preservado para auditoria.

**Status atual:** ⏳ Pendente

**Endpoint futuro:** `DELETE /api/v1/event-enrollment/{id}`

---

## 4. Como Pensamos o Sistema

### Ponto de Partida: o Boilerplate

O projeto não começou do zero. Existe uma estrutura base (chamada **boilerplate**) que já vinha com:
- A organização de pastas
- A configuração do Swagger
- A configuração de autenticação (JWT/Keycloak)
- Um exemplo de entidade e controller

A primeira coisa que fizemos foi **entender o que o boilerplate oferecia** e planejar o que precisava ser adicionado.

### Mapeando as Entidades

Uma **entidade** é como uma tabela no banco de dados. Antes de escrever qualquer código de endpoint, precisamos modelar o que existe no nosso sistema:

- **Event** → Um evento da Poliedro (FEIC, Olimpíada, etc.)
- **EventDocument** → Um PDF ou documento anexado ao evento
- **EventEnrollment** → A inscrição de uma escola num evento
- **EnrollmentFile** → Um arquivo enviado pela escola (foto, PDF do projeto)
- **Segment** → O nível/segmento (ex: Ensino Fundamental, Médio)
- **Category** → A categoria dentro do segmento (ex: Ciências, Tecnologia)
- **UserSegment** → O vínculo entre um coordenador/professor e os segmentos que ele gerencia

### A Regra de Ouro: BaseEntity

Todas as entidades herdam de `BaseEntity`, que garante que **todo registro** tem:
- `Id` → identificador único (um UUID, código longo e único)
- `CreateDate` → quando foi criado
- `UpdateDate` → quando foi atualizado pela última vez
- `DeleteDate` → quando foi "excluído" (exclusão lógica — o registro fica no banco)

### O Status do Evento

Esse foi um ponto de design importante. O status poderia ser um campo no banco (`status: "aberto"`), mas isso teria um problema: ninguém lembraria de atualizar quando a data virasse.

A solução: **calcular o status na hora da consulta**, baseado nas datas. O código compara as datas do evento com a data atual e decide qual status mostrar. Isso garante que o sistema sempre mostra a informação correta, automaticamente.

---

## 5. A Estrutura de Pastas

```
src/
├── 01-Presentation        ← Ponto de entrada da API (Controllers)
│   Controllers/
│   └── EventController.cs    → Define os endpoints de eventos
│   Startup.cs                → Configura autenticação, Swagger, middleware
│   Program.cs                → Liga o servidor
│
├── 02-Application         ← Regras de negócio (o "cérebro")
│   Dtos/                     → Formatos de entrada e saída (o que a API recebe/retorna)
│   Interfaces/               → Contratos (define "o que" fazer, sem "como")
│   Services/                 → Implementa as regras (calcula status, valida dados, etc.)
│
├── 03-Domain              ← As entidades (o "vocabulário" do sistema)
│   Entities/                 → Event, EventEnrollment, Segment, etc.
│   Enums/                    → EventStatus, FileType, UserRole
│
└── 04-Infra/
    ├── 4.1-CrossCutting/
    │   ControllerHandler/    → Captura erros e retorna mensagens amigáveis
    │   IoC/DI.cs             → Registro de todas as dependências
    └── 4.2-Data/
        ProjectContext.cs     → Conexão com o banco de dados
        Mappers/              → Define quais campos vão para quais tabelas
        Repositories/         → Busca dados do banco
```

### Como Ler o Fluxo

Quando alguém faz uma requisição `GET /api/v1/event/all`:

```
1. Chega no EventController
2. O controller chama o EventService
3. O EventService chama o EventRepository
4. O EventRepository busca no banco (PostgreSQL)
5. Os dados voltam para o EventService
6. O EventService calcula o status de cada evento
7. O resultado vai para o controller
8. O controller envia a resposta para quem pediu
```

Cada camada tem uma responsabilidade clara. O controller não sabe de banco de dados. O repositório não sabe das regras de negócio. Isso facilita manutenção e testes.

---

## 6. O Banco de Dados com Docker

### O Que É Docker?

Docker é um programa que cria "caixas isoladas" (containers) no seu computador. Cada caixa roda um programa específico sem interferir no resto do sistema.

No nosso caso, usamos Docker para rodar o **PostgreSQL** (o banco de dados) sem precisar instalá-lo no Windows.

### Como Subimos o Banco

O arquivo `docker-compose.yml` na raiz do projeto define todas as configurações do banco:
- Qual versão do PostgreSQL usar (16)
- O nome do banco (`apieventsr_dev`)
- Usuário e senha (`postgres` / `postgres`)
- A porta que será usada (`5432`)

### Comandos do Dia a Dia

Todos os comandos abaixo devem ser rodados na pasta `apieventsr/apieventsr/` (onde está o `docker-compose.yml`).

```powershell
# Ligar o banco
docker compose up -d

# Verificar se o banco está rodando
docker ps

# Parar o banco (sem perder dados)
docker compose stop

# Ligar novamente
docker compose start

# Parar e remover o container (os dados ficam no volume)
docker compose down

# PERIGO: parar, remover e apagar todos os dados
docker compose down -v
```

> **Dica:** O `-d` no `docker compose up -d` significa "detached" — roda em segundo plano. Sem ele, o terminal ficaria travado mostrando logs.

### O Que Aconteceu Quando Rodamos as Migrations?

**Migrations** são como "scripts de criação das tabelas", mas gerenciados automaticamente pelo Entity Framework (nosso ORM).

Quando rodamos:
```powershell
dotnet ef migrations add InitEventModule ...
dotnet ef database update ...
```

O Entity Framework:
1. Leu todas as entidades do código
2. Gerou um arquivo com os comandos SQL para criar as tabelas
3. Conectou no banco (via Docker) e executou esses comandos

Depois disso, as tabelas `events`, `segments`, `event_enrollments`, etc. foram criadas automaticamente.

---

## 7. O Swagger — A Interface de Testes

### O Que É o Swagger?

O Swagger é uma página web que a própria API gera automaticamente, mostrando todos os endpoints disponíveis. É como um "manual interativo" onde você pode testar as requisições sem precisar de ferramentas externas.

### Como Acessar

1. Rode a API: `dotnet run --project src\01-Presentation` (na pasta `apieventsr/apieventsr/`)
2. Abra no navegador: `https://localhost:7164/swagger`

### Como Usar

1. Clique em um endpoint (ex: `GET /api/v1/event/all`)
2. Clique em **Try it out** (botão à direita)
3. Clique em **Execute**
4. O Swagger mostra a resposta abaixo

> **Autenticação:** Alguns endpoints exigem login. Clique no botão **Authorize** no topo da página, cole seu Bearer token do Keycloak e clique em **Authorize**.

### O Que Está Disponível Hoje no Swagger

| Endpoint | O que faz |
|---|---|
| `GET /api/v1/event/all` | Lista todos os eventos com status calculado |
| `GET /api/v1/event/{id}` | Detalhe de um evento específico |
| `GET /api/v1/entity/all` | Exemplo do boilerplate (não usar) |

---

## 8. O Que Foi Feito Até Agora

### Semana 1 — Entendimento e Base

**Passo 0 — Diagnóstico do Boilerplate**
- Lemos todo o código base existente
- Entendemos como o projeto estava organizado
- Planejamos o que precisaria ser adicionado

**Passo 1 — Modelagem das Entidades**
- Criamos todas as "tabelas" do sistema no código C#
- `Event`, `EventDocument`, `EventEnrollment`, `EnrollmentFile`, `Segment`, `Category`, `UserSegment`
- Criamos os enums: `EventStatus`, `FileType`, `UserRole`
- Configuramos como cada entidade vira uma tabela no banco (os "Mappers")

**Passo 2 — GET /event/all**
- Criamos o endpoint que lista todos os eventos
- Implementamos a lógica de cálculo de status
- A resposta já indica se a escola do usuário tem inscrição

**Passo 3 — GET /event/{id}**
- Criamos o endpoint de detalhe do evento
- A resposta inclui documentos, cronograma e premiação
- Criamos os "DTOs" (formatos de resposta) específicos para cada tela

### Semana 2 — Infraestrutura e Banco

**Docker**
- Criamos o `docker-compose.yml` para rodar o PostgreSQL localmente
- Subimos o banco com as tabelas corretas via migrations

**Segurança**
- Protegemos o arquivo com a senha do banco (`.gitignore`)
- Criamos um template de configuração para novos devs

---

## 9. Problemas Que Enfrentamos

Aqui estão os problemas que aparecem com a solução de cada um. Isso é útil para não travar nos mesmos pontos.

---

### Problema 1 — `docker compose` dava erro "not found"

**O que aconteceu:** O comando `docker compose up -d` foi rodado na pasta errada.

**Por quê:** O arquivo `docker-compose.yml` fica dentro de `apieventsr/apieventsr/`, mas o terminal estava em `apieventsr/` (um nível acima).

**Solução:** Sempre rodar os comandos Docker dentro da pasta `apieventsr/apieventsr/`.

---

### Problema 2 — `dotnet ef` não encontrado

**O que aconteceu:** O comando de migrations não funcionou porque a ferramenta `dotnet-ef` não estava instalada.

**Por quê:** O EF Core Tools é uma ferramenta separada do SDK do .NET. Precisa ser instalada uma vez por computador.

**Solução:**
```powershell
dotnet tool install --global dotnet-ef
```

---

### Problema 3 — Startup não referenciava EFCore.Design

**O que aconteceu:** Mesmo com o `dotnet-ef` instalado, a migration deu erro dizendo que o projeto de startup não tinha o pacote de design.

**Por quê:** O pacote `Microsoft.EntityFrameworkCore.Design` estava só no projeto `4.2-Data`, mas precisa estar também no projeto de startup (`01-Presentation`) para as ferramentas funcionarem.

**Solução:** Adicionamos o pacote ao `.csproj` do projeto de Presentation.

---

### Problema 4 — DomainEntity sem construtor padrão

**O que aconteceu:** Ao criar a migration, o EF Core 9 (versão mais nova e mais rigorosa) não conseguia criar uma instância da entidade `DomainEntity` do boilerplate.

**Por quê:** A entidade tinha um construtor `(int property)` mas o nome do parâmetro não batia com o nome da propriedade. O EF Core 9 é mais estrito sobre isso que versões anteriores.

**Solução:** Adicionamos um construtor sem parâmetros `protected DomainEntity() {}` que o EF Core usa para instanciar a entidade internamente.

---

### Problema 5 — `delete_date` não existe no banco

**O que aconteceu:** Ao aplicar a migration, deu erro que a coluna `delete_date` não existia.

**Por quê:** Os índices únicos com filtro usavam o nome em snake_case (`delete_date`), mas o PostgreSQL criou a coluna em PascalCase (`"DeleteDate"`) com aspas, porque não configuramos snake_case naming.

**Solução:** Corrigimos o filtro nos Mappers para usar o nome correto:
```csharp
// Errado
.HasFilter("delete_date IS NULL")

// Correto
.HasFilter("\"DeleteDate\" IS NULL")
```

---

### Problema 6 — Certificado HTTPS faltando

**O que aconteceu:** Ao rodar a API pela primeira vez, ela travou com erro de HTTPS.

**Por quê:** Para desenvolvimento local com HTTPS, o .NET precisa de um certificado autoassinado que precisa ser criado e confiável no Windows.

**Solução:**
```powershell
dotnet dev-certs https --trust
```

---

### Problema 7 — Versões de pacotes incompatíveis (.NET 9)

**O que aconteceu:** Tentamos migrar para .NET 10, mas o SDK instalado era 9. Ao reverter, os pacotes `9.0.0` conflitavam com o Npgsql que exigia `9.0.1+`.

**Solução:** Atualizamos todos os pacotes EF Core e Extensions para a versão `9.0.4`, alinhada com o Npgsql.

---

## 10. O Que Falta Fazer

### Próximo Passo: Segmentos e Categorias (Pré-requisito da Inscrição)

Antes de criar o formulário de inscrição, precisamos de um endpoint que diga quais segmentos e categorias estão disponíveis para aquele evento — e filtrados pelo perfil do usuário logado.

**Endpoint planejado:** `GET /api/v1/event-enrollment/segment/{eventId}`

---

### Roteiro Completo dos Próximos Passos

| # | O Que Fazer | Endpoint | Card |
|---|---|---|---|
| 4 | Listar segmentos/categorias disponíveis | `GET /event-enrollment/segment/{eventId}` | 85075 |
| 5 | Criar inscrição | `POST /event-enrollment` | 85077 |
| 6 | Enviar arquivos | `POST /file` + `POST /file/confirm` | 85078 |
| 7 | Listar projetos inscritos | `GET /event-enrollment/all` | 85082, 85293 |
| 8 | Editar projeto | `PUT /event-enrollment/{id}` | 85083 |
| 9 | Excluir projeto | `DELETE /event-enrollment/{id}` | 85084 |
| 10 | Testes e revisão | — | — |

---

## 11. Como Estudar o Projeto

### Por Onde Começar?

**1. Leia as Entidades primeiro**
Pasta: `src/03-Domain/Entities/`

As entidades são a "linguagem" do sistema. Se você entende o que é um `Event`, um `EventEnrollment` e um `EnrollmentFile`, você entende o que o sistema faz.

**2. Veja como um endpoint funciona de ponta a ponta**
Siga o fluxo do `GET /event/all`:
- `src/01-Presentation/Controllers/EventController.cs` → como o endpoint é declarado
- `src/02-Application/Services/EventService.cs` → onde o status é calculado
- `src/04-Infra/4.2-Data/Repositories/EventRepository.cs` → como o banco é consultado

**3. Entenda os DTOs**
Pasta: `src/02-Application/Dtos/Responses/`

Um DTO é o "formato" da resposta da API. O `EventListItemResponse.cs` é exatamente o que o Swagger mostra quando você chama `GET /event/all`.

**4. Explore o banco de dados** (ver seção 12)

### Conceitos Para Estudar

| Conceito | O Que É | Onde Ver no Código |
|---|---|---|
| **Entity Framework** | Ferramenta que conecta o código C# ao banco de dados | `ProjectContext.cs`, pasta `Mappers/` |
| **DTO** | Formato de dados enviado/recebido pela API | Pasta `Dtos/` |
| **Repository** | Código que faz as queries no banco | Pasta `Repositories/` |
| **Service** | Código com as regras de negócio | Pasta `Services/` |
| **Migration** | Script de criação/atualização das tabelas | Pasta `Migrations/` (gerada automaticamente) |
| **Swagger** | Interface web de testes dos endpoints | `https://localhost:7164/swagger` |
| **JWT** | Token de autenticação — identifica quem está logado | Configurado em `Startup.cs` |
| **Exclusão Lógica** | Não apaga do banco, só marca com data de exclusão | Campo `DeleteDate` em toda entidade |

---

## 12. Como Ver e Testar o Banco de Dados

### Opção A — Pelo Terminal (psql dentro do Docker)

Com o banco rodando (`docker compose up -d`), execute:

```powershell
docker exec -it apieventsr-db psql -U postgres -d apieventsr_dev
```

Você entrará no console do PostgreSQL. Comandos úteis:

```sql
-- Ver todas as tabelas
\dt

-- Ver estrutura de uma tabela
\d events

-- Ver todos os eventos cadastrados
SELECT * FROM events;

-- Ver inscrições
SELECT * FROM event_enrollments;

-- Sair
\q
```

---

### Opção B — DBeaver (recomendado — interface gráfica)

O DBeaver é uma ferramenta gratuita para visualizar bancos de dados com uma interface amigável.

**Download:** https://dbeaver.io/download/

**Configuração da conexão:**
- Tipo: `PostgreSQL`
- Host: `localhost`
- Porta: `5432`
- Banco: `apieventsr_dev`
- Usuário: `postgres`
- Senha: `postgres`

Com o DBeaver você pode:
- Ver as tabelas em formato de planilha
- Escrever e executar queries SQL
- Ver os dados inseridos após testar no Swagger
- Ver a estrutura de cada tabela

---

### Opção C — Extensão do VS Code

Se você usa Visual Studio Code, instale a extensão **"PostgreSQL" by Chris Kolkman**.

Configuração igual à do DBeaver acima.

---

### Fluxo de Teste Completo

A sequência ideal para testar é:

```
1. Ligar o banco:
   docker compose up -d

2. Rodar a API:
   dotnet run --project src\01-Presentation

3. Abrir o Swagger:
   https://localhost:7164/swagger

4. Inserir dados de teste via Swagger
   (quando os endpoints de POST estiverem prontos)

5. Verificar no banco se os dados foram salvos corretamente:
   docker exec -it apieventsr-db psql -U postgres -d apieventsr_dev
   SELECT * FROM event_enrollments;
```

---

### Como Inserir Dados de Teste Manualmente (SQL)

Para testar os endpoints de leitura sem precisar do frontend, você pode inserir dados diretamente no banco:

```sql
-- Conecte ao banco via terminal ou DBeaver, então execute:

-- Criar um segmento
INSERT INTO segments ("Id", "Name", "IsActive", "CreateDate")
VALUES (gen_random_uuid(), 'Ensino Médio', true, now());

-- Criar um evento
INSERT INTO events (
  "Id", "Title", "Description",
  "EnrollmentStartDate", "EnrollmentEndDate", "ResultDate",
  "CreateDate"
) VALUES (
  gen_random_uuid(),
  'FEIC 2025',
  'Feira de Empreendedorismo e Inovação Científica',
  '2025-03-01 00:00:00+00',
  '2025-03-31 00:00:00+00',
  '2025-04-15 00:00:00+00',
  now()
);
```

Depois de inserir, chame `GET /api/v1/event/all` no Swagger e veja o evento aparecer com o status calculado!

---

## 13. Checklist do Ambiente Local

Use este checklist toda vez que for começar a trabalhar no projeto:

```
[ ] Docker Desktop está aberto e com o ícone verde

[ ] Banco está rodando:
    docker compose up -d
    docker ps  ← deve aparecer "apieventsr-db" com status "Up"

[ ] O arquivo appsettings.Development.json existe em:
    src\01-Presentation\appsettings.Development.json
    (se não existir: copy src\01-Presentation\appsettings.Development.example.json
                           src\01-Presentation\appsettings.Development.json)

[ ] API rodando:
    dotnet run --project src\01-Presentation

[ ] Swagger acessível:
    https://localhost:7164/swagger
```

---

## Glossário Rápido

| Termo | Significado |
|---|---|
| **API** | Servidor que recebe perguntas e retorna dados |
| **Endpoint** | Um "caminho" da API (ex: `/api/v1/event/all`) |
| **Controller** | O código que define os endpoints |
| **Service** | O código com as regras de negócio |
| **Repository** | O código que faz queries no banco |
| **Entity / Entidade** | Representa uma tabela do banco em código C# |
| **DTO** | Formato de dados que a API envia/recebe |
| **Migration** | Script automático de criação/alteração de tabelas |
| **Swagger** | Interface web para testar a API |
| **Docker** | Roda o banco de dados em uma "caixa isolada" |
| **Container** | A "caixa isolada" do Docker |
| **JWT** | Token de autenticação (vem do Keycloak) |
| **Exclusão Lógica** | Marcar como excluído sem apagar do banco |
| **UUID** | Código único e longo usado como ID dos registros |
| **Boilerplate** | Código base de partida do projeto |

---

*Última atualização: Abril 2026*
