# Locadora de Veículos

API RESTful para gestão de uma locadora de veículos, desenvolvida em ASP.NET Core 8 com
Entity Framework Core e SQL Server Express.

**Trabalho Prático** — Tecnologias para Análise e Desenvolvimento de Sistemas
Marco Túlio

## Vídeo de apresentação

https://youtu.be/-LkULsgQMv0

Demonstração do sistema em funcionamento, 11min29. Índice dos trechos:

| Tempo | Trecho |
|---|---|
| 0:00 | Abertura, entidades, ApplicationContext e o banco no SQL Server |
| 2:05 | Consulta da frota e cadastro de veículo |
| 3:27 | Locação, baixa na frota e devolução com cálculo de valores |
| 5:57 | Consultas com INNER JOIN e LEFT JOIN |
| 8:31 | Validação e tratamento de erros |
| 10:43 | Encerramento |

## Tecnologias

- C# 12 sobre .NET 8
- ASP.NET Core Web API (controllers)
- Entity Framework Core 8 — Code First, com migrations
- SQL Server Express
- Swagger / Swashbuckle

## Estrutura

```
LocadoraVeiculos/
  Models/          5 entidades do domínio
  Data/            ApplicationContext
  Controllers/     6 controllers, 32 endpoints
  Middleware/      tratamento global de exceções
  Migrations/      esquema gerado pelo Entity Framework
  Scripts/         povoamento e consultas de verificação
Docs/
  Relatorio_Academico.pdf            visão geral e link do vídeo
  Documentacao_Endpoints_API.pdf     os 32 endpoints detalhados
  Relatorio_Testes_API.pdf           38 testes com evidências
  testes/                            capturas de tela dos testes
```

## Modelo de dados

| Entidade | Chave primária | Chaves estrangeiras |
|---|---|---|
| Fabricante | Id | — |
| CategoriaVeiculo | Id | — |
| Veiculo | Id | FabricanteId, CategoriaVeiculoId |
| Cliente | Id | — |
| Aluguel | Id | ClienteId, VeiculoId |

## Como executar

1. Restaure os pacotes NuGet.
2. Ajuste a string de conexão em `LocadoraVeiculos/appsettings.json` conforme a sua
   instância do SQL Server Express.
3. No Console do Gerenciador de Pacotes, execute `Update-Database`.
4. Opcional: rode `LocadoraVeiculos/Scripts/01_povoamento.sql` para carregar a massa de
   dados usada na demonstração.
5. Inicie a aplicação. O Swagger abre em `https://localhost:7297/swagger`.

## Etapas do trabalho

| Etapa | Descrição | Situação |
|---|---|---|
| 1 | Modelagem do banco de dados | Entregue |
| 2 | Implementação do backend | Entregue |
| 3 | Testes e documentação | Entregue |
| 4 | Vídeo de apresentação | Entregue |
