async function carregarInicio() {
  try {
    const [veiculos, abertos, faturamento] = await Promise.all([
      apiGet("/veiculos"),
      apiGet("/filtros/alugueis-em-aberto"),
      apiGet("/filtros/faturamento-por-categoria"),
    ]);


    document.getElementById("cardFrota").textContent       = veiculos.length;
    document.getElementById("cardDisponiveis").textContent = veiculos.filter(v => v.disponivel).length;
    document.getElementById("cardAbertos").textContent     = abertos.total;
    document.getElementById("cardFaturamento").textContent = formatarMoeda(faturamento.faturamentoGeral);

  
    const tbody = document.getElementById("tabelaAbertos");

    if (abertos.dados.length === 0) {
      tbody.innerHTML = `<tr><td colspan="7" class="text-center text-secondary">Nenhum aluguel em aberto.</td></tr>`;
      return;
    }

    tbody.innerHTML = abertos.dados.map(a => `
      <tr>
        <td>${a.aluguelId}</td>
        <td>${escapar(a.cliente)}</td>
        <td>${escapar(a.telefone)}</td>
        <td>${escapar(a.modelo)} · ${escapar(a.placa)}</td>
        <td>${formatarData(a.dataRetirada)}</td>
        <td>${formatarData(a.dataPrevistaDevolucao)}</td>
        <td>${a.atrasado
              ? `<span class="badge bg-dark">Atrasado ${a.diasDeAtraso} dia(s)</span>`
              : `<span class="badge border text-dark">No prazo</span>`}</td>
      </tr>`).join("");

  } catch (erro) {
    mostrarMensagem(erro.message, "danger");
  }
}

carregarInicio();