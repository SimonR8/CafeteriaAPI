import { useState, useEffect } from "react";
import { obtenerProductos, crearProducto, actualizarProducto, eliminarProducto } from "../api.js";
import { dinero } from "./formato.js";

const FORMULARIO_VACIO = { id: null, nombre: "", categoria: "", precio: "", stock: "" };

function Productos() {
  const [productos, setProductos] = useState([]);
  const [formulario, setFormulario] = useState(FORMULARIO_VACIO);
  const [busqueda, setBusqueda] = useState("");
  const [error, setError] = useState("");
  const [mensaje, setMensaje] = useState("");

  async function cargar() {
    try {
      setProductos(await obtenerProductos());
    } catch (e) {
      setError(e.message);
    }
  }

  // Se ejecuta una sola vez, cuando aparece la pantalla
  useEffect(() => {
    cargar();
  }, []);

  function cambiarCampo(evento) {
    setFormulario({ ...formulario, [evento.target.name]: evento.target.value });
  }

  async function guardar(evento) {
    evento.preventDefault();
    setError("");
    setMensaje("");

    const datos = {
      nombre: formulario.nombre,
      categoria: formulario.categoria,
      precio: Number(formulario.precio),
      stock: Number(formulario.stock),
    };

    try {
      if (formulario.id === null) {
        await crearProducto(datos);
        setMensaje("Producto creado.");
      } else {
        await actualizarProducto(formulario.id, datos);
        setMensaje("Producto actualizado.");
      }
      setFormulario(FORMULARIO_VACIO);
      cargar();
    } catch (e) {
      setError(e.message);
    }
  }

  async function borrar(producto) {
    if (!window.confirm("¿Eliminar " + producto.nombre + "?")) {
      return;
    }
    setError("");
    setMensaje("");
    try {
      await eliminarProducto(producto.id);
      setMensaje("Producto eliminado.");
      cargar();
    } catch (e) {
      setError(e.message);
    }
  }

  const productosFiltrados = productos.filter((p) =>
    (p.nombre + " " + p.categoria).toLowerCase().includes(busqueda.toLowerCase())
  );

  return (
    <section>
      <h2>Productos</h2>

      {error && <p className="mensaje error">{error}</p>}
      {mensaje && <p className="mensaje ok">{mensaje}</p>}

      <form className="tarjeta formulario" onSubmit={guardar}>
        <h3>{formulario.id === null ? "Nuevo producto" : "Editando producto #" + formulario.id}</h3>
        <div className="fila">
          <label>
            Nombre
            <input name="nombre" value={formulario.nombre} onChange={cambiarCampo} />
          </label>
          <label>
            Categoría
            <input name="categoria" value={formulario.categoria} onChange={cambiarCampo} placeholder="Ej: Bebida caliente" />
          </label>
          <label>
            Precio
            <input name="precio" type="number" value={formulario.precio} onChange={cambiarCampo} />
          </label>
          <label>
            Stock
            <input name="stock" type="number" value={formulario.stock} onChange={cambiarCampo} />
          </label>
        </div>
        <div className="botones">
          <button type="submit">{formulario.id === null ? "Crear" : "Guardar cambios"}</button>
          {formulario.id !== null && (
            <button type="button" className="secundario" onClick={() => setFormulario(FORMULARIO_VACIO)}>
              Cancelar
            </button>
          )}
        </div>
      </form>

      <input
        className="buscador"
        placeholder="Buscar por nombre o categoría..."
        value={busqueda}
        onChange={(e) => setBusqueda(e.target.value)}
      />

      <table className="tabla">
        <thead>
          <tr>
            <th>Nombre</th>
            <th>Categoría</th>
            <th>Precio</th>
            <th>Stock</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {productosFiltrados.map((p) => (
            <tr key={p.id}>
              <td>{p.nombre}</td>
              <td>{p.categoria}</td>
              <td>{dinero(p.precio)}</td>
              <td className={p.stock <= 5 ? "poco" : ""}>{p.stock}</td>
              <td className="acciones">
                <button className="secundario" onClick={() => setFormulario({ ...p })}>
                  Editar
                </button>
                <button className="peligro" onClick={() => borrar(p)}>
                  Eliminar
                </button>
              </td>
            </tr>
          ))}
          {productosFiltrados.length === 0 && (
            <tr>
              <td colSpan="5">No hay productos para mostrar.</td>
            </tr>
          )}
        </tbody>
      </table>
    </section>
  );
}

export default Productos;
