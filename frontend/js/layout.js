const PAGINAS = [
  { titulo: "Início",      arquivo: "index.html" },
  { titulo: "Fabricantes", arquivo: "fabricantes.html" },
  { titulo: "Categorias",  arquivo: "categorias.html" },
  { titulo: "Veículos",    arquivo: "veiculos.html" },
  { titulo: "Clientes",    arquivo: "clientes.html" },
  { titulo: "Aluguéis",    arquivo: "alugueis.html" },
  { titulo: "Relatórios",  arquivo: "relatorios.html" },
];

function montarMenu() {
  const atual = location.pathname.split("/").pop() || "index.html";

  const links = PAGINAS.map(p =>
    `<a href="${p.arquivo}" class="${p.arquivo === atual ? "ativo" : ""}">${p.titulo}</a>`
  ).join("");

  document.getElementById("menu").innerHTML =
    `<div class="marca">🚗 Locadora</div>${links}`;
}

montarMenu();