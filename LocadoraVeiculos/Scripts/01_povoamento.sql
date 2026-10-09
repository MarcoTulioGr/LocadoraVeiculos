
--  LOCADORA DE VEICULOS - POVOAMENTO DO BANCO
--  Etapa 2 do escopo: criacao de registros

USE LocadoraVeiculosDb;
GO

-- Limpa tudo e reinicia a numeracao (permite rodar o script mais de uma vez)
DELETE FROM Alugueis;
DELETE FROM Veiculos;
DELETE FROM Clientes;
DELETE FROM CategoriasVeiculo;
DELETE FROM Fabricantes;
GO

DBCC CHECKIDENT ('Alugueis', RESEED, 0);
DBCC CHECKIDENT ('Veiculos', RESEED, 0);
DBCC CHECKIDENT ('Clientes', RESEED, 0);
DBCC CHECKIDENT ('CategoriasVeiculo', RESEED, 0);
DBCC CHECKIDENT ('Fabricantes', RESEED, 0);
GO

-- FABRICANTES
SET IDENTITY_INSERT Fabricantes ON;
INSERT INTO Fabricantes (Id, Nome, PaisOrigem) VALUES
 (1, N'Fiat',       N'Italia'),
 (2, N'Volkswagen', N'Alemanha'),
 (3, N'Toyota',     N'Japao'),
 (4, N'Chevrolet',  N'Estados Unidos'),
 (5, N'Hyundai',    N'Coreia do Sul');
SET IDENTITY_INSERT Fabricantes OFF;
GO

-- CATEGORIAS DE VEICULO (tabela de precos vigente)
SET IDENTITY_INSERT CategoriasVeiculo ON;
INSERT INTO CategoriasVeiculo (Id, Nome, Descricao, ValorDiariaBase) VALUES
 (1, N'Hatch',  N'Compactos de dois volumes, economicos para cidade', 120.00),
 (2, N'Sedan',  N'Tres volumes, porta-malas amplo',                   180.00),
 (3, N'SUV',    N'Utilitarios esportivos, maior altura do solo',      250.00),
 (4, N'Picape', N'Veiculos de carga leve com cacamba',                300.00),
 (5, N'Luxo',   N'Alto padrao, motorizacao superior',                 500.00);
SET IDENTITY_INSERT CategoriasVeiculo OFF;
GO

-- VEICULOS
--  Disponivel = 0 nos carros que estao alugados agora
SET IDENTITY_INSERT Veiculos ON;
INSERT INTO Veiculos (Id, Placa, Modelo, AnoFabricacao, Quilometragem, Cor, Disponivel, FabricanteId, CategoriaVeiculoId) VALUES
 ( 1, N'ABC1D23', N'Mobi Like',           2022, 45230, N'Branco',   1, 1, 1),
 ( 2, N'BCD2E34', N'Argo Drive',          2023, 28100, N'Prata',    1, 1, 1),
 ( 3, N'CDE3F45', N'Cronos Precision',    2023, 31500, N'Preto',    0, 1, 2),
 ( 4, N'DEF4G56', N'Polo Track',          2024, 12400, N'Cinza',    1, 2, 1),
 ( 5, N'EFG5H67', N'Virtus Comfortline',  2023, 39800, N'Branco',   0, 2, 2),
 ( 6, N'FGH6I78', N'T-Cross Sense',       2024, 18700, N'Vermelho', 1, 2, 3),
 ( 7, N'GHI7J89', N'Corolla XEi',         2023, 27600, N'Prata',    1, 3, 2),
 ( 8, N'HIJ8K90', N'Corolla Cross XRE',   2024, 15200, N'Preto',    0, 3, 3),
 ( 9, N'IJK9L01', N'Onix LT',             2022, 52300, N'Branco',   1, 4, 1),
 (10, N'JKL0M12', N'Tracker Premier',     2023, 33400, N'Cinza',    1, 4, 3),
 (11, N'KLM1N23', N'S10 High Country',    2024, 21800, N'Preto',    0, 4, 4),
 (12, N'LMN2O34', N'Creta Ultimate',      2024,  9800, N'Azul',     1, 5, 5);
SET IDENTITY_INSERT Veiculos OFF;
GO

-- CLIENTES
SET IDENTITY_INSERT Clientes ON;
INSERT INTO Clientes (Id, Nome, Cpf, Email, Telefone, NumeroCnh, DataNascimento) VALUES
 (1, N'Marco Tulio Ribeiro',  N'123.456.789-01', N'marco.ribeiro@email.com',  N'(31) 98765-4321', N'01234567890', '1998-03-14'),
 (2, N'Ana Beatriz Souza',    N'234.567.890-12', N'ana.souza@email.com',      N'(11) 97654-3210', N'12345678901', '1991-07-22'),
 (3, N'Carlos Eduardo Lima',  N'345.678.901-23', N'carlos.lima@email.com',    N'(21) 96543-2109', N'23456789012', '1985-11-05'),
 (4, N'Daniela Martins',      N'456.789.012-34', N'daniela.martins@email.com',N'(41) 95432-1098', N'34567890123', '1994-01-30'),
 (5, N'Eduardo Farias',       N'567.890.123-45', N'eduardo.farias@email.com', N'(51) 94321-0987', N'45678901234', '1979-09-17'),
 (6, N'Fernanda Alves',       N'678.901.234-56', N'fernanda.alves@email.com', N'(61) 93210-9876', N'56789012345', '2000-05-08'),
 (7, N'Gustavo Pereira',      N'789.012.345-67', N'gustavo.pereira@email.com',N'(71) 92109-8765', N'67890123456', '1988-12-19'),
 (8, N'Helena Castro',        N'890.123.456-78', N'helena.castro@email.com',  N'(81) 91098-7654', N'78901234567', '1996-04-02');
SET IDENTITY_INSERT Clientes OFF;
GO

-- ALUGUEIS
--  Os 8 primeiros estao encerrados (tem devolucao)
--  Os 4 ultimos estao em aberto (DataDevolucaoReal = NULL)
SET IDENTITY_INSERT Alugueis ON;
INSERT INTO Alugueis (Id, ClienteId, VeiculoId, DataRetirada, DataPrevistaDevolucao, DataDevolucaoReal, KmInicial, KmFinal, ValorDiaria, ValorTotal, Observacoes) VALUES
 ( 1, 1,  1, '2026-06-01', '2026-06-06', '2026-06-06', 44250, 45230, 120.00,  600.00, N'Devolucao no prazo'),
 ( 2, 2,  4, '2026-06-10', '2026-06-13', '2026-06-14', 11650, 12400, 120.00,  480.00, N'Devolvido com 1 dia de atraso'),
 ( 3, 3,  7, '2026-06-15', '2026-06-22', '2026-06-22', 26200, 27600, 180.00, 1260.00, NULL),
 ( 4, 4, 10, '2026-07-01', '2026-07-05', '2026-07-05', 32510, 33400, 250.00, 1000.00, NULL),
 ( 5, 5,  2, '2026-07-08', '2026-07-11', '2026-07-11', 27550, 28100, 120.00,  360.00, NULL),
 ( 6, 6, 12, '2026-07-20', '2026-07-23', '2026-07-23',  9180,  9800, 500.00, 1500.00, N'Cliente solicitou entrega no aeroporto'),
 ( 7, 7,  9, '2026-08-02', '2026-08-09', '2026-08-09', 51100, 52300, 120.00,  840.00, NULL),
 ( 8, 8,  6, '2026-08-12', '2026-08-15', '2026-08-15', 18020, 18700, 250.00,  750.00, NULL),
 ( 9, 1,  3, '2026-09-10', '2026-09-20', NULL,         31500, NULL,  180.00, NULL,    N'Locacao em andamento'),
 (10, 3,  5, '2026-09-12', '2026-09-19', NULL,         39800, NULL,  180.00, NULL,    N'Locacao em andamento'),
 (11, 5,  8, '2026-09-14', '2026-09-21', NULL,         15200, NULL,  250.00, NULL,    N'Locacao em andamento'),
 (12, 2, 11, '2026-09-15', '2026-09-25', NULL,         21800, NULL,  300.00, NULL,    N'Locacao em andamento');
SET IDENTITY_INSERT Alugueis OFF;
GO

-- CONFERENCIA
SELECT 'Fabricantes' AS Tabela, COUNT(*) AS Registros FROM Fabricantes
UNION ALL SELECT 'CategoriasVeiculo', COUNT(*) FROM CategoriasVeiculo
UNION ALL SELECT 'Veiculos',          COUNT(*) FROM Veiculos
UNION ALL SELECT 'Clientes',          COUNT(*) FROM Clientes
UNION ALL SELECT 'Alugueis',          COUNT(*) FROM Alugueis;
GO
