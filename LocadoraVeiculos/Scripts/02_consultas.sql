--  LOCADORA DE VEICULOS - CONSULTAS DE VERIFICACAO
--  Etapa 2 do escopo: execucao de consultas
--  Estas consultas viram as 5 rotas de filtro da API

USE LocadoraVeiculosDb;
GO

-- 1. INNER JOIN (3 tabelas)
--    Frota completa com marca e categoria
SELECT  v.Placa,
        v.Modelo,
        f.Nome  AS Fabricante,
        c.Nome  AS Categoria,
        v.AnoFabricacao,
        v.Quilometragem,
        c.ValorDiariaBase,
        CASE WHEN v.Disponivel = 1 THEN 'Sim' ELSE 'Nao' END AS Disponivel
FROM        Veiculos          v
INNER JOIN  Fabricantes       f ON f.Id = v.FabricanteId
INNER JOIN  CategoriasVeiculo c ON c.Id = v.CategoriaVeiculoId
ORDER BY    f.Nome, v.Modelo;
GO

-- 2. LEFT JOIN
--    Todos os clientes, inclusive os que nunca alugaram
SELECT      cl.Nome,
            cl.Cpf,
            cl.Email,
            COUNT(a.Id)            AS TotalAlugueis,
            ISNULL(SUM(a.ValorTotal), 0) AS ValorGastoTotal
FROM        Clientes cl
LEFT JOIN   Alugueis a ON a.ClienteId = cl.Id
GROUP BY    cl.Nome, cl.Cpf, cl.Email
ORDER BY    ValorGastoTotal DESC;
GO

-- 3. INNER JOIN com filtro
--    Alugueis em aberto (veiculo ainda nao devolvido)
SELECT      a.Id            AS AluguelId,
            cl.Nome         AS Cliente,
            cl.Telefone,
            v.Placa,
            v.Modelo,
            a.DataRetirada,
            a.DataPrevistaDevolucao,
            DATEDIFF(DAY, a.DataRetirada, a.DataPrevistaDevolucao) AS DiasContratados,
            a.ValorDiaria
FROM        Alugueis a
INNER JOIN  Clientes cl ON cl.Id = a.ClienteId
INNER JOIN  Veiculos v  ON v.Id  = a.VeiculoId
WHERE       a.DataDevolucaoReal IS NULL
ORDER BY    a.DataPrevistaDevolucao;
GO

-- 4. INNER JOIN com agregacao
--    Faturamento e quilometragem rodada por categoria
SELECT      c.Nome                      AS Categoria,
            COUNT(a.Id)                 AS QtdeLocacoes,
            SUM(a.ValorTotal)           AS Faturamento,
            SUM(a.KmFinal - a.KmInicial) AS KmRodados
FROM        Alugueis          a
INNER JOIN  Veiculos          v ON v.Id = a.VeiculoId
INNER JOIN  CategoriasVeiculo c ON c.Id = v.CategoriaVeiculoId
WHERE       a.DataDevolucaoReal IS NOT NULL
GROUP BY    c.Nome
ORDER BY    Faturamento DESC;
GO

-- 5. LEFT JOIN
--    Veiculos que nunca foram alugados
SELECT      v.Placa,
            v.Modelo,
            f.Nome AS Fabricante,
            v.AnoFabricacao
FROM        Veiculos    v
INNER JOIN  Fabricantes f ON f.Id = v.FabricanteId
LEFT JOIN   Alugueis    a ON a.VeiculoId = v.Id
WHERE       a.Id IS NULL
ORDER BY    v.Modelo;
GO

-- 6. Devolucoes em atraso
SELECT      cl.Nome AS Cliente,
            v.Placa,
            a.DataPrevistaDevolucao,
            a.DataDevolucaoReal,
            DATEDIFF(DAY, a.DataPrevistaDevolucao, a.DataDevolucaoReal) AS DiasAtraso
FROM        Alugueis a
INNER JOIN  Clientes cl ON cl.Id = a.ClienteId
INNER JOIN  Veiculos v  ON v.Id  = a.VeiculoId
WHERE       a.DataDevolucaoReal > a.DataPrevistaDevolucao
ORDER BY    DiasAtraso DESC;
GO
