let veiculos = [];
let idEmEdicao = null;
let idParaExcluir = null;

const modalForm    = new bootstrap.Modal(document.getElementById("modalForm"));
const modalExcluir = new bootstrap.Modal(document.getElementById("modalExcluir"));
const form         = document.getElementById("formVeiculo");

async function carregarOpcoes() {
  const [fabricantes, categorias] = await Promise.all([
    apiGet("/fabricantes"),
    apiGet("/categoriasveiculo"),
  ]);

  document.getElementById("filtroFabricante").innerHTML =
    `<option value="">Todos</option>` +
    fabricantes.map(f => `<option value="${escapar(f.nome)}">${escapar(f.nome)}</option>`).join("");
  document.getElementById("filtroCategoria").innerHTML =
    `<option value="">Todas</option>` +
    categorias.map(c => `<option value="${escapar(c.nome)}">${escapar(c.nome)}</option>`).join("");

  document.getElementById("campoFabricante").innerHTML =
    `<option value="">Selecione...</option>` +
    fabricantes.map(f => `<option value="${f.id}">${escapar(f.nome)}</option>`).join("");
  document.getElementById("campoCategoria").innerHTML =
    `<option value="">Selecione...</option>` +
    categorias.map(c => `<option value="${c.id}">${escapar(c.nome)} (${formatarMoeda(c.valorDiariaBase)}/dia)</option>`).join("");
}

async function carregar() {
  try {
    const parametros = new URLSearchParams();
    const fabricante = document.getElementById("filtroFabricante").value;
    const categoria  = document.getElementById("filtroCategoria").value;
    const disponivel = document.getElementById("filtroDisponivel").value;
    const anoMinimo  = document.getElementById("filtroAno").value;

    if (fabricante) parametros.append("fabricante", fabricante);
    if (categoria)  parametros.append("categoria", categoria);
    if (disponivel) parametros.append("disponivel", disponivel);
    if (anoMinimo)  parametros.append("anoMinimo", anoMinimo);

    const query = parametros.toString();
    const rota  = "/filtros/veiculos" + (query ? "?" + query : "");
    document.getElementById("rotaUsada").textContent = "GET /api" + rota;

    const resposta = await apiGet(rota);
    veiculos = resposta.dados;
    renderizar();
  } catch (erro) {
    mostrarMensagem(erro.message, "danger");
  }
}

function renderizar() {
  const tbody = document.getElementById("tabela");
  tbody.innerHTML = veiculos.length === 0
    ? `<tr><td colspan="9" class="text-center text-secondary">Nenhum veículo encontrado.</td></tr>`
    : veiculos.map(v => `
      <tr>
        <td><strong>${escapar(v.placa)}</strong></td>
        <td>${escapar(v.modelo)}</td>
        <td>${escapar(v.fabricante)}</td>
        <td>${escapar(v.categoria)}</td>
        <td>${v.anoFabricacao}</td>
        <td>${v.quilometragem.toLocaleString("pt-BR")}</td>
        <td>${formatarMoeda(v.valorDiariaBase)}</td>
        <td>${v.disponivel
              ? `<span class="badge border text-dark">Disponível</span>`
              : `<span class="badge bg-dark">Alugado</span>`}</td>
        <td class="text-end text-nowrap">
          <button class="btn btn-sm btn-outline-dark" onclick="abrirEdicao(${v.id})">Editar</button>
          <button class="btn btn-sm btn-outline-danger" onclick="pedirExclusao(${v.id})">Excluir</button>
        </td>
      </tr>`).join("");

  document.getElementById("contador").textContent = `${veiculos.length} veículo(s)`;
}

function limparFiltros() {
  ["filtroFabricante", "filtroCategoria", "filtroDisponivel", "filtroAno"]
    .forEach(id => document.getElementById(id).value = "");
  carregar();
}

["filtroFabricante", "filtroCategoria", "filtroDisponivel", "filtroAno"]
  .forEach(id => document.getElementById(id).addEventListener("change", carregar));

function abrirNovo() {
  idEmEdicao = null;
  form.reset();
  form.classList.remove("was-validated");
  document.getElementById("tituloModal").textContent = "Novo veículo";
  modalForm.show();
}

async function abrirEdicao(id) {
  try {
    const v = await apiGet(`/veiculos/${id}`);  
    idEmEdicao = id;
    form.classList.remove("was-validated");
    document.getElementById("campoPlaca").value      = v.placa;
    document.getElementById("campoModelo").value     = v.modelo;
    document.getElementById("campoAno").value        = v.anoFabricacao;
    document.getElementById("campoKm").value         = v.quilometragem;
    document.getElementById("campoCor").value        = v.cor ?? "";
    document.getElementById("campoFabricante").value = v.fabricanteId;
    document.getElementById("campoCategoria").value  = v.categoriaVeiculoId;
    document.getElementById("campoDisponivel").checked = v.disponivel;
    document.getElementById("tituloModal").textContent = `Editar veículo ${v.placa}`;
    modalForm.show();
  } catch (erro) {
    mostrarMensagem(erro.message, "danger");
  }
}

form.addEventListener("submit", async (evento) => {
  evento.preventDefault();
  if (!form.checkValidity()) {
    form.classList.add("was-validated");
    return;
  }

  const corpo = {
    placa: document.getElementById("campoPlaca").value.trim().toUpperCase(),
    modelo: document.getElementById("campoModelo").value.trim(),
    anoFabricacao: Number(document.getElementById("campoAno").value),
    quilometragem: Number(document.getElementById("campoKm").value),
    cor: document.getElementById("campoCor").value.trim() || null,
    fabricanteId: Number(document.getElementById("campoFabricante").value),
    categoriaVeiculoId: Number(document.getElementById("campoCategoria").value),
    disponivel: document.getElementById("campoDisponivel").checked,
  };

  try {
    if (idEmEdicao === null) {
      await apiPost("/veiculos", corpo);
      mostrarMensagem("Veículo cadastrado com sucesso.");
    } else {
      await apiPut(`/veiculos/${idEmEdicao}`, { id: idEmEdicao, ...corpo });
      mostrarMensagem("Veículo atualizado com sucesso.");
    }
    carregar();
  } catch (erro) {
    mostrarMensagem(erro.message, "danger");
  }
  modalForm.hide();
});

function pedirExclusao(id) {
  const v = veiculos.find(x => x.id === id);
  idParaExcluir = id;
  document.getElementById("nomeExcluir").textContent = `${v.modelo} (${v.placa})`;
  modalExcluir.show();
}

async function confirmarExclusao() {
  try {
    await apiDelete(`/veiculos/${idParaExcluir}`);
    mostrarMensagem("Veículo excluído com sucesso.");
    carregar();
  } catch (erro) {
    mostrarMensagem(erro.message, "danger");
  }
  modalExcluir.hide();
}


async function iniciar() {
  try {
    await carregarOpcoes();  
    await carregar();
  } catch (erro) {
    mostrarMensagem(erro.message, "danger");
  }
}

iniciar();