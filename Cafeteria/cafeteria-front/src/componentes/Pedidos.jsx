import { useState, useEffect } from "react";
import { obtenerPedidos, obtenerProductos, obtenerClientes, crearPedido, cambiarEstadoPedido } from "../api.js";
import { dinero, fecha } from "./formato.js";

function Pedidos() {
  const [pedidos, setPedidos] = useState([]);
  const [productos, setProductos] = useState([]);
  const [clientes, setClientes] = useState([]);

  // Datos del pedido que se está armando
  const [clienteId, setClienteId] = useState("");
  const [items, setItems] = useState([]);
  const [productoId, setProductoId] = useState("");
  const [cantidad, setCantidad] = useState(1);

  const [error, setError] = useState("");
  const [mensaje, setMensaje] = useState("");

  async function cargar() {
    try {
      setPedidos(await obtenerPedidos());
      setProductos(await obtenerProductos());
      setClientes(await obtenerClientes());
    } catch (e) {
      setError(e.message);
    }
  }

  useEffect(() => {
    cargar();
  }, []);

  function agregarItem() {
    if (productoId === "" || Number(cantidad) < 1) {
      return;
    }

    // Si el producto ya estaba en la lista, solo sumamos la cantidad
    const existente = items.find((i) => i.productoId === Number(productoId));
    if (existente) {
      setItems(
        items.map((i) =>
          i.productoId === Number(productoId) ? { ...i, cantidad: i.cantidad + Number(cantidad) } : i
        )
      );
    } else {
      setItems([...items, { productoId: Number(productoId), cantidad: Number(cantidad) }]);
    }

    setProductoId("");
    setCantidad(1);
  }

  function quitarItem(id) {
    setItems(items.filter((i) => i.productoId !== id));
  }

  function nombreDe(id) {
    const producto = productos.find((p) => p.id === id);
    return producto ? producto.nombre : "Producto " + id;
  }

  function precioDe(id) {
    const producto = productos.find((p) => p.id === id);
    return producto ? producto.precio : 0;
  }

  // Total estimado (sin descuento): el API calcula el total real
  let totalEstimado = 0;
  for (const item of items) {
    totalEstimado = totalEstimado + precioDe(item.productoId) * item.cantidad;
  }

  async function enviarPedido() {
    setError("");
    setMensaje("");

    if (clienteId === "") {
      setError("Primero escoge un cliente.");
      return;
    }

    try {
      const pedido = await crearPedido({ clienteId: Number(clienteId), items: items });
      setMensaje("Pedido #" + pedido.id + " creado. Total: " + dinero(pedido.total));
      setItems([]);
      setClienteId("");
      cargar();
    } catch (e) {
      setError(e.message);
    }
  }

  async function cambiarEstado(pedido, nuevoEstado) {
    setError("");
    setMensaje("");
    try {
      await cambiarEstadoPedido(pedido.id, nuevoEstado);
      setMensaje("Pedido #" + pedido.id + " ahora está " + nuevoEstado + ".");
      cargar();
    } catch (e) {
      setError(e.message);
    }
  }

  return (
    <section>
      <h2>Pedidos</h2>

      {error && <p className="mensaje error">{error}</p>}
      {mensaje && <p className="mensaje ok">{mensaje}</p>}

      <div className="tarjeta formulario">
        <h3>Nuevo pedido</h3>

        <label>
          Cliente
          <select value={clienteId} onChange={(e) => setClienteId(e.target.value)}>
            <option value="">-- Escoge un cliente --</option>
            {clientes.map((c) => (
              <option key={c.id} value={c.id}>
                {c.nombre} ({c.puntos} puntos)
              </option>
            ))}
          </select>
        </label>

        <div className="fila agregar">
          <label>
            Producto
            <select value={productoId} onChange={(e) => setProductoId(e.target.value)}>
              <option value="">-- Escoge un producto --</option>
              {productos.map((p) => (
                <option key={p.id} value={p.id}>
                  {p.nombre} - {dinero(p.precio)} (quedan {p.stock})
                </option>
              ))}
            </select>
          </label>
          <label>
            Cantidad
            <input type="number" min="1" value={cantidad} onChange={(e) => setCantidad(e.target.value)} />
          </label>
          <button type="button" className="secundario" onClick={agregarItem}>
            Agregar
          </button>
        </div>

        {items.length > 0 && (
          <ul className="lista-items">
            {items.map((i) => (
              <li key={i.productoId}>
                {i.cantidad} x {nombreDe(i.productoId)} — {dinero(precioDe(i.productoId) * i.cantidad)}
                <button type="button" className="peligro pequeno" onClick={() => quitarItem(i.productoId)}>
                  Quitar
                </button>
              </li>
            ))}
          </ul>
        )}

        <div className="botones">
          <button type="button" onClick={enviarPedido} disabled={items.length === 0}>
            Crear pedido
          </button>
          {items.length > 0 && <span className="total-estimado">Total sin descuento: {dinero(totalEstimado)}</span>}
        </div>
      </div>

      <h3>Pedidos hechos</h3>
      {pedidos.length === 0 && <p>Todavía no hay pedidos.</p>}

      <div className="rejilla">
        {[...pedidos].reverse().map((pedido) => (
          <article className="tarjeta pedido" key={pedido.id}>
            <div className="pedido-encabezado">
              <strong>Pedido #{pedido.id}</strong>
              <span className={"estado " + pedido.estado.toLowerCase()}>{pedido.estado}</span>
            </div>
            <p className="pequeno-texto">
              {pedido.nombreCliente} · {fecha(pedido.fecha)}
            </p>

            <ul>
              {pedido.detalles.map((d) => (
                <li key={d.productoId}>
                  {d.cantidad} x {d.nombreProducto} — {dinero(d.subtotal)}
                </li>
              ))}
            </ul>

            <div className="totales">
              <span>Subtotal: {dinero(pedido.subtotal)}</span>
              {pedido.descuento > 0 && <span>Descuento: -{dinero(pedido.descuento)}</span>}
              <strong>Total: {dinero(pedido.total)}</strong>
            </div>

            <div className="botones">
              {pedido.estado === "Pendiente" && (
                <button onClick={() => cambiarEstado(pedido, "Preparando")}>Preparar</button>
              )}
              {pedido.estado === "Preparando" && (
                <button onClick={() => cambiarEstado(pedido, "Entregado")}>Entregar</button>
              )}
              {(pedido.estado === "Pendiente" || pedido.estado === "Preparando") && (
                <button className="peligro" onClick={() => cambiarEstado(pedido, "Cancelado")}>
                  Cancelar
                </button>
              )}
            </div>
          </article>
        ))}
      </div>
    </section>
  );
}

export default Pedidos;
