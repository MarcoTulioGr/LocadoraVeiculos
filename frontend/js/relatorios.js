function linhaVazia(colunas, texto) {
  return `<tr><td colspan="${colunas}" class="text-center text-secondary">${texto}</td></tr>`;
}

function renderizarFaturamento(resposta) {
  const dados = resposta.dados;
  const maior = Math.max(...dados.map(d => d.faturamento), 0);

  // barras com progress do bootstrap, largura proporcional ao maior faturamento
  document.getElementById("graficoFaturamento").innerHTML = dados.length === 0
    ? `<p class="text-secondary m-0">Nenhum aluguel encerrado ainda.</p>`
    : dados.map(d => {
        const largura = maior > 0 ? (d.faturamento / maior) * 100 : 0;
        return `
        <div class="mb-2">
          <div class="d-flex justify-content-between small">
            <span>${escapar(d.categoria)}</span>
            <strong>${formatarMoeda(d.faturamento)}</strong>
          </div>
          <div class="progress" role="progressbar" aria-label="${escapar(d.categoria)}">
            <div class="progress-bar bg-secondary" style="width: ${largura.toFixed(1)}%"></div>
          </div>
        </div>`;
      }).join("");

  document.getElementById("tabelaFaturamento").innerHTML = dados.length === 0
    ? linhaVazia(4, "Nenhum aluguel encerrado ainda.")
    : dados.map(d => `
      <tr>
        <td>${escapar(d.categoria)}</td>
        <td>${d.qtdeLocacoes}</td>
        <td>${formatarMoeda(d.faturamento)}</td>
        <td>${d.kmRodados.toLocaleString("pt-BR")}</td>
      </tr>`).join("");

  document.getElementById("faturamentoGeral").textContent = formatarMoeda(resposta.faturamentoGeral);
}

function renderizarAtrasos(resposta) {
  document.getElementById("totalAtrasos").textContent = resposta.total;
  document.getElementById("tabelaAtrasos").innerHTML = resposta.dados.length === 0
    ? linhaVazia(8, "Nenhuma devolução em atraso.")
    : resposta.dados.map(d => `
      <tr>
        <td>${d.aluguelId}</td>
        <td>${escapar(d.cliente)}</td>
        <td>${escapar(d.email)}</td>
        <td>${escapar(d.modelo)} · ${escapar(d.placa)}</td>
        <td>${formatarData(d.dataPrevistaDevolucao)}</td>
        <td>${formatarData(d.dataDevolucaoReal)}</td>
        <td><span class="badge bg-dark">${d.diasAtraso} dia(s)</span></td>
        <td>${d.valorTotal != null ? formatarMoeda(d.valorTotal) : "—"}</td>
      </tr>`).join("");
}

function renderizarOciosos(resposta) {
  document.getElementById("totalOciosos").textContent = resposta.total;
  document.getElementById("tabelaOciosos").innerHTML = resposta.dados.length === 0
    ? linhaVazia(7, "Lista vazia: todos os veículos da frota já foram alugados pelo menos uma vez.")
    : resposta.dados.map(v => `
      <tr>
        <td><strong>${escapar(v.placa)}</strong></td>
        <td>${escapar(v.modelo)}</td>
        <td>${escapar(v.fabricante)}</td>
        <td>${escapar(v.categoria)}</td>
        <td>${v.anoFabricacao}</td>
        <td>${v.quilometragem.toLocaleString("pt-BR")}</td>
        <td>${formatarMoeda(v.valorDiariaBase)}</td>
      </tr>`).join("");
}

function rotaClientes() {
  const somenteSem = document.getElementById("filtroSemAluguel").checked;
  const rota = "/filtros/clientes-e-alugueis" + (somenteSem ? "?somenteSemAluguel=true" : "");
  document.getElementById("rotaClientes").textContent = "GET /api" + rota;
  return rota;
}

function renderizarClientes(resposta) {
  document.getElementById("totalClientes").textContent = resposta.total;
  document.getElementById("tabelaClientes").innerHTML = resposta.dados.length === 0
    ? linhaVazia(6, "Nenhum cliente encontrado.")
    : resposta.dados.map(c => `
      <tr>
        <td>${escapar(c.nome)}</td>
        <td>${escapar(c.cpf)}</td>
        <td>${escapar(c.email)}</td>
        <td>${c.totalAlugueis}</td>
        <td>${c.alugueisEmAberto > 0 ? `<span class="badge bg-dark">${c.alugueisEmAberto}</span>` : "0"}</td>
        <td>${formatarMoeda(c.valorGastoTotal)}</td>
      </tr>`).join("");
}

async function carregarClientes() {
  try {
    renderizarClientes(await apiGet(rotaClientes()));
  } catch (erro) {
    mostrarMensagem(erro.message, "danger");
  }
}

document.getElementById("filtroSemAluguel").addEventListener("change", carregarClientes);

async function carregarRelatorios() {
  try {
    const [faturamento, atrasos, ociosos, clientes] = await Promise.all([
      apiGet("/filtros/faturamento-por-categoria"),
      apiGet("/filtros/devolucoes-em-atraso"),
      apiGet("/filtros/veiculos-nunca-alugados"),
      apiGet(rotaClientes()),
    ]);

    renderizarFaturamento(faturamento);
    renderizarAtrasos(atrasos);
    renderizarOciosos(ociosos);
    renderizarClientes(clientes);
  } catch (erro) {
    mostrarMensagem(erro.message, "danger");
  }
}

carregarRelatorios();
