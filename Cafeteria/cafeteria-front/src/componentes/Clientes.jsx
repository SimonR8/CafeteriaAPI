import { useState, useEffect } from "react";
import { obtenerClientes, crearCliente, actualizarCliente, eliminarCliente } from "../api.js";

const FORMULARIO_VACIO = { id: null, nombre: "", telefono: "" };

function Clientes() {
  const [clientes, setClientes] = useState([]);
  const [formulario, setFormulario] = useState(FORMULARIO_VACIO);
  const [error, setError] = useState("");
  const [mensaje, setMensaje] = useState("");

  async function cargar() {
    try {
      setClientes(await obtenerClientes());
    } catch (e) {
      setError(e.message);
    }
  }

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

    const datos = { nombre: formulario.nombre, telefono: formulario.telefono };

    try {
      if (formulario.id === null) {
        await crearCliente(datos);
        setMensaje("Cliente creado.");
      } else {
        await actualizarCliente(formulario.id, datos);
        setMensaje("Cliente actualizado.");
      }
      setFormulario(FORMULARIO_VACIO);
      cargar();
    } catch (e) {
      setError(e.message);
    }
  }

  async function borrar(cliente) {
    if (!window.confirm("¿Eliminar a " + cliente.nombre + "?")) {
      return;
    }
    setError("");
    setMensaje("");
    try {
      await eliminarCliente(cliente.id);
      setMensaje("Cliente eliminado.");
      cargar();
    } catch (e) {
      setError(e.message);
    }
  }

  return (
    <section>
      <h2>Clientes</h2>

      {error && <p className="mensaje error">{error}</p>}
      {mensaje && <p className="mensaje ok">{mensaje}</p>}

      <form className="tarjeta formulario" onSubmit={guardar}>
        <h3>{formulario.id === null ? "Nuevo cliente" : "Editando cliente #" + formulario.id}</h3>
        <div className="fila">
          <label>
            Nombre
            <input name="nombre" value={formulario.nombre} onChange={cambiarCampo} />
          </label>
          <label>
            Teléfono
            <input name="telefono" value={formulario.telefono} onChange={cambiarCampo} />
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

      <table className="tabla">
        <thead>
          <tr>
            <th>Nombre</th>
            <th>Teléfono</th>
            <th>Puntos</th>
            <th>Descuento</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {clientes.map((c) => (
            <tr key={c.id}>
              <td>{c.nombre}</td>
              <td>{c.telefono}</td>
              <td>{c.puntos}</td>
              <td>{c.puntos >= 100 ? "10%" : "—"}</td>
              <td className="acciones">
                <button className="secundario" onClick={() => setFormulario({ id: c.id, nombre: c.nombre, telefono: c.telefono })}>
                  Editar
                </button>
                <button className="peligro" onClick={() => borrar(c)}>
                  Eliminar
                </button>
              </td>
            </tr>
          ))}
          {clientes.length === 0 && (
            <tr>
              <td colSpan="5">No hay clientes para mostrar.</td>
            </tr>
          )}
        </tbody>
      </table>
    </section>
  );
}

export default Clientes;
