# Plano de ataque — API de Eventos (.NET 10)

## 1) O que eu extraí dos cards

Li todos os cards do arquivo e agrupei em 4 blocos grandes:

### Bloco A — Entrada no módulo
- **85066** Acesso
- **85067** Visualização da lista
- **85069** Status de eventos

### Bloco B — Detalhe do evento
- **85071** Informações gerais *(já em desenvolvimento)*
- **85072** Cronograma do evento
- **85074** Regulamento e mais informações
- **85075** Acesso à inscrição
- **85079** Detalhes da premiação

### Bloco C — Inscrição de projeto
- **85077** Formulário de inscrição
- **85078** Submissão de arquivos
- **85081** Finalizar inscrição

### Bloco D — Projetos cadastrados
- **85082** Lista de projetos
- **85293** Permissões de edição e exclusão
- **85083** Editar projeto
- **85084** Excluir projeto
- **85085** Inscrever novo projeto

---

## 2) Leitura prática do backlog

### O que parece já estar encaminhado
Pelos próprios cards, estes itens já estão em desenvolvimento:
- **85066** Acesso
- **85067** Visualização da lista
- **85071** Informações gerais

### O que parece ser o coração real do backend
O miolo mais sensível do projeto é este:
1. **status do evento**
2. **inscrição de projeto**
3. **regras de segmento/categoria**
4. **upload e remoção de arquivos**
5. **listagem de projetos**
6. **permissão de editar/excluir**
7. **edição e exclusão lógica**

Em outras palavras:  
**o gargalo não é mostrar evento; o gargalo é controlar inscrição com regra de negócio.**

---

## 3) Ordem mais segura para começar

### Fase 0 — Ler o boilerplate antes de codar
Primeiro, descobrir no projeto atual:
- onde ficam controllers
- onde ficam services
- onde ficam repositories
- como o projeto faz autenticação e usuário logado
- como o projeto faz paginação, response padrão e tratamento de erro
- como o projeto já integra com arquivo/blob/storage

> Regra de ouro: **não inventar arquitetura nova se o boilerplate já tem um jeito pronto.**

### Fase 1 — Fechar o domínio mínimo
Antes de endpoint novo, fechar estas peças:
- `Event`
- `EventEnrollment`
- `EnrollmentFile`
- `EventDocument`
- `Segment`
- `Category`
- `School`
- `User`

Também fechar alguns campos importantes:
- datas do evento
- status calculado do evento
- vínculo do usuário com segmento
- autor do projeto
- flag de exclusão lógica
- tipo do arquivo
- blob name / storage key

### Fase 2 — Entregar leitura antes de escrita
Ordem mais segura:
1. `GET /event/all`
2. regra de status do evento
3. `GET /event/{id}`
4. documentos + cronograma + premiação
5. disponibilidade de segmento/categoria

### Fase 3 — Criar inscrição
Depois partir para:
1. validação do formulário
2. `POST /event-enrollment`
3. upload de arquivos
4. confirmação de arquivo
5. finalize inscrição

### Fase 4 — Pós-inscrição
Depois disso:
1. `GET /event-enrollment/all`
2. flags de permissão por linha
3. `PUT /event-enrollment/{id}`
4. `DELETE /event-enrollment/{id}` com exclusão lógica

---

## 4) Dependências entre os cards

### Dependência macro
- **85066** -> **85067** -> **85069** -> **85071**
- **85071** -> **85075**
- **85075** -> **85077** -> **85078** -> **85081**
- **85071** -> **85082**
- **85082** -> **85293**
- **85293** -> **85083** e **85084**
- **85082** -> **85085** -> **85077**

### Observação importante
Os cards **85072**, **85074** e **85079** parecem ser **subpartes da tela de detalhes do evento**, não necessariamente módulos independentes de backend.  
Eu trataria esses cards como **composição do GET do detalhe do evento**, e não como 3 APIs completamente separadas.

---

## 5) Regras críticas que precisam existir no backend

## Status do evento
Pelos cards, o status vem de data:
- antes do início da inscrição -> **Em breve**
- entre início e fim da inscrição -> **Inscrição aberta**
- depois do fim da inscrição e antes da divulgação -> **Em andamento**
- depois da divulgação -> **Encerrado**
- se a escola já tem projeto inscrito -> etiqueta extra **Inscrição realizada**

### Alerta
Os cards não deixam 100% claro o comportamento exato em **datas iguais** (`==`) nas viradas de status.  
Vale alinhar isso com QA/PO antes de travar a regra final.

## Regra de segmento/categoria
- só mostrar segmento apto
- professor/coordenador só vê segmento com vínculo
- categoria depende do segmento
- não permitir duplicidade por **escola + evento + segmento + categoria**
- ao trocar segmento, desfazer categoria

## Regras de arquivo
- máximo 6 arquivos no total
- máximo 4 imagens
- máximo 1 PDF
- máximo 1 vídeo
- vídeo até 1 minuto
- ordenar por nome
- não aceitar dois arquivos com mesmo nome no mesmo projeto
- nome salvo no formato `idEscola_idProjeto_nomeDoArquivo`

## Permissão
- **Escola**: edita/exclui qualquer projeto da escola
- **Coordenador**: edita/exclui apenas projetos dos segmentos com vínculo
- **Professor**: edita/exclui apenas o projeto do qual é autor
- se o evento não estiver com **Inscrição aberta**, ninguém edita/exclui

## Exclusão
- exclusão deve ser **lógica**
- projeto excluído não aparece na lista
- projeto excluído não entra no cálculo de limite de inscrição

---

## 6) Minha sugestão de recorte técnico

Se eu fosse quebrar em entregas pequenas, faria assim:

### Slice 1 — Leitura básica
- `GET /event/all`
- status calculado
- separação de eventos em andamento/anterior
- `GET /event/{id}`

### Slice 2 — Detalhe do evento
- descrição
- documentos
- cronograma
- premiação
- botão habilitado/desabilitado

### Slice 3 — Formulário
- nome do projeto
- responsável
- representante da gestão
- segmento
- categoria
- regras de disponibilidade

### Slice 4 — Criar inscrição
- `POST /event-enrollment`
- validação obrigatória
- redirecionamento lógico do fluxo

### Slice 5 — Arquivos
- `POST file`
- `POST file/confirm`
- `GET file/{id}`
- `DELETE file/...`
- validações de limite e duplicidade

### Slice 6 — Projetos cadastrados
- `GET /event-enrollment/all`
- contador
- tabela
- action flags

### Slice 7 — Manutenção
- `PUT /event-enrollment/{id}`
- `DELETE /event-enrollment/{id}`
- exclusão lógica
- reaproveitar validação do create

---

## 7) Rotas vistas no board da imagem

Na imagem aparecem, com boa confiança, estas rotas:
- `GET /event/all`
- `GET /event/{id}`
- `GET /event-enrollment/all`
- `POST /event-enrollment`
- `PUT /event-enrollment/{id}`
- `DELETE /event-enrollment/{id}`
- `GET /event-enrollment/segment/...`
- `POST /file`
- `POST /file/confirm`
- `GET /file/{id}`
- `DELETE /file/{context}/{blobName}`
- também aparece algo como `DELETE /file/event-enrollment/{fileId}`

### Alerta
Alguns trechos da imagem estão pequenos.  
Então eu trataria os caminhos acima como **muito prováveis**, mas ainda validaria os sufixos exatos no board original ou no boilerplate.

---

## 8) O melhor caminho para começar amanhã cedo

Se você quiser sair do zero sem travar, eu começaria exatamente assim:

1. abrir o boilerplate
2. achar um módulo já pronto parecido
3. copiar a estrutura do módulo parecido
4. criar primeiro o `GET /event/all`
5. criar um serviço de **status do evento**
6. depois fazer `GET /event/{id}`
7. depois modelar `EventEnrollment`
8. depois atacar o formulário e as regras de segmento/categoria
9. depois arquivos
10. por último edição/exclusão

### Em resumo
**Comece pela leitura e pelo cálculo de status.**  
Depois faça o **miolo da inscrição**.  
Deixe **edição/exclusão** por último.

---

# 9) Prompt base para IA — estilo dev junior

Use este prompt antes dos prompts específicos:

```text
Você vai atuar como um desenvolvedor backend .NET 10 Júnior, mas organizado e cuidadoso.

Regras do seu comportamento:
- Siga o padrão do boilerplate já existente.
- Não invente arquitetura nova sem necessidade.
- Não crie abstrações sofisticadas se o projeto não usar isso hoje.
- Evite CQRS, MediatR, Domain Events, Specification Pattern ou camadas extras, a menos que já existam no projeto.
- Prefira classes simples, services diretos, DTOs explícitos e validações fáceis de entender.
- Faça como um dev júnior bom faria: simples, legível, incremental e sem overengineering.
- Sempre explique em qual pasta cada arquivo deve ser criado.
- Sempre entregue em passos pequenos.
- Antes de escrever código, resuma o plano em 5 passos.
- Depois entregue o código por arquivo.
- Em cada arquivo, explique rapidamente a responsabilidade dele.
- Se existir alguma ambiguidade na regra de negócio, destaque isso antes de codar.
- Reaproveite o máximo possível do que já existir no boilerplate.

Contexto funcional:
Estou implementando um módulo de Eventos com:
- listagem de eventos
- cálculo de status
- detalhe do evento
- inscrição de projeto
- upload de arquivos
- listagem de projetos
- edição e exclusão lógica
- permissões por perfil

Quero uma solução realista, parecida com o que um time aceitaria de um dev júnior.
```

---

# 10) Prompts prontos para usar

## Prompt 1 — Mapear o boilerplate
```text
Use o prompt base.

Agora eu quero que você faça apenas o diagnóstico do boilerplate, sem codar ainda.

Analise a estrutura do projeto e me responda:
1. qual módulo existente mais se parece com o módulo de Eventos
2. qual padrão de organização o boilerplate usa
3. em quais pastas devo colocar controller, service, repository, DTO, entity e validator
4. como o projeto pega o usuário logado
5. como o projeto faz upload de arquivos
6. qual o melhor ponto de entrada para começar Eventos sem quebrar padrão

Entregue:
- um mapa da estrutura atual
- uma sugestão de onde encaixar o módulo Eventos
- uma lista dos arquivos que eu devo criar primeiro
- sem overengineering
- tudo explicado como se eu fosse um dev júnior do time
```

## Prompt 2 — Modelar as entidades mínimas
```text
Use o prompt base.

Quero modelar apenas o domínio mínimo para o módulo Eventos.

Considere estas entidades e relações prováveis:
- Event
- EventEnrollment
- EnrollmentFile
- EventDocument
- Segment
- Category
- School
- User

Considere também estas regras:
- status do evento é calculado por datas
- inscrição é da escola no evento
- projeto tem segmento e categoria
- exclusão de inscrição é lógica
- arquivo fica associado a uma inscrição
- professor pode ser autor
- coordenador tem vínculo com segmentos

Entregue:
1. entidades mínimas
2. campos mínimos por entidade
3. enums necessários
4. relacionamentos
5. o que deve ser persistido e o que deve ser calculado
6. exemplos de classes C# simples
7. sem usar abstrações sofisticadas
8. nomes simples e realistas
```

## Prompt 3 — Implementar status e GET /event/all
```text
Use o prompt base.

Quero implementar a primeira fatia do módulo:
- GET /event/all
- cálculo de status do evento
- etiqueta extra "Inscrição realizada"
- separação entre eventos em andamento e eventos anteriores

Regras dos cards:
- antes da inscrição: Em breve
- durante a inscrição: Inscrição aberta
- depois da inscrição e antes da divulgação: Em andamento
- depois da divulgação: Encerrado
- se a escola tiver projeto inscrito no evento: mostrar etiqueta "Inscrição realizada"

Quero que você:
1. proponha DTO de resposta
2. proponha service simples para cálculo de status
3. mostre controller, service e repository
4. devolva flag suficiente para o frontend separar as seções
5. mantenha o código simples
6. explique cada arquivo criado
7. destaque qualquer ambiguidade de data antes de codar
```

## Prompt 4 — Implementar GET /event/{id}
```text
Use o prompt base.

Agora quero implementar o detalhe do evento.

O GET /event/{id} precisa retornar:
- banner
- título
- descrição
- documentos anexos
- cronograma
- detalhes da premiação
- status do evento
- flag se a aba "Projetos cadastrados" fica habilitada
- flag se o botão "Inscrever novo projeto" fica habilitado

Considere que cronograma e premiação podem ser montados a partir dos próprios campos do evento.

Quero:
1. DTO de detalhe
2. service simples de montagem do detalhe
3. ordenação alfabética dos documentos
4. flags prontas para frontend
5. código por arquivo
6. solução simples e compatível com boilerplate
```

## Prompt 5 — Disponibilidade de segmento e categoria
```text
Use o prompt base.

Agora quero implementar a regra de segmento e categoria para a inscrição.

Regras:
- segmento é obrigatório
- categoria é obrigatória
- professor e coordenador só podem usar segmentos com vínculo
- só mostrar segmentos aptos a receber projeto
- só mostrar categorias aptas dentro do segmento escolhido
- não permitir duplicidade por escola + evento + segmento + categoria
- se trocar o segmento, a categoria deve ser resetada no frontend

Quero que você me ajude a fazer isso pelo backend, de forma simples.

Entregue:
1. sugestão de endpoint para buscar segmentos/categorias aptos
2. service com validação de disponibilidade
3. query/repository necessários
4. DTO de resposta
5. validações reutilizáveis para create e update
6. tudo simples, sem patterns desnecessários
```

## Prompt 6 — Criar inscrição
```text
Use o prompt base.

Agora quero implementar o POST /event-enrollment.

Campos relevantes:
- nome do projeto
- responsável
- representante da gestão
- segmento
- categoria
- arquivos
- usuário logado
- escola do usuário

Regras:
- campos obrigatórios
- não permitir duplicidade de segmento/categoria dentro do evento para a mesma escola
- respeitar permissões do usuário
- só permitir criar inscrição se o evento estiver com status "Inscrição aberta"

Entregue:
1. request DTO
2. response DTO
3. controller
4. service
5. validações
6. persistência
7. mensagens de erro simples e legíveis
8. explicação por arquivo
```

## Prompt 7 — Upload de arquivos
```text
Use o prompt base.

Agora quero implementar o fluxo de arquivos da inscrição.

Rotas prováveis vistas no board:
- POST /file
- POST /file/confirm
- GET /file/{id}
- DELETE /file/{context}/{blobName}
- pode existir também DELETE /file/event-enrollment/{fileId}

Regras:
- máximo 6 arquivos
- máximo 4 imagens
- máximo 1 PDF
- máximo 1 vídeo
- vídeo com no máximo 1 minuto
- ordenar arquivos por nome
- impedir dois arquivos com mesmo nome no mesmo projeto
- salvar nome como idEscola_idProjeto_nomeDoArquivo

Quero:
1. desenho simples do fluxo de upload
2. quais validações ficam antes do storage e quais ficam depois
3. entidades/DTOs mínimas
4. endpoints e services
5. observações sobre o que validar no backend mesmo que o frontend já valide
6. tudo pensado como um dev júnior faria
```

## Prompt 8 — Listar projetos cadastrados com permissões
```text
Use o prompt base.

Agora quero implementar o GET /event-enrollment/all para a aba de projetos cadastrados.

A resposta precisa permitir montar:
- contador de projetos
- tabela com ID, nome do projeto, segmento, categoria, data de cadastro
- ações editar/excluir por linha

Regras de permissão:
- Escola edita/exclui qualquer projeto
- Coordenador edita/exclui apenas projetos dos segmentos com vínculo
- Professor edita/exclui apenas projetos dos quais é autor
- se o evento não estiver com status "Inscrição aberta", ninguém pode editar/excluir

Entregue:
1. DTO de listagem
2. flags IsEditable e IsDeletable
3. service para regra de permissão
4. query/repository
5. código simples e direto
6. explicação por arquivo
```

## Prompt 9 — Editar inscrição
```text
Use o prompt base.

Agora quero implementar o PUT /event-enrollment/{id}.

Regras:
- tela de edição reaproveita a estrutura do cadastro
- salvar alterações só quando houver mudança
- se houver campo obrigatório inválido, não salvar
- respeitar permissões de edição
- reaproveitar o máximo possível das validações do create
- se o evento não estiver com status "Inscrição aberta", bloquear edição

Quero:
1. o melhor jeito simples de reaproveitar create e update
2. request DTO
3. service
4. validações
5. comparação simples para detectar alteração
6. código por arquivo
7. sem sofisticação desnecessária
```

## Prompt 10 — Excluir inscrição com deleção lógica
```text
Use o prompt base.

Agora quero implementar o DELETE /event-enrollment/{id}.

Regras:
- exclusão é lógica
- projeto excluído não aparece na listagem
- projeto excluído não entra na regra de bloqueio de nova inscrição
- respeitar permissões
- se evento não estiver com status "Inscrição aberta", bloquear exclusão

Entregue:
1. a estratégia mais simples de deleção lógica
2. quais campos adicionar na entidade
3. service e repository
4. como filtrar excluídos na listagem
5. como ignorar excluídos na validação de duplicidade
6. código por arquivo
```

## Prompt 11 — Checklist final de QA backend
```text
Use o prompt base.

Eu já implementei o módulo. Agora quero um checklist técnico de QA backend.

Monte um checklist baseado nestes grupos:
- status do evento
- detalhe do evento
- formulário de inscrição
- segmento/categoria
- arquivos
- criação de inscrição
- listagem de projetos
- permissões
- edição
- exclusão lógica

Para cada item, me diga:
1. o que testar
2. o cenário feliz
3. o cenário de erro
4. o risco de regressão
5. quais cenários são mais críticos

Quero esse checklist com mentalidade de time real, mas escrito de forma simples.
```

---

# 11) Decisões que eu deixaria anotadas antes de começar

Antes da primeira linha de código, eu deixaria estas dúvidas registradas:

1. datas de virada do status usam `<`/`>` ou `<=`/`>=`?
2. "Inscrição realizada" é status secundário ou label adicional?
3. o endpoint de segmento retorna só segmentos ou segmento + categorias?
4. o upload é síncrono, assíncrono ou em duas etapas por causa do storage?
5. vídeo com duração máxima de 1 minuto será validado no backend usando metadado de mídia ou apenas no frontend?
6. existe tabela pronta para vínculo de coordenador/professor com segmento?
7. arquivos de evento e arquivos de inscrição usam a mesma estrutura de storage?
8. o contador da aba de projetos mostra apenas ativos ou inclui excluídos logicamente?  
   Minha recomendação: **mostrar apenas ativos**.

---

# 12) Conclusão objetiva

Se eu tivesse que resumir em uma frase:

**Seu melhor começo é construir primeiro a leitura dos eventos e o motor de status, depois o núcleo da inscrição, e só então edição/exclusão.**

A sequência mais segura é:
1. status
2. detalhe
3. segmento/categoria
4. create enrollment
5. upload
6. listagem
7. permission flags
8. update
9. delete lógico
10. testes
