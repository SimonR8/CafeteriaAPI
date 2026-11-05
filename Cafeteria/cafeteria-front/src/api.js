// Aquí están todas las llamadas al API de C#.
const URL_BASE = "http://localhost:5101/api";

// El token se conserva en el navegador para adjuntarlo a las peticiones protegidas.
let tokenActual = localStorage.getItem("cafeteria_token");
export function guardarToken(token) {
  tokenActual = token;
  if (token) localStorage.setItem("cafeteria_token", token);
  else localStorage.removeItem("cafeteria_token");
}

function armarMensaje(datos) {
  if (typeof datos === "string") return datos;
  if (datos && datos.errors) return Object.values(datos.errors).flat().join(" ");
  return "Ocurrió un error inesperado.";
}

async function pedir(ruta, metodo = "GET", cuerpo = null) {
  const headers = {};
  if (cuerpo !== null) headers["Content-Type"] = "application/json";
  if (tokenActual) headers.Authorization = `Bearer ${tokenActual}`;
  const opciones = { method: metodo, headers };
  if (cuerpo !== null) opciones.body = JSON.stringify(cuerpo);

  let respuesta;
  try {
    respuesta = await fetch(URL_BASE + ruta, opciones);
  } catch (error) {
    throw new Error("No se pudo conectar con el API. ¿Está ejecutándose en Visual Studio?");
  }

  const texto = await respuesta.text();
  let datos = texto;
  try { datos = JSON.parse(texto); } catch (error) { /* La respuesta era texto. */ }
  if (!respuesta.ok) throw new Error(armarMensaje(datos));
  return datos;
}

// ---------- Autenticación ----------
export async function registrarUsuario(datos) {
  const respuesta = await pedir("/auth/registro", "POST", datos);
  guardarToken(respuesta.token);
  return respuesta;
}
export async function iniciarSesion(datos) {
  const respuesta = await pedir("/auth/login", "POST", datos);
  guardarToken(respuesta.token);
  return respuesta;
}

// ---------- Productos ----------
export function obtenerProductos() { return pedir("/productos"); }
export function crearProducto(producto) { return pedir("/productos", "POST", producto); }
export function actualizarProducto(id, producto) { return pedir("/productos/" + id, "PUT", producto); }
export function eliminarProducto(id) { return pedir("/productos/" + id, "DELETE"); }

// ---------- Clientes ----------
export function obtenerClientes() { return pedir("/clientes"); }
export function crearCliente(cliente) { return pedir("/clientes", "POST", cliente); }
export function actualizarCliente(id, cliente) { return pedir("/clientes/" + id, "PUT", cliente); }
export function eliminarCliente(id) { return pedir("/clientes/" + id, "DELETE"); }

// ---------- Pedidos ----------
export function obtenerPedidos() { return pedir("/pedidos"); }
export function crearPedido(pedido) { return pedir("/pedidos", "POST", pedido); }
export function cambiarEstadoPedido(id, estado) { return pedir("/pedidos/" + id + "/estado", "PUT", { estado: estado }); }
