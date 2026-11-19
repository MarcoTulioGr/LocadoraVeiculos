let alugueis = [];

// lista
async function carregar() {
  try {
    alugueis = await apiGet("/alugueis");
    renderizar();
  } catch (erro) {
    mostrarMensagem(erro.message, "danger");
  }
}

// atrasado = ainda aberto e com a prevista antes de hoje
function statusDe(a) {
  if (a.dataDevolucaoReal) return "encerrado";
  return a.dataPrevistaDevolucao.substring(0, 10) < hojeISO() ? "atrasado" : "aberto";
}

function badgeStatus(status) {
  if (status === "encerrado") return `<span class="badge border text-dark">Encerrado</span>`;
  if (status === "atrasado")  return `<span class="badge bg-dark">Atrasado</span>`;
  return `<span class="badge bg-secondary">Em aberto</span>`;
}

function renderizar() {
  const cliente = normalizar(document.getElementById("filtroCliente").value);
  const status  = document.getElementById("filtroStatus").value;
  const de      = document.getElementById("filtroDe").value;
  const ate     = document.getElementById("filtroAte").value;

  const lista = alugueis.filter(a => {
    const st = statusDe(a);
    const retirada = a.dataRetirada.substring(0, 10);
    return normalizar(a.cliente.nome).includes(cliente)
      && (status === "" || st === status || (status === "aberto" && st === "atrasado"))
      && (de  === "" || retirada >= de)
      && (ate === "" || retirada <= ate);
  });

  const tbody = document.getElementById("tabela");
  tbody.innerHTML = lista.length === 0
    ? `<tr><td colspan="9" class="text-center text-secondary">Nenhum aluguel encontrado.</td></tr>`
    : lista.map(a => {
        const st = statusDe(a);
        return `
        <tr>
          <td>${a.id}</td>
          <td>${escapar(a.cliente.nome)}</td>
          <td>${escapar(a.veiculo.modelo)} · ${escapar(a.veiculo.placa)}</td>
          <td>${formatarData(a.dataRetirada)}</td>
          <td>${formatarData(a.dataPrevistaDevolucao)}</td>
          <td>${formatarData(a.dataDevolucaoReal)}</td>
          <td>${badgeStatus(st)}</td>
          <td>${a.valorTotal != null ? formatarMoeda(a.valorTotal) : "—"}</td>
          <td class="text-end text-nowrap">
            ${st !== "encerrado"
              ? `<button class="btn btn-sm btn-dark" onclick="abrirDevolucao(${a.id})">Devolver</button>` : ""}
            <button class="btn btn-sm btn-outline-dark" onclick="abrirEdicao(${a.id})">Editar</button>
            <button class="btn btn-sm btn-outline-danger" onclick="pedirExclusao(${a.id})">Excluir</button>
          </td>
        </tr>`;
      }).join("");

  document.getElementById("contador").textContent = `${lista.length} de ${alugueis.length} registro(s)`;
}

function limparFiltros() {
  ["filtroCliente", "filtroStatus", "filtroDe", "filtroAte"]
    .forEach(id => document.getElementById(id).value = "");
  renderizar();
}

["filtroCliente", "filtroStatus", "filtroDe", "filtroAte"].forEach(id => {
  document.getElementById(id).addEventListener("input", renderizar);
  document.getElementById(id).addEventListener("change", renderizar);
});

carregar();