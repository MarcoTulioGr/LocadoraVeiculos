let alugueis = [];
let veiculosDisponiveis = [];
let aluguelEmEdicao = null;
let idParaExcluir = null;

const modalForm    = new bootstrap.Modal(document.getElementById("modalForm"));
const modalExcluir = new bootstrap.Modal(document.getElementById("modalExcluir"));
const form         = document.getElementById("formAluguel");

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

// novo / edicao
function ajustarMinimoPrevista() {
  const retirada = document.getElementById("campoRetirada").value;
  document.getElementById("campoPrevista").min = retirada ? somarDias(retirada, 1) : "";
}

function atualizarResumo() {
  const retirada = document.getElementById("campoRetirada").value;
  const prevista = document.getElementById("campoPrevista").value;
  const diaria   = Number(document.getElementById("campoDiaria").value);
  const resumo   = document.getElementById("resumoAluguel");

  if (!retirada || !prevista || !diaria) {
    resumo.textContent = "Preencha as datas e a diária para ver o valor estimado.";
    return;
  }
  if (prevista <= retirada) {
    resumo.textContent = "A devolução prevista deve ser depois da retirada.";
    return;
  }

  const dias = diasEntre(retirada, prevista);
  resumo.innerHTML = `${dias} diária(s) × ${formatarMoeda(diaria)} = <strong>${formatarMoeda(dias * diaria)}</strong> estimado`;
}

async function abrirNovo() {
  try {
    const [clientes, disponiveis] = await Promise.all([
      apiGet("/clientes"),
      apiGet("/filtros/veiculos?disponivel=true"),
    ]);
    veiculosDisponiveis = disponiveis.dados;

    document.getElementById("campoCliente").innerHTML =
      `<option value="">Selecione...</option>` +
      clientes.map(c => `<option value="${c.id}">${escapar(c.nome)}</option>`).join("");
    document.getElementById("campoVeiculo").innerHTML = veiculosDisponiveis.length === 0
      ? `<option value="">Nenhum veículo disponível</option>`
      : `<option value="">Selecione...</option>` +
        veiculosDisponiveis.map(v =>
          `<option value="${v.id}">${escapar(v.modelo)} · ${escapar(v.placa)} (${escapar(v.categoria)})</option>`).join("");
  } catch (erro) {
    mostrarMensagem(erro.message, "danger");
    return;
  }

  aluguelEmEdicao = null;
  form.reset();
  form.classList.remove("was-validated");
  document.getElementById("campoCliente").disabled = false;
  document.getElementById("campoVeiculo").disabled = false;
  document.getElementById("campoRetirada").value = hojeISO();
  document.getElementById("campoPrevista").value = somarDias(hojeISO(), 3);
  ajustarMinimoPrevista();
  atualizarResumo();
  document.getElementById("tituloModal").textContent = "Novo aluguel";
  modalForm.show();
}

async function abrirEdicao(id) {
  try {
    const a = await apiGet(`/alugueis/${id}`);
    // guarda o aluguel sem cliente e veiculo para reenviar no put
    const { cliente, veiculo, ...resto } = a;
    aluguelEmEdicao = resto;

    form.classList.remove("was-validated");
    document.getElementById("campoCliente").innerHTML =
      `<option value="${a.clienteId}">${escapar(cliente?.nome ?? "Cliente " + a.clienteId)}</option>`;
    document.getElementById("campoVeiculo").innerHTML =
      `<option value="${a.veiculoId}">${escapar(veiculo?.modelo ?? "Veículo")} · ${escapar(veiculo?.placa ?? a.veiculoId)}</option>`;
    document.getElementById("campoCliente").disabled = true;
    document.getElementById("campoVeiculo").disabled = true;
    document.getElementById("campoRetirada").value    = a.dataRetirada.substring(0, 10);
    document.getElementById("campoPrevista").value    = a.dataPrevistaDevolucao.substring(0, 10);
    document.getElementById("campoDiaria").value      = a.valorDiaria;
    document.getElementById("campoObservacoes").value = a.observacoes ?? "";
    ajustarMinimoPrevista();
    atualizarResumo();
    document.getElementById("tituloModal").textContent = `Editar aluguel #${id}`;
    modalForm.show();
  } catch (erro) {
    mostrarMensagem(erro.message, "danger");
  }
}

document.getElementById("campoVeiculo").addEventListener("change", () => {
  const v = veiculosDisponiveis.find(x => x.id === Number(document.getElementById("campoVeiculo").value));
  if (v) document.getElementById("campoDiaria").value = v.valorDiariaBase;
  atualizarResumo();
});

document.getElementById("campoRetirada").addEventListener("change", () => {
  ajustarMinimoPrevista();
  atualizarResumo();
});
document.getElementById("campoPrevista").addEventListener("change", atualizarResumo);
document.getElementById("campoDiaria").addEventListener("input", atualizarResumo);

form.addEventListener("submit", async (evento) => {
  evento.preventDefault();
  if (!form.checkValidity()) {
    form.classList.add("was-validated");
    return;
  }

  const retirada    = document.getElementById("campoRetirada").value;
  const prevista    = document.getElementById("campoPrevista").value;
  const diaria      = Number(document.getElementById("campoDiaria").value);
  const observacoes = document.getElementById("campoObservacoes").value.trim() || null;

  try {
    if (aluguelEmEdicao === null) {
      // a api pega o km do veiculo, entao kmInicial vai 0
      await apiPost("/alugueis", {
        clienteId: Number(document.getElementById("campoCliente").value),
        veiculoId: Number(document.getElementById("campoVeiculo").value),
        dataRetirada: retirada,
        dataPrevistaDevolucao: prevista,
        kmInicial: 0,
        valorDiaria: diaria,
        observacoes,
      });
      mostrarMensagem("Aluguel registrado com sucesso.");
    } else {
      const a = aluguelEmEdicao;
      // put substitui tudo, entao manda o objeto inteiro trocando so o que foi editado.
      // se a data nao mudou, mantem o horario original
      const corpo = {
        ...a,
        dataRetirada: retirada === a.dataRetirada.substring(0, 10) ? a.dataRetirada : retirada,
        dataPrevistaDevolucao: prevista === a.dataPrevistaDevolucao.substring(0, 10) ? a.dataPrevistaDevolucao : prevista,
        valorDiaria: diaria,
        observacoes,
      };
      await apiPut(`/alugueis/${a.id}`, corpo);
      mostrarMensagem("Aluguel atualizado com sucesso.");
    }
    carregar();
  } catch (erro) {
    mostrarMensagem(erro.message, "danger");
  }
  modalForm.hide();
});

// exclusao
function pedirExclusao(id) {
  const a = alugueis.find(x => x.id === id);
  idParaExcluir = id;
  document.getElementById("nomeExcluir").textContent =
    `#${a.id} (${a.cliente.nome} · ${a.veiculo.modelo} ${a.veiculo.placa})`;
  document.getElementById("avisoExcluir").textContent = statusDe(a) === "encerrado"
    ? ""
    : `Este aluguel está em aberto: ao excluir, o veículo ${a.veiculo.placa} volta a ficar disponível.`;
  modalExcluir.show();
}

async function confirmarExclusao() {
  try {
    await apiDelete(`/alugueis/${idParaExcluir}`);
    mostrarMensagem("Aluguel excluído com sucesso.");
    carregar();
  } catch (erro) {
    mostrarMensagem(erro.message, "danger");
  }
  modalExcluir.hide();
}

carregar();