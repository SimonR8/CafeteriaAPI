import { useState } from "react";
import { iniciarSesion, registrarUsuario } from "../api.js";

export default function Autenticacion({ onAutenticado }) {
  const [modoRegistro, setModoRegistro] = useState(false);
  const [nombre, setNombre] = useState("");
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [mensaje, setMensaje] = useState("");
  const [cargando, setCargando] = useState(false);

  async function enviar(e) {
    e.preventDefault();
    setMensaje("");
    setCargando(true);
    try {
      const respuesta = modoRegistro
        ? await registrarUsuario({ nombre, username, password })
        : await iniciarSesion({ username, password });
      onAutenticado(respuesta.usuario);
    } catch (error) {
      setMensaje(error.message);
    } finally {
      setCargando(false);
    }
  }

  return (
    <div className="auth-pagina">
      <form className="auth-tarjeta" onSubmit={enviar}>
        <h1>Cafetería</h1>
        <h2>{modoRegistro ? "Crear cuenta" : "Iniciar sesión"}</h2>
        <p>{modoRegistro ? "Regístrate para acceder al sistema." : "Ingresa tus credenciales para continuar."}</p>
        {modoRegistro && (
          <label>Nombre completo
            <input value={nombre} onChange={e => setNombre(e.target.value)} required minLength="2" autoComplete="name" />
          </label>
        )}
        <label>Usuario
          <input value={username} onChange={e => setUsername(e.target.value)} required minLength="3" autoComplete="username" />
        </label>
        <label>Contraseña
          <input type="password" value={password} onChange={e => setPassword(e.target.value)} required minLength="6" autoComplete={modoRegistro ? "new-password" : "current-password"} />
        </label>
        {mensaje && <div className="mensaje error">{mensaje}</div>}
        <button type="submit" disabled={cargando}>{cargando ? "Procesando..." : modoRegistro ? "Registrarme" : "Ingresar"}</button>
        <button type="button" className="secundario auth-cambiar" onClick={() => { setModoRegistro(!modoRegistro); setMensaje(""); }}>
          {modoRegistro ? "Ya tengo una cuenta: iniciar sesión" : "¿No tienes cuenta? Regístrate"}
        </button>
      </form>
    </div>
  );
}
