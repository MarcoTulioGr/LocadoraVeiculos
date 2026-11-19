let clientes = [];
let idEmEdicao = null;
let idParaExcluir = null;

const modalForm    = new bootstrap.Modal(document.getElementById("modalForm"));
const modalExcluir = new bootstrap.Modal(document.getElementById("modalExcluir"));
const form         = document.getElementById("formCliente");

async function carregar() {
  try {
    const somenteSem = document.getElementById("filtroSemAluguel").checked;
    const rotaFiltro = "/filtros/clientes-e-alugueis" + (somenteSem ? "?somenteSemAluguel=true" : "");
    document.getElementById("rotaUsada").textContent = "GET /api" + rotaFiltro;

    // Busca as duas rotas ao mesmo tempo
    const [completos, resumo] = await Promise.all([
      apiGet("/clientes"),
      apiGet(rotaFiltro),
    ]);

    clientes = resumo.dados.map(r => ({
      ...completos.find(c => c.id === r.id),
      totalAlugueis: r.totalAlugueis,
      alugueisEmAberto: r.alugueisEmAberto,
      valorGastoTotal: r.valorGastoTotal,
    }));

    renderizar();
  } catch (erro) {
    mostrarMensagem(erro.message, "danger");
  }
}

function renderizar() {
  const nome = normalizar(document.getElementById("filtroNome").value);
  const cpf = document.getElementById("filtroCpf").value.replace(/\D/g, "");

  const lista = clientes.filter(c =>
    normalizar(c.nome).includes(nome) &&
    c.cpf.replace(/\D/g, "").includes(cpf)
  );

  const tbody = document.getElementById("tabela");
  tbody.innerHTML = lista.length === 0
    ? `<tr><td colspan="7" class="text-center text-secondary">Nenhum cliente encontrado.</td></tr>`
    : lista.map(c => `
      <tr>
        <td>${escapar(c.nome)}</td>
        <td>${escapar(c.cpf)}</td>
        <td>${escapar(c.email)}</td>
        <td>${escapar(c.telefone ?? "—")}</td>
        <td>${c.totalAlugueis}${c.alugueisEmAberto > 0
              ? ` <span class="badge bg-dark">${c.alugueisEmAberto} em aberto</span>` : ""}</td>
        <td>${formatarMoeda(c.valorGastoTotal)}</td>
        <td class="text-end text-nowrap">
          <button class="btn btn-sm btn-outline-dark" onclick="abrirEdicao(${c.id})">Editar</button>
          <button class="btn btn-sm btn-outline-danger" onclick="pedirExclusao(${c.id})">Excluir</button>
        </td>
      </tr>`).join("");

  document.getElementById("contador").textContent = `${lista.length} de ${clientes.length} registro(s)`;
}

function limparFiltros() {
  document.getElementById("filtroNome").value = "";
  document.getElementById("filtroCpf").value = "";
  document.getElementById("filtroSemAluguel").checked = false;
  carregar();
}

document.getElementById("filtroNome").addEventListener("input", renderizar);
document.getElementById("filtroCpf").addEventListener("input", renderizar);
// O checkbox chama a API de novo (filtro no servidor)
document.getElementById("filtroSemAluguel").addEventListener("change", carregar);

function abrirNovo() {
  idEmEdicao = null;
  form.reset();
  form.classList.remove("was-validated");
  document.getElementById("tituloModal").textContent = "Novo cliente";
  modalForm.show();
}

function abrirEdicao(id) {
  const c = clientes.find(x => x.id === id);
  idEmEdicao = id;
  form.classList.remove("was-validated");
  document.getElementById("campoNome").value       = c.nome;
  document.getElementById("campoCpf").value        = c.cpf;
  document.getElementById("campoEmail").value      = c.email;
  document.getElementById("campoTelefone").value   = c.telefone ?? "";
  document.getElementById("campoCnh").value        = c.numeroCnh ?? "";
  document.getElementById("campoNascimento").value = c.dataNascimento ? c.dataNascimento.substring(0, 10) : "";
  document.getElementById("tituloModal").textContent = `Editar cliente #${id}`;
  modalForm.show();
}

form.addEventListener("submit", async (evento) => {
  evento.preventDefault();
  if (!form.checkValidity()) {
    form.classList.add("was-validated");
    return;
  }

  const valor = id => document.getElementById(id).value.trim();
  const corpo = {
    nome: valor("campoNome"),
    cpf: valor("campoCpf"),
    email: valor("campoEmail"),
    telefone: valor("campoTelefone") || null,
    numeroCnh: valor("campoCnh") || null,
    dataNascimento: valor("campoNascimento") || null,
  };

  try {
    if (idEmEdicao === null) {
      await apiPost("/clientes", corpo);
      mostrarMensagem("Cliente cadastrado com sucesso.");
    } else {
      await apiPut(`/clientes/${idEmEdicao}`, { id: idEmEdicao, ...corpo });
      mostrarMensagem("Cliente atualizado com sucesso.");
    }
    carregar();
  } catch (erro) {
    mostrarMensagem(erro.message, "danger");
  }
  modalForm.hide();
});

function pedirExclusao(id) {
  idParaExcluir = id;
  document.getElementById("nomeExcluir").textContent = clientes.find(x => x.id === id).nome;
  modalExcluir.show();
}

async function confirmarExclusao() {
  try {
    await apiDelete(`/clientes/${idParaExcluir}`);
    mostrarMensagem("Cliente excluído com sucesso.");
    carregar();
  } catch (erro) {
    mostrarMensagem(erro.message, "danger");
  }
  modalExcluir.hide();
}

carregar();