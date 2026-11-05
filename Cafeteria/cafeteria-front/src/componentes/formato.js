// Funciones pequeñas para mostrar dinero y fechas bonitos

export function dinero(valor) {
  return "$" + Number(valor).toLocaleString("es-CO");
}

export function fecha(texto) {
  return new Date(texto).toLocaleString("es-CO", { dateStyle: "short", timeStyle: "short" });
}
