# Locadora de Veículos

Sistema de gestão de uma locadora de veículos. Tem duas partes: uma API RESTful em
ASP.NET Core 8 com Entity Framework Core e SQL Server Express, e um front-end em HTML,
Bootstrap e JavaScript que consome essa API.

**Trabalho Prático** — Tecnologias para Análise e Desenvolvimento de Sistemas
Marco Túlio

## Vídeo de apresentação

https://youtu.be/1Zn8gMjif6I

## Tecnologias

**Back-end**

- C# 12 sobre .NET 8
- ASP.NET Core Web API (controllers)
- Entity Framework Core 8.0.10, Code First com migrations
- SQL Server Express
- Swagger (Swashbuckle 6.6.2)

**Front-end**

- HTML5 e CSS3
- Bootstrap 5.3.3 (via CDN)
- JavaScript puro (Fetch API), sem framework e sem etapa de build

## Estrutura

```
LocadoraVeiculos.sln
LocadoraVeiculos/                    API (back-end)
  Controllers/                       6 controllers, 32 endpoints
    FabricantesController.cs           CRUD de fabricantes
    CategoriasVeiculoController.cs     CRUD de categorias
    VeiculosController.cs              CRUD de veículos
    ClientesController.cs              CRUD de clientes
    AlugueisController.cs              CRUD de aluguéis e devolução
    FiltrosController.cs               consultas e relatórios
  Models/                            5 entidades do domínio + DevolucaoRequest
  Data/                              ApplicationContext (DbContext)
  Middleware/                        tratamento global de exceções
  Migrations/                        esquema gerado pelo Entity Framework
  Scripts/
    01_povoamento.sql                  massa de dados da demonstração
    02_consultas.sql                   consultas de verificação
  Program.cs                         serviços, CORS e Swagger
  appsettings.json                   string de conexão
frontend/                            interface web (front-end)
  index.html, fabricantes.html, categorias.html, veiculos.html,
  clientes.html, alugueis.html, relatorios.html
  css/style.css                      estilos próprios
  js/api.js                          endereço da API e funções de requisição
  js/layout.js                       menu lateral comum às telas
  js/<tela>.js                       lógica de cada tela
Docs/
  Relatorio_Academico.pdf            Parte 1: visão geral e link do vídeo
  Documentacao_Endpoints_API.pdf     Parte 1: os 32 endpoints detalhados
  Relatorio_Testes_API.pdf           Parte 1: 38 testes com evidências
  testes/                            capturas de tela dos testes
  parte2/
    Relatorio_Parte2.pdf             Parte 2: relatório do front-end
    latex/                           fonte LaTeX do relatório (modelo SBC)
*.png                                wireframes das telas (raiz do repositório)
```

## Modelo de dados

| Entidade | Chave primária | Chaves estrangeiras |
|---|---|---|
| Fabricante | Id | — |
| CategoriaVeiculo | Id | — |
| Veiculo | Id | FabricanteId, CategoriaVeiculoId |
| Cliente | Id | — |
| Aluguel | Id | ClienteId, VeiculoId |

## Parte 1 - Back-end

### Como executar

1. Restaure os pacotes NuGet.
2. Ajuste a string de conexão em `LocadoraVeiculos/appsettings.json` para a sua
   instância do SQL Server Express.
3. No Console do Gerenciador de Pacotes, execute `Update-Database`.
4. Opcional: rode `LocadoraVeiculos/Scripts/01_povoamento.sql` para carregar a massa de
   dados usada na demonstração.
5. Inicie a aplicação. O Swagger abre em `https://localhost:7297/swagger`.

## Parte 2 - Front-end

### Como executar

1. Suba a API no perfil HTTP:
   ```
   dotnet run --project LocadoraVeiculos --launch-profile http
   ```
   Ela fica disponível em `http://localhost:5207`. Esse endereço está definido em
   `frontend/js/api.js`.
2. Abra a pasta `frontend/` no VS Code e clique em **Go Live** (extensão Live Server), ou
   clique com o botão direito em `index.html` e escolha **Open with Live Server**.
3. O navegador abre em `http://127.0.0.1:5500`. O CORS da API já libera essa origem
   (e também `http://localhost:5500`).

### Telas

- **Início** (`index.html`): cards de resumo (frota, disponíveis, aluguéis em aberto,
  faturamento) e lista de aluguéis em aberto.
- **Fabricantes** (`fabricantes.html`): cadastro, edição e exclusão; filtro por nome e país.
- **Categorias** (`categorias.html`): cadastro, edição e exclusão; filtro por nome e faixa
  de diária.
- **Veículos** (`veiculos.html`): cadastro, edição e exclusão; filtro por fabricante,
  categoria, situação e ano mínimo.
- **Clientes** (`clientes.html`): cadastro, edição e exclusão; filtro por nome, CPF e
  "somente sem aluguel".
- **Aluguéis** (`alugueis.html`): novo aluguel, edição, exclusão e devolução; filtro por
  cliente, status e período de retirada.
- **Relatórios** (`relatorios.html`): faturamento por categoria, devoluções em atraso,
  veículos nunca alugados e clientes x aluguéis.

### Rotas usadas por tela

| Tela | Rotas |
|---|---|
| Início | `GET /api/veiculos`<br>`GET /api/filtros/alugueis-em-aberto`<br>`GET /api/filtros/faturamento-por-categoria` |
| Fabricantes | `GET /api/fabricantes`<br>`POST /api/fabricantes`<br>`PUT /api/fabricantes/{id}`<br>`DELETE /api/fabricantes/{id}` |
| Categorias | `GET /api/categoriasveiculo`<br>`POST /api/categoriasveiculo`<br>`PUT /api/categoriasveiculo/{id}`<br>`DELETE /api/categoriasveiculo/{id}` |
| Veículos | `GET /api/filtros/veiculos`<br>`GET /api/fabricantes`<br>`GET /api/categoriasveiculo`<br>`GET /api/veiculos/{id}`<br>`POST /api/veiculos`<br>`PUT /api/veiculos/{id}`<br>`DELETE /api/veiculos/{id}` |
| Clientes | `GET /api/clientes`<br>`GET /api/filtros/clientes-e-alugueis`<br>`POST /api/clientes`<br>`PUT /api/clientes/{id}`<br>`DELETE /api/clientes/{id}` |
| Aluguéis | `GET /api/alugueis`<br>`GET /api/alugueis/{id}`<br>`POST /api/alugueis`<br>`PUT /api/alugueis/{id}`<br>`DELETE /api/alugueis/{id}`<br>`POST /api/alugueis/{id}/devolucao`<br>`GET /api/clientes`<br>`GET /api/filtros/veiculos?disponivel=true` |
| Relatórios | `GET /api/filtros/faturamento-por-categoria`<br>`GET /api/filtros/devolucoes-em-atraso`<br>`GET /api/filtros/veiculos-nunca-alugados`<br>`GET /api/filtros/clientes-e-alugueis` |

### Wireframes

**Início**

![Início](<Início@1x.png>)

**Fabricantes**

![Fabricantes](<Fabricantes@1x.png>)

**Categorias**

![Categorias](<Categorias@1x.png>)

**Veículos**

![Veículos](<Veículos@1x.png>)

**Clientes**

![Clientes](<Clientes@1x.png>)

**Aluguéis**

![Aluguéis](<Aluguéis@1x.png>)

**Relatórios**

![Relatórios](<Relatórios@2x.png>)

**Modal de cadastro e edição**

![Modal de cadastro e edição](<Modal · cadastro edição@1x.png>)

**Modal de confirmação de exclusão**

![Modal de confirmação de exclusão](<Modal · confirmar exclusão@1x.png>)

**Modal de novo aluguel**

![Modal de novo aluguel](<Modal · novo aluguel@1x.png>)

**Modal de devolução**

![Modal de devolução](<Modal · devolução@1x.png>)
