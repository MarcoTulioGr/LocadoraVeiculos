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

## Parte 2 - Front-end

### Wireframes
[Ver wireframes](https://claude.ai/artifact/CtE9Hfm8YQ1qvLtKHZv8s5#play-d36327ea7ea5)

### Como rodar

O front-end fica em `frontend/` e usa HTML, Bootstrap 5.3.3 (via CDN) e JavaScript puro, sem
framework e sem etapa de build.

1. Suba a API na porta HTTP:
   ```
   dotnet run --project LocadoraVeiculos --launch-profile http
   ```
   Ela fica disponível em `http://localhost:5207` (o endereço está em `frontend/js/api.js`).
2. Abra a pasta `frontend/` no VS Code e clique em **Go Live** (extensão Live Server), ou
   clique com o botão direito em `index.html` → **Open with Live Server**.
3. O navegador abre em `http://127.0.0.1:5500`. O CORS da API já libera essa origem.

### Telas

- **Início** (`index.html`): cards de resumo (frota, disponíveis, aluguéis em aberto,
  faturamento) e lista de aluguéis em aberto.
- **Fabricantes** (`fabricantes.html`): cadastro, edição e exclusão; filtro por nome e país.
- **Categorias** (`categorias.html`): cadastro, edição e exclusão; filtro por nome e faixa de diária.
- **Veículos** (`veiculos.html`): cadastro, edição e exclusão; filtro por fabricante, categoria,
  situação e ano mínimo.
- **Clientes** (`clientes.html`): cadastro, edição e exclusão; filtro por nome, CPF e
  "somente sem aluguel".
- **Aluguéis** (`alugueis.html`): novo aluguel, edição, exclusão e devolução; filtro por
  cliente, status e período de retirada.
- **Relatórios** (`relatorios.html`): faturamento por categoria, devoluções em atraso,
  veículos nunca alugados e clientes x aluguéis.

### Rotas usadas por tela

| Tela | Rotas |
|---|---|
| Início | `GET /api/veiculos`, `GET /api/filtros/alugueis-em-aberto`, `GET /api/filtros/faturamento-por-categoria` |
| Fabricantes | `GET`, `POST /api/fabricantes`, `PUT`, `DELETE /api/fabricantes/{id}` |
| Categorias | `GET`, `POST /api/categoriasveiculo`, `PUT`, `DELETE /api/categoriasveiculo/{id}` |
| Veículos | `GET /api/filtros/veiculos`, `GET /api/fabricantes`, `GET /api/categoriasveiculo`, `GET`, `PUT`, `DELETE /api/veiculos/{id}`, `POST /api/veiculos` |
| Clientes | `GET /api/clientes`, `GET /api/filtros/clientes-e-alugueis`, `POST /api/clientes`, `PUT`, `DELETE /api/clientes/{id}` |
| Aluguéis | `GET`, `POST /api/alugueis`, `GET`, `PUT`, `DELETE /api/alugueis/{id}`, `POST /api/alugueis/{id}/devolucao`, `GET /api/clientes`, `GET /api/filtros/veiculos?disponivel=true` |
| Relatórios | `GET /api/filtros/faturamento-por-categoria`, `GET /api/filtros/devolucoes-em-atraso`, `GET /api/filtros/veiculos-nunca-alugados`, `GET /api/filtros/clientes-e-alugueis` |
